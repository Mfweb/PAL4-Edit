using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PAL4_EDIT
{
    class W_API
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LPMODULEENTRY32
        {
            public UInt32 dwSize;
            public UInt32 th32ModuleID;
            public UInt32 th32ProcessID;
            public UInt32 GlblcntUsage;
            public UInt32 ProccntUsage;
            public IntPtr modBaseAddr;
            public UInt32 modBaseSize;
            public UInt32 hModule;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] mFile;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] mPath;
        }
        //HANDLE OpenProcess(
        //    DWORD dwDesiredAccess,// access flag
        //    BOOL bInheritHandle, // handle inheritance flag
        //    DWORD dwProcessId  // process identifier
        //    );
        [DllImport("kernel32.dll")]
        public static extern
            IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        //BOOL CloseHandle(
        //    HANDLE hObject  // handle to object to close
        //    );
        [DllImport("kernel32.dll")]
        public static extern
            Int32 CloseHandle(IntPtr hObject);

        //BOOL WriteProcessMemory(
        //    HANDLE hProcess, // handle to process whose memory is written to
        //    LPVOID lpBaseAddress, // address to start writing to
        //    LPVOID lpBuffer, // pointer to buffer to write data to
        //    DWORD nSize, // number of bytes to write
        //    LPDWORD lpNumberOfBytesWritten  // actual number of bytes written
        //    );
        [DllImport("kernel32.dll")]
        public static extern
            Int32 WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, UInt32 nSize, out int lpNumberOfBytesWritten);

        //BOOL ReadProcessMemory(
        //    HANDLE hProcess, // handle of the process whose memory is read
        //    LPCVOID lpBaseAddress, // address to start reading
        //    LPVOID lpBuffer, // address of buffer to place read data
        //    DWORD nSize, // number of bytes to read
        //    LPDWORD lpNumberOfBytesRead  // address of number of bytes read
        //    );
        [DllImport("kernel32.dll")]
        public static extern
            Int32 ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpbuff, UInt32 nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern
            Int32 GetLastError();

        [DllImport("user32.dll")]
        public static extern
            Int32 GetWindowThreadProcessId(IntPtr hWnd, out Int32 lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern
            IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("kernel32.dll")]
        public static extern
            IntPtr CreateToolhelp32Snapshot(uint flags,uint processid);

        [DllImport("kernel32.dll")]
        public static extern
           int Module32First(IntPtr hSnapshot,ref LPMODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        public static extern
           int GetModuleHandle(string lpModuleName);
    }
}
