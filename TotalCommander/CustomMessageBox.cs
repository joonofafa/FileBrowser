using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TotalCommander
{
    /// <summary>
    /// 부모 폼 중앙에 표시되는 커스텀 메시지 박스 클래스
    /// </summary>
    public class CustomMessageBox : Form
    {
        private Label lblMessage;
        private Button btnOK;
        private Button btnCancel;
        private Button btnYes;
        private Button btnNo;
        private PictureBox pictureIcon;
        private Panel panelButtons;
        private bool closeResult = false;

        private const int CP_NOCLOSE_BUTTON = 0x200;

        // 커스텀 메시지 박스 생성자
        private CustomMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = caption;
            this.ClientSize = new Size(350, 150);
            this.ShowInTaskbar = false;

            // 아이콘 설정
            pictureIcon = new PictureBox();
            pictureIcon.Size = new Size(32, 32);
            pictureIcon.Location = new Point(15, 15);
            SetMessageBoxIcon(icon);
            
            // 메시지 라벨 설정
            lblMessage = new Label();
            lblMessage.AutoSize = true;
            lblMessage.MaximumSize = new Size(280, 0);
            lblMessage.Location = new Point(60, 15);
            lblMessage.Text = message;
            
            // 버튼 패널 설정
            panelButtons = new Panel();
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Height = 45;
            
            // 버튼 추가
            AddButtons(buttons);
            
            // 컨트롤 추가
            this.Controls.Add(pictureIcon);
            this.Controls.Add(lblMessage);
            this.Controls.Add(panelButtons);
            
            // 폼 사이즈 조정
            AdjustFormSize();
            
            // Esc 키로 닫기
            this.KeyPreview = true;
            this.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
        }
        
        // 창 스타일 설정 (닫기 버튼 비활성화 등)
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (this.closeResult == false)
                    cp.ClassStyle = cp.ClassStyle | CP_NOCLOSE_BUTTON;
                return cp;
            }
        }
        
        // 메시지 박스 아이콘 설정
        private void SetMessageBoxIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    pictureIcon.Image = SystemIcons.Information.ToBitmap();
                    break;
                case MessageBoxIcon.Warning:
                    pictureIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
                case MessageBoxIcon.Error:
                    pictureIcon.Image = SystemIcons.Error.ToBitmap();
                    break;
                case MessageBoxIcon.Question:
                    pictureIcon.Image = SystemIcons.Question.ToBitmap();
                    break;
                default:
                    pictureIcon.Visible = false;
                    lblMessage.Left = 15;
                    lblMessage.MaximumSize = new Size(320, 0);
                    break;
            }
        }
        
        // 버튼 추가
        private void AddButtons(MessageBoxButtons buttons)
        {
            const int buttonWidth = 80;
            const int buttonHeight = 28;
            const int margin = 10;
            
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    btnOK = new Button();
                    btnOK.Text = "확인";
                    btnOK.Size = new Size(buttonWidth, buttonHeight);
                    btnOK.Location = new Point(this.ClientSize.Width - buttonWidth - margin, margin);
                    btnOK.DialogResult = DialogResult.OK;
                    btnOK.Click += (s, e) => this.Close();
                    panelButtons.Controls.Add(btnOK);
                    this.AcceptButton = btnOK;
                    break;
                    
                case MessageBoxButtons.OKCancel:
                    btnOK = new Button();
                    btnOK.Text = "확인";
                    btnOK.Size = new Size(buttonWidth, buttonHeight);
                    btnOK.Location = new Point(this.ClientSize.Width - (buttonWidth * 2) - (margin * 2), margin);
                    btnOK.DialogResult = DialogResult.OK;
                    btnOK.Click += (s, e) => this.Close();
                    
                    btnCancel = new Button();
                    btnCancel.Text = "취소";
                    btnCancel.Size = new Size(buttonWidth, buttonHeight);
                    btnCancel.Location = new Point(this.ClientSize.Width - buttonWidth - margin, margin);
                    btnCancel.DialogResult = DialogResult.Cancel;
                    btnCancel.Click += (s, e) => this.Close();
                    
                    panelButtons.Controls.Add(btnOK);
                    panelButtons.Controls.Add(btnCancel);
                    this.AcceptButton = btnOK;
                    this.CancelButton = btnCancel;
                    break;
                    
                case MessageBoxButtons.YesNo:
                    btnYes = new Button();
                    btnYes.Text = "예";
                    btnYes.Size = new Size(buttonWidth, buttonHeight);
                    btnYes.Location = new Point(this.ClientSize.Width - (buttonWidth * 2) - (margin * 2), margin);
                    btnYes.DialogResult = DialogResult.Yes;
                    btnYes.Click += (s, e) => this.Close();
                    
                    btnNo = new Button();
                    btnNo.Text = "아니요";
                    btnNo.Size = new Size(buttonWidth, buttonHeight);
                    btnNo.Location = new Point(this.ClientSize.Width - buttonWidth - margin, margin);
                    btnNo.DialogResult = DialogResult.No;
                    btnNo.Click += (s, e) => this.Close();
                    
                    panelButtons.Controls.Add(btnYes);
                    panelButtons.Controls.Add(btnNo);
                    this.AcceptButton = btnYes;
                    this.CancelButton = btnNo;
                    break;
                    
                case MessageBoxButtons.YesNoCancel:
                    btnYes = new Button();
                    btnYes.Text = "예";
                    btnYes.Size = new Size(buttonWidth, buttonHeight);
                    btnYes.Location = new Point(this.ClientSize.Width - (buttonWidth * 3) - (margin * 3), margin);
                    btnYes.DialogResult = DialogResult.Yes;
                    btnYes.Click += (s, e) => this.Close();
                    
                    btnNo = new Button();
                    btnNo.Text = "아니요";
                    btnNo.Size = new Size(buttonWidth, buttonHeight);
                    btnNo.Location = new Point(this.ClientSize.Width - (buttonWidth * 2) - (margin * 2), margin);
                    btnNo.DialogResult = DialogResult.No;
                    btnNo.Click += (s, e) => this.Close();
                    
                    btnCancel = new Button();
                    btnCancel.Text = "취소";
                    btnCancel.Size = new Size(buttonWidth, buttonHeight);
                    btnCancel.Location = new Point(this.ClientSize.Width - buttonWidth - margin, margin);
                    btnCancel.DialogResult = DialogResult.Cancel;
                    btnCancel.Click += (s, e) => this.Close();
                    
                    panelButtons.Controls.Add(btnYes);
                    panelButtons.Controls.Add(btnNo);
                    panelButtons.Controls.Add(btnCancel);
                    this.AcceptButton = btnYes;
                    this.CancelButton = btnCancel;
                    break;
                    
                case MessageBoxButtons.RetryCancel:
                    btnOK = new Button();
                    btnOK.Text = "다시 시도";
                    btnOK.Size = new Size(buttonWidth, buttonHeight);
                    btnOK.Location = new Point(this.ClientSize.Width - (buttonWidth * 2) - (margin * 2), margin);
                    btnOK.DialogResult = DialogResult.Retry;
                    btnOK.Click += (s, e) => this.Close();
                    
                    btnCancel = new Button();
                    btnCancel.Text = "취소";
                    btnCancel.Size = new Size(buttonWidth, buttonHeight);
                    btnCancel.Location = new Point(this.ClientSize.Width - buttonWidth - margin, margin);
                    btnCancel.DialogResult = DialogResult.Cancel;
                    btnCancel.Click += (s, e) => this.Close();
                    
                    panelButtons.Controls.Add(btnOK);
                    panelButtons.Controls.Add(btnCancel);
                    this.AcceptButton = btnOK;
                    this.CancelButton = btnCancel;
                    break;
                    
                case MessageBoxButtons.AbortRetryIgnore:
                    Button btnAbort = new Button();
                    btnAbort.Text = "중단";
                    btnAbort.Size = new Size(buttonWidth, buttonHeight);
                    btnAbort.Location = new Point(this.ClientSize.Width - (buttonWidth * 3) - (margin * 3), margin);
                    btnAbort.DialogResult = DialogResult.Abort;
                    btnAbort.Click += (s, e) => this.Close();
                    
                    Button btnRetry = new Button();
                    btnRetry.Text = "다시 시도";
                    btnRetry.Size = new Size(buttonWidth, buttonHeight);
                    btnRetry.Location = new Point(this.ClientSize.Width - (buttonWidth * 2) - (margin * 2), margin);
                    btnRetry.DialogResult = DialogResult.Retry;
                    btnRetry.Click += (s, e) => this.Close();
                    
                    Button btnIgnore = new Button();
                    btnIgnore.Text = "무시";
                    btnIgnore.Size = new Size(buttonWidth, buttonHeight);
                    btnIgnore.Location = new Point(this.ClientSize.Width - buttonWidth - margin, margin);
                    btnIgnore.DialogResult = DialogResult.Ignore;
                    btnIgnore.Click += (s, e) => this.Close();
                    
                    panelButtons.Controls.Add(btnAbort);
                    panelButtons.Controls.Add(btnRetry);
                    panelButtons.Controls.Add(btnIgnore);
                    this.AcceptButton = btnRetry;
                    break;
            }
        }
        
        // 폼 크기 자동 조정
        private void AdjustFormSize()
        {
            // 메시지에 따라 높이 조정
            int messageHeight = TextRenderer.MeasureText(lblMessage.Text, lblMessage.Font, lblMessage.MaximumSize).Height;
            int formHeight = Math.Max(messageHeight + panelButtons.Height + 60, 150);
            
            // 폼 크기 설정
            this.ClientSize = new Size(this.ClientSize.Width, formHeight);
            
            // 버튼 패널 위치 조정
            panelButtons.Location = new Point(0, formHeight - panelButtons.Height);
            
            // 버튼 위치 업데이트
            foreach (Control c in panelButtons.Controls)
            {
                if (c is Button)
                {
                    c.Top = (panelButtons.Height - c.Height) / 2;
                }
            }
        }
        
        // 부모 폼 중앙에 위치시키기
        private void CenterToParent(Form parent)
        {
            if (parent != null && parent.Visible)
            {
                this.Left = parent.Left + (parent.Width - this.Width) / 2;
                this.Top = parent.Top + (parent.Height - this.Height) / 2;
                
                // 화면 경계 확인
                Rectangle screen = Screen.FromControl(parent).WorkingArea;
                
                if (this.Left < screen.Left)
                    this.Left = screen.Left;
                else if (this.Left + this.Width > screen.Right)
                    this.Left = screen.Right - this.Width;
                
                if (this.Top < screen.Top)
                    this.Top = screen.Top;
                else if (this.Top + this.Height > screen.Bottom)
                    this.Top = screen.Bottom - this.Height;
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }
        
        // 정적 Show 메서드 (기존 MessageBox.Show 대체)
        public static DialogResult Show(string message)
        {
            return Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.None, null);
        }
        
        public static DialogResult Show(string message, string caption)
        {
            return Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.None, null);
        }
        
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons)
        {
            return Show(message, caption, buttons, MessageBoxIcon.None, null);
        }
        
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(message, caption, buttons, icon, null);
        }
        
        public static DialogResult Show(IWin32Window owner, string message)
        {
            return Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.None, owner);
        }
        
        public static DialogResult Show(IWin32Window owner, string message, string caption)
        {
            return Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.None, owner);
        }
        
        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxButtons buttons)
        {
            return Show(message, caption, buttons, MessageBoxIcon.None, owner);
        }
        
        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(message, caption, buttons, icon, owner);
        }
        
        // 메인 Show 메서드
        private static DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, IWin32Window owner)
        {
            using (CustomMessageBox msgBox = new CustomMessageBox(message, caption, buttons, icon))
            {
                // 부모 폼 가져오기
                Form parentForm = null;
                if (owner != null)
                {
                    parentForm = owner as Form;
                    if (parentForm == null)
                    {
                        Control control = owner as Control;
                        if (control != null)
                            parentForm = control.FindForm();
                    }
                }
                else
                {
                    parentForm = Form.ActiveForm;
                }
                
                // 부모 폼 중앙에 위치
                msgBox.CenterToParent(parentForm);
                
                // 모달로 표시
                if (parentForm != null)
                {
                    msgBox.ShowInTaskbar = false;
                    return msgBox.ShowDialog(parentForm);
                }
                else
                {
                    return msgBox.ShowDialog();
                }
            }
        }
    }
} 