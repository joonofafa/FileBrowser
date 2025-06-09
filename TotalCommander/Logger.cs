using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TotalCommander
{
    /// <summary>
    /// 파일 기반 로깅 유틸리티 클래스
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFilePath;
        private static bool _isEnabled = true;
        
        /// <summary>
        /// 로거를 초기화합니다. 애플리케이션 시작 시 호출해야 합니다.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // 로그 디렉토리 생성 (Documents 폴더 아래 TotalCommander/Logs)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string logDirPath = Path.Combine(documentsPath, "TotalCommander", "Logs");
                
                if (!Directory.Exists(logDirPath))
                {
                    Directory.CreateDirectory(logDirPath);
                }
                
                // 날짜별 로그 파일 생성
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                _logFilePath = Path.Combine(logDirPath, $"log_{today}.txt");
                
                // 로그 파일이 존재하지 않으면 헤더 기록
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
                
                // 시작 로그 작성
                Info("Logger initialized successfully");
            }
            catch (Exception ex)
            {
                _isEnabled = false;
                System.Windows.Forms.MessageBox.Show($"로그 파일을 초기화하는 중 오류가 발생했습니다: {ex.Message}",
                    "로깅 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 정보 로그를 기록합니다.
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }
        
        /// <summary>
        /// 디버그 로그를 기록합니다.
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }
        
        /// <summary>
        /// 경고 로그를 기록합니다.
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Warning(string message)
        {
            WriteLog("WARNING", message);
        }
        
        /// <summary>
        /// 오류 로그를 기록합니다.
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Error(string message)
        {
            WriteLog("ERROR", message);
        }
        
        /// <summary>
        /// 오류 로그를 기록합니다.
        /// </summary>
        /// <param name="ex">발생한 예외</param>
        /// <param name="message">추가 메시지 (선택사항)</param>
        public static void Error(Exception ex, string message = null)
        {
            string errorMessage = string.IsNullOrEmpty(message) ? 
                $"Exception: {ex.Message}" : 
                $"{message} - Exception: {ex.Message}";
            
            WriteLog("ERROR", errorMessage);
            WriteLog("ERROR", $"StackTrace: {ex.StackTrace}");
        }
        
        /// <summary>
        /// 멀티라인 디버그 로그를 기록합니다.
        /// </summary>
        /// <param name="title">로그 제목</param>
        /// <param name="content">여러 줄의 내용</param>
        public static void DebugMultiline(string title, string content)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
                return;
                
            try
            {
                lock (_lock)
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] --- {title} ---");
                        writer.WriteLine(content);
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] --- End of {title} ---");
                        writer.WriteLine();
                    }
                }
            }
            catch
            {
                // 로그 작성 실패 시 조용히 무시
                _isEnabled = false;
            }
        }
        
        /// <summary>
        /// 로그를 파일에 기록합니다.
        /// </summary>
        /// <param name="level">로그 레벨</param>
        /// <param name="message">로그 메시지</param>
        private static void WriteLog(string level, string message)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logFilePath))
                return;
                
            try
            {
                lock (_lock)
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");
                    }
                }
            }
            catch
            {
                // 로그 작성 실패 시 조용히 무시
                _isEnabled = false;
            }
        }
    }
} 