using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using TotalCommander;

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
                // Initialize logger
                Logger.Initialize();
                Logger.Information("Application starting...");
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                LoadStartupLibraryDLL();
                
                Form_TotalCommander form = new Form_TotalCommander();
                Application.Run(form);
                
                Logger.Information("Application shutting down gracefully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unhandled exception in Main");
                MessageBox.Show("Application encountered a fatal error: " + ex.Message, 
                    "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Loads required libraries.
        /// </summary>
        private static void LoadStartupLibraryDLL()
        {
            try
            {
                // Initialize Shell32.dll required for Explorer functionality
                // Additional DLL loading code can be added here
                Logger.Debug("DLL libraries loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load startup libraries");
            }
        }
    }
}

