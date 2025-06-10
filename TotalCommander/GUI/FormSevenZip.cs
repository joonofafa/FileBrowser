using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TotalCommander.Compression;

namespace TotalCommander.GUI
{
    public partial class FormSevenZip : Form
    {
        #region 필드

        private readonly SevenZipCompression _sevenZip;
        private CancellationTokenSource _cancellationTokenSource;
        private Progress<int> _progressReporter;
        private bool _operationInProgress = false;

        public string[] FileList { get; set; }
        public string DestArchive { get; set; }
        
        /// <summary>
        /// 압축 작업이 완료되었을 때 발생하는 이벤트
        /// </summary>
        public event EventHandler CompressionCompleted;

        #endregion

        #region 생성자

        public FormSevenZip()
        {
            InitializeComponent();

            // SevenZipCompression 인스턴스 생성
            _sevenZip = new SevenZipCompression();
            
            // 작업 완료 이벤트 핸들러 등록
            _sevenZip.OperationCompleted += SevenZip_OperationCompleted;

            // 진행 상황 리포터 초기화
            _progressReporter = new Progress<int>(percent =>
            {
                progressBar1.Value = percent;
                lblStatus.Text = $"{StringResources.GetString("CompressionProgress")} {percent}%";
            });

            // 이벤트 핸들러 등록
            FormClosed += (s, e) => { _cancellationTokenSource?.Cancel(); };
            btnOpenSaveDialog.Click += btnArchiveOpenFolder_Click;
            btnOK.Click += btnOK_Click;
            btnCancel.Click += btnCancel_Click;
            FormClosing += FormSevenZip_FormClosing;
            Load += FormSevenZip_Load;
        }

        public FormSevenZip(string[] filePaths) : this()
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

        #region 폼 이벤트

        private void FormSevenZip_Load(object sender, EventArgs e)
        {
            txtFileList.Text = FileList != null ? string.Join(Environment.NewLine, FileList) : string.Empty;
            txtArchivePath.Text = DestArchive != null ? DestArchive : string.Empty;

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            InitCompressionLevelComboBox();
            InitArchiveFormatComboBox();

            // UI 초기화
            progressBar1.Visible = false;
            lblStatus.Visible = false;
            _operationInProgress = false;
        }

