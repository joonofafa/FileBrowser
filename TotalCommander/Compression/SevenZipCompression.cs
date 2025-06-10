using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TotalCommander.Compression
{
    /// <summary>
    /// 압축 파일 형식
    /// </summary>
    public enum ArchiveFormat
    {
        Zip,
        SevenZip,
        Tar,
        GZip,
        BZip2,
        XZ
    }

    /// <summary>
    /// 압축 레벨
    /// </summary>
    public enum CompressionLevel
    {
        None = 0,
        Fastest = 1,
        Fast = 3,
        Normal = 5,
        Maximum = 7,
        Ultra = 9
    }

    /// <summary>
    /// 7za.exe를 사용한 압축 및 해제 기능을 제공하는 클래스
    /// </summary>
    public class SevenZipCompression
    {
        #region 필드

        private const string SevenZipExeName = "7za.exe";
        private readonly string _sevenZipPath;
        private readonly StringBuilder _errorOutput = new StringBuilder();
        private readonly StringBuilder _standardOutput = new StringBuilder();
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region 이벤트

        /// <summary>
        /// 압축 또는 압축 해제 작업이 완료되었을 때 발생하는 이벤트
        /// </summary>
        public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        #endregion

        #region 생성자

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="sevenZipPath">7za.exe 파일 경로 (null인 경우 실행 파일과 같은 위치에서 찾음)</param>
        public SevenZipCompression(string sevenZipPath = null)
        {
            // 7za.exe 경로가 지정되지 않은 경우 실행 파일과 같은 경로에서 찾음
            if (string.IsNullOrEmpty(sevenZipPath))
            {
                _sevenZipPath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    SevenZipExeName);
            }
            else
            {
                _sevenZipPath = sevenZipPath;
            }

            // 7za.exe가 존재하는지 확인
            if (!File.Exists(_sevenZipPath))
            {
                throw new FileNotFoundException($"7za.exe를 찾을 수 없습니다: {_sevenZipPath}");
            }
        }

        #endregion

        #region 공용 메서드

        /// <summary>
        /// 파일 압축 (비동기)
        /// </summary>
        /// <param name="archivePath">압축 파일 경로</param>
        /// <param name="sourceFiles">소스 파일 경로 목록</param>
        /// <param name="format">압축 형식</param>
        /// <param name="level">압축 수준</param>
        /// <param name="progress">진행 상황 보고 콜백 (0-100)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>작업 결과</returns>
        public async Task<bool> CompressAsync(
            string archivePath,
            IEnumerable<string> sourceFiles,
            ArchiveFormat format = ArchiveFormat.Zip,
            CompressionLevel level = CompressionLevel.Normal,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            // 파일 경로 목록을 임시 파일로 저장
            string listFile = Path.GetTempFileName();
            try
            {
                // 파일 목록 생성
                using (StreamWriter writer = new StreamWriter(listFile, false, Encoding.UTF8))
                {
                    foreach (string file in sourceFiles)
                    {
                        await writer.WriteLineAsync(file);
                    }
                }

                // 압축 형식에 따른 확장자 및 명령줄 스위치
                string formatSwitch = GetFormatSwitch(format);
                
                // 압축 레벨
                string levelSwitch = $"-mx{(int)level}";
                
                // 명령 구성
                string arguments = $"a {formatSwitch} {levelSwitch} -bb1 -bsp1 \"{archivePath}\" @\"{listFile}\"";

                // 명령 실행
                bool result = await RunProcessAsync(arguments, progress, cancellationToken);
                
                // 작업 완료 이벤트 발생
                OnOperationCompleted(new OperationCompletedEventArgs(result, archivePath, true));
                
                return result;
            }
            finally
            {
                // 임시 파일 삭제
                if (File.Exists(listFile))
                {
                    File.Delete(listFile);
                }
            }
        }

        /// <summary>
        /// 압축 파일 해제 (비동기)
        /// </summary>
        /// <param name="archivePath">압축 파일 경로</param>
        /// <param name="destinationPath">압축 해제 대상 경로</param>
        /// <param name="progress">진행 상황 보고 콜백 (0-100)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>작업 결과</returns>
        public async Task<bool> ExtractAsync(
            string archivePath,
            string destinationPath,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            // 대상 디렉토리가 없으면 생성
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // 명령 구성 (-bb1: 진행률 표시, -bsp1: 진행률 표시)
            string arguments = $"x \"{archivePath}\" -o\"{destinationPath}\" -y -bb1 -bsp1";

            // 명령 실행
            bool result = await RunProcessAsync(arguments, progress, cancellationToken);
            
            // 작업 완료 이벤트 발생
            OnOperationCompleted(new OperationCompletedEventArgs(result, destinationPath, false));
            
            return result;
        }

        /// <summary>
        /// 압축 파일 내용 목록 조회
        /// </summary>
        /// <param name="archivePath">압축 파일 경로</param>
        /// <returns>압축 파일 내 항목 목록</returns>
        public async Task<List<string>> ListContentsAsync(string archivePath)
        {
            // 명령 구성
            string arguments = $"l \"{archivePath}\" -ba";

            // 명령 실행
            _standardOutput.Clear();
            bool success = await RunProcessAsync(arguments);

            if (success)
            {
                List<string> fileList = new List<string>();
                string output = _standardOutput.ToString();
                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    // 파일 정보에서 파일 이름만 추출
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        fileList.Add(parts[parts.Length - 1]);
                    }
                }

                return fileList;
            }

            return new List<string>();
        }

        /// <summary>
        /// 진행 중인 작업 취소
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        #endregion

        #region 내부 헬퍼 메서드

        /// <summary>
        /// 압축 형식에 따른 명령줄 스위치 반환
        /// </summary>
        private string GetFormatSwitch(ArchiveFormat format)
        {
            switch (format)
            {
                case ArchiveFormat.Zip:
                    return "-tzip";
                case ArchiveFormat.SevenZip:
                    return "-t7z";
                case ArchiveFormat.Tar:
                    return "-ttar";
                case ArchiveFormat.GZip:
                    return "-tgzip";
                case ArchiveFormat.BZip2:
                    return "-tbzip2";
                case ArchiveFormat.XZ:
                    return "-txz";
                default:
                    return "-tzip";
            }
        }

        /// <summary>
        /// 7za.exe 프로세스 실행
        /// </summary>
        private async Task<bool> RunProcessAsync(
            string arguments,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _errorOutput.Clear();
            _standardOutput.Clear();

            using (Process process = new Process())
            {
                process.StartInfo.FileName = _sevenZipPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                // 출력 데이터 처리를 위한 이벤트 핸들러
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _standardOutput.AppendLine(e.Data);
                        
                        // 진행률 파싱 및 보고
                        if (progress != null && e.Data.Contains("%"))
                        {
                            int percentIndex = e.Data.IndexOf("%");
                            if (percentIndex > 0 && percentIndex < e.Data.Length)
                            {
                                // % 앞의 숫자 찾기
                                int i = percentIndex - 1;
                                while (i >= 0 && char.IsDigit(e.Data[i]))
                                {
                                    i--;
                                }

                                if (i < percentIndex - 1)
                                {
                                    string percentStr = e.Data.Substring(i + 1, percentIndex - i - 1);
                                    if (int.TryParse(percentStr, out int percent))
                                    {
                                        progress.Report(percent);
                                    }
                                }
                            }
                        }
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _errorOutput.AppendLine(e.Data);
                    }
                };

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // 취소 토큰이 설정된 경우 프로세스 종료 처리
                    if (cancellationToken != CancellationToken.None)
                    {
                        _cancellationTokenSource.Token.Register(() =>
                        {
                            try
                            {
                                if (!process.HasExited)
                                {
                                    process.Kill();
                                }
                            }
                            catch { /* 무시 */ }
                        });
                    }

                    // 프로세스 완료 대기
                    await Task.Run(() => process.WaitForExit(), _cancellationTokenSource.Token);
                    
                    // 결과 확인
                    return process.ExitCode == 0;
                }
                catch (OperationCanceledException)
                {
                    // 작업 취소됨
                    return false;
                }
                catch (Exception ex)
                {
                    _errorOutput.AppendLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 작업 완료 이벤트 발생
        /// </summary>
        protected virtual void OnOperationCompleted(OperationCompletedEventArgs e)
        {
            OperationCompleted?.Invoke(this, e);
        }

        #endregion
    }

    /// <summary>
    /// 압축 작업 완료 이벤트 인자
    /// </summary>
    public class OperationCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 작업 성공 여부
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// 압축 파일 또는 압축 해제 폴더 경로
        /// </summary>
        public string Path { get; }
        
        /// <summary>
        /// 압축 작업 여부 (true: 압축, false: 압축 해제)
        /// </summary>
        public bool IsCompression { get; }
        
        /// <summary>
        /// 생성자
        /// </summary>
        public OperationCompletedEventArgs(bool success, string path, bool isCompression)
        {
            Success = success;
            Path = path;
            IsCompression = isCompression;
        }
    }
} 