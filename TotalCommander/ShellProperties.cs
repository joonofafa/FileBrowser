using System;
using System.Runtime.InteropServices;
using TotalCommander;

namespace TotalCommander
{
    /// <summary>
    /// Utility class for displaying file properties dialog
    /// </summary>
    public static class ShellProperties
    {
        /// <summary>
        /// Win32 API function to display file properties dialog
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
        /// Display properties dialog for a single file
        /// </summary>
        /// <param name="fileName">Path to the file to display properties for</param>
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
        /// Display properties dialog for multiple files
        /// </summary>
        /// <param name="fileNames">Array of file paths to display properties for</param>
        public static bool ShowFileProperties(string[] fileNames)
        {
            if (fileNames == null || fileNames.Length == 0)
                return false;

            if (fileNames.Length == 1)
                return ShowFileProperties(fileNames[0]);

            // For multiple files, currently only shows properties for the first file
            // To properly implement multi-selection properties dialog, Shell COM interface should be used
            // This is a simplified version
            return ShowFileProperties(fileNames[0]);
        }
    }
} 