        private void FormSevenZip_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_operationInProgress)
            {
                DialogResult result = MessageBox.Show(
                    StringResources.GetString("CancelOperationConfirm"),
                    StringResources.GetString("Confirmation"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _cancellationTokenSource?.Cancel();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnArchiveOpenFolder_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // 선택된 압축 형식에 따라 필터 설정
                ArchiveFormat format = (ArchiveFormat)((ComboBoxItem)cboArchiveFormat.SelectedItem).Value;
                string filter;

                switch (format)
                {
                    case ArchiveFormat.Zip:
                        filter = "ZIP 파일 (*.zip)|*.zip";
                        break;
                    case ArchiveFormat.SevenZip:
                        filter = "7Z 파일 (*.7z)|*.7z";
                        break;
                    case ArchiveFormat.Tar:
                        filter = "TAR 파일 (*.tar)|*.tar";
                        break;
                    case ArchiveFormat.GZip:
                        filter = "GZIP 파일 (*.gz)|*.gz";
                        break;
                    case ArchiveFormat.BZip2:
                        filter = "BZIP2 파일 (*.bz2)|*.bz2";
                        break;
                    case ArchiveFormat.XZ:
                        filter = "XZ 파일 (*.xz)|*.xz";
                        break;
                    default:
                        filter = "ZIP 파일 (*.zip)|*.zip";
                        break;
                }

                saveFileDialog.Filter = $"{filter}|모든 파일 (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = GetDefaultFileName(format);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtArchivePath.Text = saveFileDialog.FileName;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtArchivePath.Text))
            {
                MessageBox.Show(
                    StringResources.GetString("ZipFileRequired"),
                    StringResources.GetString("Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (FileList == null || FileList.Length == 0)
            {
                MessageBox.Show(
                    StringResources.GetString("NoFilesToCompress"),
                    StringResources.GetString("Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // 압축 옵션 설정
            CompressionLevel level = (CompressionLevel)((ComboBoxItem)cboCompressionLevel.SelectedItem).Value;
            ArchiveFormat format = (ArchiveFormat)((ComboBoxItem)cboArchiveFormat.SelectedItem).Value;

            // UI 업데이트
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            lblStatus.Text = StringResources.GetString("Compressing");
            lblStatus.Visible = true;
            _operationInProgress = true;

            // 압축 작업 시작
            CompressFilesAsync(txtArchivePath.Text, FileList, format, level);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_operationInProgress)
            {
                _cancellationTokenSource?.Cancel();
                _operationInProgress = false;
                lblStatus.Text = StringResources.GetString("Cancelled");
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        #endregion

        #region 압축 메서드

        private async void CompressFilesAsync(string archivePath, string[] files, ArchiveFormat format, CompressionLevel level)
        {
            try
            {
                // 다른 컨트롤 비활성화
                SetControlsEnabled(false);
                btnCancel.Enabled = true;

                // 취소 토큰 생성
                _cancellationTokenSource = new CancellationTokenSource();

                // 압축 작업 시작
                bool success = await _sevenZip.CompressAsync(
                    archivePath,
                    files,
                    format,
                    level,
                    _progressReporter,
                    _cancellationTokenSource.Token);

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    lblStatus.Text = StringResources.GetString("Cancelled");
                }
                else if (success)
                {
                    lblStatus.Text = StringResources.GetString("CompressionCompleted");
                    
                    // 압축 완료 후 대화상자 종료
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    lblStatus.Text = StringResources.GetString("CompressionFailed");
                    MessageBox.Show(
                        StringResources.GetString("CompressionError"),
                        StringResources.GetString("Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = StringResources.GetString("CompressionFailed");
                MessageBox.Show(
                    $"{StringResources.GetString("CompressionError")}: {ex.Message}",
                    StringResources.GetString("Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // 작업 상태 업데이트
                _operationInProgress = false;
                
                // 컨트롤 활성화
                SetControlsEnabled(true);
            }
        }

        #endregion

        #region 헬퍼 메서드

        private void InitCompressionLevelComboBox()
        {
            cboCompressionLevel.Items.Clear();

            ComboBoxItem[] arrItems = new ComboBoxItem[5];

            // 압축 레벨 옵션 추가
            arrItems[0] = new ComboBoxItem
            {
                Text = StringResources.GetString("NoCompression"),
                Value = (int)CompressionLevel.None
            };

            arrItems[1] = new ComboBoxItem
            {
                Text = StringResources.GetString("FastestCompression"),
                Value = (int)CompressionLevel.Fastest
            };

            arrItems[2] = new ComboBoxItem
            {
                Text = StringResources.GetString("NormalCompression"),
                Value = (int)CompressionLevel.Normal
            };

            arrItems[3] = new ComboBoxItem
            {
                Text = StringResources.GetString("MaximumCompression"),
                Value = (int)CompressionLevel.Maximum
            };

            arrItems[4] = new ComboBoxItem
            {
                Text = StringResources.GetString("UltraCompression"),
                Value = (int)CompressionLevel.Ultra
            };

            cboCompressionLevel.Items.AddRange(arrItems);
            cboCompressionLevel.SelectedIndex = 2; // 기본값: Normal
        }

        private void InitArchiveFormatComboBox()
        {
            cboArchiveFormat.Items.Clear();

            ComboBoxItem[] arrItems = new ComboBoxItem[6];

            // 압축 형식 옵션 추가
            arrItems[0] = new ComboBoxItem
            {
                Text = "ZIP",
                Value = (int)ArchiveFormat.Zip
            };

            arrItems[1] = new ComboBoxItem
            {
                Text = "7Z",
                Value = (int)ArchiveFormat.SevenZip
            };

            arrItems[2] = new ComboBoxItem
            {
                Text = "TAR",
                Value = (int)ArchiveFormat.Tar
            };

            arrItems[3] = new ComboBoxItem
            {
                Text = "GZ",
                Value = (int)ArchiveFormat.GZip
            };

            arrItems[4] = new ComboBoxItem
            {
                Text = "BZ2",
                Value = (int)ArchiveFormat.BZip2
            };

            arrItems[5] = new ComboBoxItem
            {
                Text = "XZ",
                Value = (int)ArchiveFormat.XZ
            };

            cboArchiveFormat.Items.AddRange(arrItems);
            cboArchiveFormat.SelectedIndex = 0; // 기본값: ZIP
            
            // 형식 변경 이벤트 처리
            cboArchiveFormat.SelectedIndexChanged += (s, e) =>
            {
                // 현재 선택된 형식에 맞게 파일 확장자 업데이트
                if (!string.IsNullOrEmpty(txtArchivePath.Text))
                {
                    ArchiveFormat format = (ArchiveFormat)((ComboBoxItem)cboArchiveFormat.SelectedItem).Value;
                    string dir = Path.GetDirectoryName(txtArchivePath.Text);
                    string newFileName = Path.GetFileNameWithoutExtension(txtArchivePath.Text);
                    string newExt = GetFileExtension(format);
                    txtArchivePath.Text = Path.Combine(dir, newFileName + newExt);
                }
            };
        }

        private string GetDefaultFileName(ArchiveFormat format)
        {
            string ext = GetFileExtension(format);
            return "Archive" + ext;
        }

        private string GetFileExtension(ArchiveFormat format)
        {
            switch (format)
            {
                case ArchiveFormat.Zip:
                    return ".zip";
                case ArchiveFormat.SevenZip:
                    return ".7z";
                case ArchiveFormat.Tar:
                    return ".tar";
                case ArchiveFormat.GZip:
                    return ".gz";
                case ArchiveFormat.BZip2:
                    return ".bz2";
                case ArchiveFormat.XZ:
                    return ".xz";
                default:
                    return ".zip";
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            cboArchiveFormat.Enabled = enabled;
            cboCompressionLevel.Enabled = enabled;
            txtArchivePath.Enabled = enabled;
            btnOpenSaveDialog.Enabled = enabled;
            btnOK.Enabled = enabled;
            btnCancel.Text = enabled ? StringResources.GetString("Cancel") : StringResources.GetString("Stop");
        }

        #endregion

        #region 이벤트 핸들러

        /// <summary>
        /// SevenZipCompression 작업 완료 이벤트 핸들러
        /// </summary>
        private void SevenZip_OperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<OperationCompletedEventArgs>(SevenZip_OperationCompleted), sender, e);
                return;
            }
            
            // 작업 성공 시 CompressionCompleted 이벤트 발생
            if (e.Success && e.IsCompression)
            {
                CompressionCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
} 