using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using TotalCommander;

namespace TotalCommander
{
    /// <summary>
    /// Log level enumeration
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,      // Detailed information for debugging
        Information = 1, // General information about application flow
        Warning = 2,     // Potential issues that might cause problems
        Error = 3,       // Errors that prevent normal operation
        Critical = 4     // Critical errors that require immediate attention
    }

    /// <summary>
    /// File-based logging utility class
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFilePath;
        private static bool _isEnabled = true;
        private static LogLevel _minimumLogLevel = LogLevel.Debug; // Log everything by default
        private static List<string> _recentLogs = new List<string>(100); // Keep recent logs in memory
        
        /// <summary>
        /// Gets or sets the minimum log level to record
        /// </summary>
        public static LogLevel MinimumLogLevel
        {
            get { return _minimumLogLevel; }
            set { _minimumLogLevel = value; }
        }
        
        /// <summary>
        /// Gets the recent logs
        /// </summary>
        public static IEnumerable<string> RecentLogs
        {
            get { return _recentLogs; }
        }
        
        /// <summary>
        /// Initialize the logger. Should be called at application startup.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Create log directory (under Documents folder TotalCommander/Logs)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string logDirPath = Path.Combine(documentsPath, "TotalCommander", "Logs");
                
                if (!Directory.Exists(logDirPath))
                {
                    Directory.CreateDirectory(logDirPath);
                }
                
                // Create date-based log file
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                _logFilePath = Path.Combine(logDirPath, $"log_{today}.txt");
                
                // Write header if log file doesn't exist
                if (!File.Exists(_logFilePath))
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, false, Encoding.UTF8))
                    {
                        writer.WriteLine("===== TotalCommander Log File =====");
                        writer.WriteLine($"Created at: {DateTime.Now}");
                        writer.WriteLine("==================================");
                        writer.WriteLine();
                    }
                }
                
                // Write startup log
                Information("Logger initialized successfully");
            }
            catch (Exception ex)
            {
                _isEnabled = false;
                MessageBox.Show($"Error initializing log file: {ex.Message}",
                    "Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="message">Log message</param>
        public static void Debug(string message)
        {
            WriteLog(LogLevel.Debug, message);
        }
        
        /// <summary>
        /// Log an information message
        /// </summary>
        /// <param name="message">Log message</param>
        public static void Information(string message)
        {
            WriteLog(LogLevel.Information, message);
        }
        
        /// <summary>
        /// Log an information message and show it in a message box
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="title">Message box title</param>
        public static void InformationWithDialog(string message, string title = "Information")
        {
            Information($"[DIALOG] {message}");
            
            Form activeForm = Form.ActiveForm;
            if (activeForm != null)
            {
                CustomDialogHelper.ShowMessageBox(activeForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">Log message</param>
        public static void Warning(string message)
        {
            WriteLog(LogLevel.Warning, message);
        }
        
        /// <summary>
        /// Log a warning message and show it in a message box
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="title">Message box title</param>
        public static void WarningWithDialog(string message, string title = "Warning")
        {
            Warning($"[DIALOG] {message}");
            
            Form activeForm = Form.ActiveForm;
            if (activeForm != null)
            {
                CustomDialogHelper.ShowMessageBox(activeForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">Log message</param>
        public static void Error(string message)
        {
            WriteLog(LogLevel.Error, message);
        }
        
        /// <summary>
        /// Log an error message and show it in a message box
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="title">Message box title</param>
        public static void ErrorWithDialog(string message, string title = "Error")
        {
            Error($"[DIALOG] {message}");
            
            Form activeForm = Form.ActiveForm;
            if (activeForm != null)
            {
                CustomDialogHelper.ShowMessageBox(activeForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Log an error message with exception details
        /// </summary>
        /// <param name="ex">Exception that occurred</param>
        /// <param name="message">Additional message (optional)</param>
        public static void Error(Exception ex, string message = null)
        {
            string errorMessage = string.IsNullOrEmpty(message) ? 
                $"Exception: {ex.Message}" : 
                $"{message} - Exception: {ex.Message}";
            
            WriteLog(LogLevel.Error, errorMessage);
            WriteLog(LogLevel.Error, $"StackTrace: {ex.StackTrace}");
            
            // Log inner exception if present
            if (ex.InnerException != null)
            {
                WriteLog(LogLevel.Error, $"Inner Exception: {ex.InnerException.Message}");
                WriteLog(LogLevel.Error, $"Inner StackTrace: {ex.InnerException.StackTrace}");
            }
        }
        
        /// <summary>
        /// Log an error with exception details and show it in a message box
        /// </summary>
        /// <param name="ex">Exception that occurred</param>
        /// <param name="message">Additional message (optional)</param>
        /// <param name="title">Message box title</param>
        public static void ErrorWithDialog(Exception ex, string message = null, string title = "Error")
        {
            string errorMessage = string.IsNullOrEmpty(message) ? 
                $"Exception: {ex.Message}" : 
                $"{message}\n\nDetails: {ex.Message}";
            
            Error(ex, $"[DIALOG] {(message ?? string.Empty)}");
            
            Form activeForm = Form.ActiveForm;
            if (activeForm != null)
            {
                CustomDialogHelper.ShowMessageBox(activeForm, errorMessage, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(errorMessage, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Log a critical error message
        /// </summary>
        /// <param name="message">Log message</param>
        public static void Critical(string message)
        {
            WriteLog(LogLevel.Critical, message);
        }
        
        /// <summary>
        /// Log a critical error message and show it in a message box
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="title">Message box title</param>
        public static void CriticalWithDialog(string message, string title = "Critical Error")
        {
            Critical($"[DIALOG] {message}");
            
            Form activeForm = Form.ActiveForm;
            if (activeForm != null)
            {
                CustomDialogHelper.ShowMessageBox(activeForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Log a multiline debug message
        /// </summary>
        /// <param name="title">Log title</param>
        /// <param name="content">Multiline content</param>
        public static void DebugMultiline(string title, string content)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath) || LogLevel.Debug < _minimumLogLevel)
                return;
                
            try
            {
                lock (_lock)
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] --- {title} ---";
                        writer.WriteLine(logEntry);
                        writer.WriteLine(content);
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] --- End of {title} ---");
                        writer.WriteLine();
                        
                        // Add to recent logs
                        AddToRecentLogs(logEntry);
                        AddToRecentLogs($"Content: {content.Length} characters");
                    }
                }
            }
            catch
            {
                // Silently fail if logging fails
                _isEnabled = false;
            }
        }
        
        /// <summary>
        /// Write a log entry to the file
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Log message</param>
        private static void WriteLog(LogLevel level, string message)
        {
            // Skip if logging is disabled or level is below minimum
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath) || level < _minimumLogLevel)
                return;
                
            try
            {
                lock (_lock)
                {
                    string levelText = level.ToString().ToUpper();
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{levelText}] {message}";
                    
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(logEntry);
                    }
                    
                    // Add to recent logs
                    AddToRecentLogs(logEntry);
                }
            }
            catch
            {
                // Silently fail if logging fails
                _isEnabled = false;
            }
        }
        
        /// <summary>
        /// Add a log entry to the recent logs collection
        /// </summary>
        /// <param name="logEntry">Log entry to add</param>
        private static void AddToRecentLogs(string logEntry)
        {
            lock (_recentLogs)
            {
                _recentLogs.Add(logEntry);
                
                // Keep only the most recent logs (limit to 100)
                while (_recentLogs.Count > 100)
                {
                    _recentLogs.RemoveAt(0);
                }
            }
        }
        
        /// <summary>
        /// Flush all pending log entries and close the log file
        /// </summary>
        public static void Shutdown()
        {
            if (_isEnabled)
            {
                Information("Logger shutdown");
            }
        }
        
        /// <summary>
        /// Shows a confirmation dialog and returns true if user selects Yes.
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Dialog title</param>
        /// <returns>True if user confirms, false otherwise</returns>
        public static bool Confirm(string message, string title = "Confirmation")
        {
            Form activeForm = Form.ActiveForm;
            DialogResult result;
            
            if (activeForm != null)
            {
                result = CustomDialogHelper.ShowMessageBox(activeForm, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            else
            {
                result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            
            return result == DialogResult.Yes;
        }
    }
} 
