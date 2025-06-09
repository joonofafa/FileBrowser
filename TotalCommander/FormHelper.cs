using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TotalCommander
{
    /// <summary>
    /// Helper class for Form positioning and other common form operations
    /// </summary>
    public static class FormHelper
    {
        // Win32 API 상수 및 구조체 정의
        private const int WH_CALLWNDPROCRET = 12;
        private const int WM_INITDIALOG = 0x0110;
        private const int WM_ACTIVATE = 0x0006;
        private const int WM_WINDOWPOSCHANGING = 0x0046;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        /// <summary>
        /// Centers the form on its parent or on the screen if no parent is available.
        /// Also ensures the form is completely visible on screen.
        /// </summary>
        /// <param name="form">The form to center</param>
        public static void CenterFormOnParentOrScreen(Form form)
        {
            if (form == null)
                return;

            // Set default start position to manual so we can position it exactly
            form.StartPosition = FormStartPosition.Manual;
            
            // Get parent form
            Form parentForm = form.Owner;
            
            // If no owner, try to get active form from the application
            if (parentForm == null)
            {
                parentForm = Form.ActiveForm;
                if (parentForm != form)
                {
                    form.Owner = parentForm; // Set owner if possible
                }
            }
            
            // If we found a parent form, center on it
            if (parentForm != null && parentForm.Visible)
            {
                int x = parentForm.Left + (parentForm.Width - form.Width) / 2;
                int y = parentForm.Top + (parentForm.Height - form.Height) / 2;
                
                // Ensure the form is fully visible on screen
                Rectangle screen = Screen.FromControl(parentForm).WorkingArea;
                
                if (x < screen.Left)
                    x = screen.Left;
                else if (x + form.Width > screen.Right)
                    x = screen.Right - form.Width;
                
                if (y < screen.Top)
                    y = screen.Top;
                else if (y + form.Height > screen.Bottom)
                    y = screen.Bottom - form.Height;
                
                form.Location = new Point(x, y);
            }
            // If no parent form, center on screen
            else
            {
                Rectangle screen = Screen.FromControl(form).WorkingArea;
                int x = screen.Left + (screen.Width - form.Width) / 2;
                int y = screen.Top + (screen.Height - form.Height) / 2;
                form.Location = new Point(x, y);
            }
        }
        
        /// <summary>
        /// Shows a dialog centered on the parent form
        /// </summary>
        /// <param name="form">The form to show</param>
        /// <param name="owner">The owner form</param>
        /// <returns>Dialog result</returns>
        public static DialogResult ShowDialogCentered(Form form, Form owner)
        {
            if (form == null)
                return DialogResult.Cancel;
                
            // Set owner
            if (owner != null)
            {
                form.Owner = owner;
            }
            
            // Use manual positioning
            form.StartPosition = FormStartPosition.Manual;
            
            // Center dialog before showing it
            CenterFormOnParentOrScreen(form);
            
            // Show dialog
            return form.ShowDialog();
        }
        
        /// <summary>
        /// Shows a common dialog with the specified owner
        /// </summary>
        /// <param name="dialog">The common dialog to show</param>
        /// <param name="owner">The owner form</param>
        /// <returns>Dialog result</returns>
        public static DialogResult ShowCommonDialog(CommonDialog dialog, Form owner)
        {
            if (dialog == null)
                return DialogResult.Cancel;
            
            DialogResult result;
            
            // Create dialog monitor to center common dialogs
            using (var monitor = new DialogMonitor(owner))
            {
                // Show dialog
                if (owner != null)
                {
                    result = dialog.ShowDialog(owner);
                }
                else
                {
                    result = dialog.ShowDialog();
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Dialog monitor class that hooks window creation messages
        /// to position common dialogs
        /// </summary>
        private class DialogMonitor : IDisposable
        {
            private Form _parentForm;
            private IntPtr _hookHandle = IntPtr.Zero;
            private HookProc _hookProc;
            
            public DialogMonitor(Form parentForm)
            {
                _parentForm = parentForm;
                
                if (_parentForm != null)
                {
                    // Create hook procedure
                    _hookProc = HookCallback;
                    
                    // Install hook
                    _hookHandle = SetWindowsHookEx(WH_CALLWNDPROCRET, _hookProc, 
                        IntPtr.Zero, GetCurrentThreadId());
                }
            }
            
            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && _parentForm != null && _parentForm.Handle != IntPtr.Zero)
                {
                    CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
                    
                    // Check if this is a dialog initialization message
                    if (msg.message == WM_INITDIALOG || msg.message == WM_ACTIVATE)
                    {
                        // Get dialog window size
                        RECT dialogRect;
                        if (GetWindowRect(msg.hwnd, out dialogRect))
                        {
                            int dialogWidth = dialogRect.right - dialogRect.left;
                            int dialogHeight = dialogRect.bottom - dialogRect.top;
                            
                            // Calculate centered position
                            RECT parentRect;
                            GetWindowRect(_parentForm.Handle, out parentRect);
                            
                            int centerX = parentRect.left + 
                                ((parentRect.right - parentRect.left) - dialogWidth) / 2;
                            int centerY = parentRect.top + 
                                ((parentRect.bottom - parentRect.top) - dialogHeight) / 2;
                            
                            // Ensure it's on screen
                            Rectangle screen = Screen.FromHandle(_parentForm.Handle).WorkingArea;
                            
                            if (centerX < screen.Left)
                                centerX = screen.Left;
                            else if (centerX + dialogWidth > screen.Right)
                                centerX = screen.Right - dialogWidth;
                            
                            if (centerY < screen.Top)
                                centerY = screen.Top;
                            else if (centerY + dialogHeight > screen.Bottom)
                                centerY = screen.Bottom - dialogHeight;
                            
                            // Set dialog position
                            SetWindowPos(msg.hwnd, IntPtr.Zero, centerX, centerY, 
                                0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
                        }
                    }
                }
                
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }
            
            public void Dispose()
            {
                if (_hookHandle != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookHandle);
                    _hookHandle = IntPtr.Zero;
                }
                
                // Ensure the delegate is not garbage collected
                _hookProc = null;
            }
        }
    }
} 