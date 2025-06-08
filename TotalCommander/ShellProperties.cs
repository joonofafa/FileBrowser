using System;
using System.Runtime.InteropServices;

namespace TotalCommander
{
    /// <summary>
    /// 파일 속성 대화 상자를 표시하는 유틸리티 클래스
    /// </summary>
    public static class ShellProperties
    {
        /// <summary>
        /// 파일 속성 대화 상자를 표시하는 Win32 API 함수
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const uint SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 0xC;

        /// <summary>
        /// 단일 파일의 속성 대화 상자를 표시합니다.
        /// </summary>
        /// <param name="fileName">표시할 파일 경로</param>
        public static bool ShowFileProperties(string fileName)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = fileName;
            info.nShow = (int)SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }

        /// <summary>
        /// 여러 파일의 속성 대화 상자를 표시합니다.
        /// </summary>
        /// <param name="fileNames">표시할 파일 경로 배열</param>
        public static bool ShowFileProperties(string[] fileNames)
        {
            if (fileNames == null || fileNames.Length == 0)
                return false;

            if (fileNames.Length == 1)
                return ShowFileProperties(fileNames[0]);

            // 여러 파일의 경우, 첫 번째 파일로 다중 선택 속성 대화 상자를 표시합니다.
            // 실제로는 완벽한 다중 선택 속성 대화 상자를 구현하기 위해서는 Shell COM 인터페이스를 사용해야 합니다.
            // 이 구현은 단순화된 버전입니다.
            return ShowFileProperties(fileNames[0]);
        }
    }
} 