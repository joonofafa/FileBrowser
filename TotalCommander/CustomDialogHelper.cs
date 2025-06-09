using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace TotalCommander
{
    /// <summary>
    /// 공통 대화 상자를 부모 폼 중앙에 표시하기 위한 헬퍼 클래스
    /// </summary>
    public static class CustomDialogHelper
    {
        // Win32 API 선언
        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        // Win32 상수
        private const int WH_CBT = 5;
        private const int HCBT_ACTIVATE = 5;

        // Win32 구조체
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // 델리게이트 정의
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static HookProc _hookProc;
        private static IntPtr _hookHandle = IntPtr.Zero;
        private static Form _parentForm;

        /// <summary>
        /// 대화 상자 후킹 설치
        /// </summary>
        public static void InstallHook(Form parentForm)
        {
            if (_hookHandle != IntPtr.Zero)
                return;

            _parentForm = parentForm;
            _hookProc = new HookProc(DialogHookProc);
            _hookHandle = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, GetCurrentThreadId());
        }

        /// <summary>
        /// 대화 상자 후킹 제거
        /// </summary>
        public static void RemoveHook()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
                _parentForm = null;
            }
        }

        /// <summary>
        /// 대화 상자 후킹 프로시저
        /// </summary>
        private static IntPtr DialogHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HCBT_ACTIVATE && _parentForm != null)
            {
                // 대화 상자의 핸들을 가져옴
                IntPtr hWnd = wParam;

                try
                {
                    // 대화 상자의 크기와 위치 정보 가져오기
                    GetWindowRect(hWnd, out RECT dialogRect);
                    int dialogWidth = dialogRect.Right - dialogRect.Left;
                    int dialogHeight = dialogRect.Bottom - dialogRect.Top;

                    // 부모 폼의 중앙 좌표 계산
                    int parentCenterX = _parentForm.Left + (_parentForm.Width / 2);
                    int parentCenterY = _parentForm.Top + (_parentForm.Height / 2);

                    // 대화 상자의 새로운 위치 계산 (부모 폼 중앙)
                    int newLeft = parentCenterX - (dialogWidth / 2);
                    int newTop = parentCenterY - (dialogHeight / 2);

                    // 화면 경계 확인
                    Rectangle screenBounds = Screen.FromControl(_parentForm).WorkingArea;
                    if (newLeft < screenBounds.Left)
                        newLeft = screenBounds.Left;
                    else if (newLeft + dialogWidth > screenBounds.Right)
                        newLeft = screenBounds.Right - dialogWidth;

                    if (newTop < screenBounds.Top)
                        newTop = screenBounds.Top;
                    else if (newTop + dialogHeight > screenBounds.Bottom)
                        newTop = screenBounds.Bottom - dialogHeight;

                    // 대화 상자 위치 변경
                    MoveWindow(hWnd, newLeft, newTop, dialogWidth, dialogHeight, true);
                }
                catch
                {
                    // 예외 무시
                }

                // 한 번만 처리하고 후킹 제거
                RemoveHook();
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        /// <summary>
        /// OpenFileDialog를 부모 폼 중앙에 표시하기
        /// </summary>
        public static DialogResult ShowOpenFileDialog(OpenFileDialog dialog, Form parent)
        {
            InstallHook(parent);
            return dialog.ShowDialog(parent);
        }

        /// <summary>
        /// SaveFileDialog를 부모 폼 중앙에 표시하기
        /// </summary>
        public static DialogResult ShowSaveFileDialog(SaveFileDialog dialog, Form parent)
        {
            InstallHook(parent);
            return dialog.ShowDialog(parent);
        }

        /// <summary>
        /// FolderBrowserDialog를 부모 폼 중앙에 표시하기
        /// </summary>
        public static DialogResult ShowFolderBrowserDialog(FolderBrowserDialog dialog, Form parent)
        {
            InstallHook(parent);
            return dialog.ShowDialog(parent);
        }

        /// <summary>
        /// 일반 공통 대화 상자를 부모 폼 중앙에 표시하기
        /// </summary>
        public static DialogResult ShowCommonDialog(CommonDialog dialog, Form parent)
        {
            InstallHook(parent);
            return dialog.ShowDialog(parent);
        }

        /// <summary>
        /// 메시지 박스를 부모 폼 중앙에 표시하기
        /// </summary>
        public static DialogResult ShowMessageBox(Form parent, string text, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return CustomMessageBox.Show(parent, text, caption, buttons, icon);
        }

        /// <summary>
        /// 메시지 박스를 부모 폼 중앙에 표시하기 (string 확장 메서드)
        /// </summary>
        public static DialogResult ShowMessageBox(this string text, Form parent, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return CustomMessageBox.Show(parent, text, caption, buttons, icon);
        }
    }
} 