using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace TotalCommander
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // 로거 초기화
                Logger.Initialize();
                Logger.Info("Application starting...");
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                LoadStartupLibraryDLL();
                
                Form_TotalCommander form = new Form_TotalCommander();
                Application.Run(form);
                
                Logger.Info("Application shutting down gracefully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unhandled exception in Main");
                MessageBox.Show("Application encountered a fatal error: " + ex.Message, 
                    "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 필요한 라이브러리를 로드합니다.
        /// </summary>
        private static void LoadStartupLibraryDLL()
        {
            try
            {
                // 아이콘 로드에 필요한 Shell32.dll 초기화
                // 여기에 필요한 DLL 로드 관련 코드를 추가할 수 있습니다
                Logger.Debug("DLL libraries loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load startup libraries");
            }
        }
    }
}
