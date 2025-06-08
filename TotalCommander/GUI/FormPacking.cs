using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalCommander.GUI
{
    public partial class FormPacking : Form
    {
        #region Constants

        // 압축 레벨 열거형
        private enum CompressionLevel
        {
            NoCompression = 0,
            Fastest = 1,
            Optimal = 9,
            SmallestSize = 9
        }

        // 업데이트 모드 열거형
        private enum UpdateMode
        {
            Add = 0,
            Update = 1,
            Fresh = 2,
            Sync = 3
        }

        #endregion

        #region Fields

        public string[] FileList { get; set; }
        public string DestArchive { get; set; }
        
        #endregion

        #region Constructor

        public FormPacking()
        {
            InitializeComponent();
            FormClosed += (s, e) => { timerProgress.Stop(); };
            btnOpenSaveDialog.Click += btnArchiveOpenFolder_Click;
            btnOK.Click += btnAdd_Click;
            btnCancel.Click += btnCancel_Click;
            FormClosing += FormPacking_FormClosing;
            Load += FormPacking_Load;
        }

        public FormPacking(string[] filePaths) : this()
        {
            FileList = filePaths;
            if (filePaths.Length > 0)
            {
                // 기본 압축 파일 이름 설정 (첫 번째 파일이 있는 디렉토리에 생성)
                string directory = Path.GetDirectoryName(filePaths[0]);
                DestArchive = Path.Combine(directory, "Archive.zip");
            }
        }

        #endregion

        #region Form Events

        private void FormPacking_Load(object sender, EventArgs e)
        {
            txtFileName.Text = FileList != null ? string.Join(Environment.NewLine, FileList) : string.Empty;
            txtFileName.Text = DestArchive != null ? DestArchive : string.Empty;

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            
            InitCompressionLevelComboBox();
            InitArchiveFormatComboBox();
            InitUpdateModeComboBox();
            
            // UI 초기화
            progressBar1.Visible = false;
            lblStatus.Visible = false;
            timerProgress.Enabled = false;
        }

        private void FormPacking_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerProgress.Stop();
        }

        private void btnArchiveOpenFolder_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "ZIP 파일 (*.zip)|*.zip|모든 파일 (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = "archive.zip";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFileName.Text = saveFileDialog.FileName;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show("압축 파일 경로를 지정해주세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (FileList == null || FileList.Length == 0)
            {
                MessageBox.Show("압축할 파일이 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 압축 옵션 설정
            CompressionLevel level = (CompressionLevel)((ComboBoxItem)cboCompressionLevel.SelectedItem).Value;
            
            // 업데이트 모드 설정
            UpdateMode updateMode = (UpdateMode)((ComboBoxItem)cboUpdateMode.SelectedItem).Value;

            // UI 업데이트
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            lblStatus.Text = "압축 중...";
            lblStatus.Visible = true;
            timerProgress.Start();
            
            // 압축 작업 시작
            CompressFiles(txtFileName.Text, FileList, level, updateMode);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            timerProgress.Stop();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region Compression Methods

        private void CompressFiles(string archivePath, string[] files, CompressionLevel level, UpdateMode updateMode)
        {
            try
            {
                // 백그라운드에서 압축 작업 수행
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += (s, e) => 
                {
                    try
                    {
                        // .NET의 압축 기능 사용
                        bool fileExists = File.Exists(archivePath);
                        
                        // 업데이트 모드에 따라 처리
                        if (fileExists && updateMode == UpdateMode.Fresh)
                        {
                            File.Delete(archivePath);
                            fileExists = false;
                        }
                        
                        if (!fileExists)
                        {
                            // 새로운 ZIP 파일 생성
                            using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                            {
                                int count = 0;
                                foreach (string file in files)
                                {
                                    if (File.Exists(file))
                                    {
                                        string entryName = Path.GetFileName(file);
                                        ZipArchiveEntry entry = archive.CreateEntry(entryName, 
                                            (System.IO.Compression.CompressionLevel)level);
                                        
                                        using (Stream entryStream = entry.Open())
                                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                                        {
                                            fs.CopyTo(entryStream);
                                        }
                                        
                                        count++;
                                        int progress = (int)((count / (double)files.Length) * 100);
                                        worker.ReportProgress(progress);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 기존 ZIP 파일 업데이트
                            using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
                            {
                                int count = 0;
                                foreach (string file in files)
                                {
                                    if (File.Exists(file))
                                    {
                                        string entryName = Path.GetFileName(file);
                                        
                                        // 기존 항목 제거 (업데이트 모드일 경우)
                                        ZipArchiveEntry existingEntry = archive.GetEntry(entryName);
                                        if (existingEntry != null)
                                        {
                                            existingEntry.Delete();
                                        }
                                        
                                        // 새 항목 추가
                                        ZipArchiveEntry entry = archive.CreateEntry(entryName, 
                                            (System.IO.Compression.CompressionLevel)level);
                                        
                                        using (Stream entryStream = entry.Open())
                                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                                        {
                                            fs.CopyTo(entryStream);
                                        }
                                        
                                        count++;
                                        int progress = (int)((count / (double)files.Length) * 100);
                                        worker.ReportProgress(progress);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Result = ex;
                    }
                };
                
                worker.ProgressChanged += (s, e) =>
                {
                    // UI 업데이트
                    progressBar1.Value = e.ProgressPercentage;
                    lblStatus.Text = $"압축 중... {e.ProgressPercentage}%";
                };
                
                worker.RunWorkerCompleted += (s, e) =>
                {
                    timerProgress.Stop();
                    
                    if (e.Result is Exception ex)
                    {
                        MessageBox.Show($"압축 중 오류가 발생했습니다: {ex.Message}", 
                                      "압축 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        progressBar1.Visible = false;
                        lblStatus.Visible = false;
                    }
                    else
                    {
                        progressBar1.Value = 100;
                        lblStatus.Text = "압축 완료!";
                        MessageBox.Show("파일 압축이 완료되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                };
                
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"압축 작업을 시작하는 중 오류가 발생했습니다: {ex.Message}", 
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar1.Visible = false;
                lblStatus.Visible = false;
                timerProgress.Stop();
            }
        }

        #endregion

        #region UI Initialization Methods

        private void InitCompressionLevelComboBox()
        {
            cboCompressionLevel.Items.Clear();
            
            ComboBoxItem[] arrItems = new ComboBoxItem[3];
            
            // 압축 레벨 옵션 추가
            ComboBoxItem item = new ComboBoxItem()
            {
                Text = "저압축 (빠름)",
                Value = (int)CompressionLevel.Fastest
            };
            arrItems[0] = item;
            
            item = new ComboBoxItem()
            {
                Text = "보통",
                Value = (int)CompressionLevel.Optimal
            };
            arrItems[1] = item;
            
            item = new ComboBoxItem()
            {
                Text = "최대 압축 (느림)",
                Value = (int)CompressionLevel.SmallestSize
            };
            arrItems[2] = item;
            
            cboCompressionLevel.Items.AddRange(arrItems);
            cboCompressionLevel.SelectedIndex = 1; // 기본값: 보통
        }

        private void InitArchiveFormatComboBox()
        {
            cboArchiveFormat.Items.Clear();
            
            // ZIP 형식만 지원
            ComboBoxItem item = new ComboBoxItem()
            {
                Text = "ZIP",
                Value = 0
            };
            cboArchiveFormat.Items.Add(item);
            cboArchiveFormat.SelectedIndex = 0;
            cboArchiveFormat.Enabled = false; // 현재는 ZIP만 지원하므로 비활성화
        }

        private void InitUpdateModeComboBox()
        {
            cboUpdateMode.Items.Clear();
            
            ComboBoxItem[] cboItems = new ComboBoxItem[3];
            
            ComboBoxItem item = new ComboBoxItem()
            {
                Text = "기존 파일에 추가",
                Value = (int)UpdateMode.Add
            };
            cboItems[0] = item;
            
            item = new ComboBoxItem()
            {
                Text = "기존 파일 업데이트",
                Value = (int)UpdateMode.Update
            };
            cboItems[1] = item;
            
            item = new ComboBoxItem()
            {
                Text = "아카이브 새로 생성",
                Value = (int)UpdateMode.Fresh
            };
            cboItems[2] = item;
            
            cboUpdateMode.Items.AddRange(cboItems);
            cboUpdateMode.SelectedIndex = 0; // 기본값: 추가
        }

        #endregion

        // 타이머 이벤트 핸들러
        private void timerProgress_Tick(object sender, EventArgs e)
        {
            // 실제 진행 상황은 백그라운드 워커에서 업데이트됨
        }
    }
}
