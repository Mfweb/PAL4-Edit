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
            unsafe Int32 ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte* lpbuff, UInt32 nSize, out int lpNumberOfBytesRead);

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




        /// <summary>
        /// 读多个字节内存
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hAddr"></param>
        /// <param name="readLen"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        unsafe public bool ReadBytes(IntPtr hwnd, int hAddr, uint readLen, byte* outData) {
            int ip = new int();
            int o_sta = W_API.ReadProcessMemory(hwnd, hAddr, outData, readLen, out ip);
            if (o_sta == 0) {
                outData = null;
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 读2字节内存
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hAddr"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        unsafe public bool Read2Byte(IntPtr hwnd, int hAddr, out UInt16 outData) {
            byte[] out_byte = new byte[2];
            IntPtr dataPrt = Marshal.AllocHGlobal(out_byte.Length);

            int ip = new int();
            int o_sta = W_API.ReadProcessMemory(hwnd, hAddr, (byte*)dataPrt, 2, out ip);
            Marshal.Copy((IntPtr)dataPrt, out_byte, 0, out_byte.Length);
            Marshal.FreeHGlobal(dataPrt);

            if (o_sta == 0) {
                outData = 0;
                return false;
            }
            
            
            UInt16 temp = 0;
            temp |= out_byte[0];
            temp |= (UInt16)(out_byte[1] << 8);
            outData = (UInt16)temp;
            return true;
        }


        /// <summary>
        /// 读4字节内存
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hAddr"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        unsafe public bool Read4Byte(IntPtr hwnd, int hAddr, out Int32 outData) {
            byte[] out_byte = new byte[4];
            IntPtr dataPrt = Marshal.AllocHGlobal(out_byte.Length);

            int ip = new int();
            int o_sta = W_API.ReadProcessMemory(hwnd, hAddr, (byte *)dataPrt, 4, out ip);
            Marshal.Copy((IntPtr)dataPrt, out_byte, 0, out_byte.Length);
            Marshal.FreeHGlobal(dataPrt);

            if (o_sta == 0) {
                outData = 0;
                return false;
            }

            UInt32 temp = 0;
            temp |= (UInt32)out_byte[0];
            temp |= ((UInt32)out_byte[1]) << 8;
            temp |= ((UInt32)out_byte[2]) << 16;
            temp |= ((UInt32)out_byte[3]) << 24;
            outData = (Int32)temp;
            return true;
        }
        
        /// <summary>
        /// 写2字节内存
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hAddr"></param>
        /// <param name="inData"></param>
        /// <returns></returns>
        public bool Write2Byte(IntPtr hwnd, int hAddr, UInt16 inData) {
            byte[] temp_byte = new byte[2];
            temp_byte[0] = (byte)inData;
            temp_byte[1] = (byte)(inData >> 8);
            int ip = 0;
            int o_sta = W_API.WriteProcessMemory(hwnd, hAddr, temp_byte, 2, out ip);
            if (o_sta == 0) {
                return false;
            }
            return true;
        }
        
        /// <summary>
        ///  写4字节内存
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hAddr"></param>
        /// <param name="inData"></param>
        /// <returns></returns>
        public bool Write4Byte(IntPtr hwnd, int hAddr, Int32 inData) {
            byte[] temp_byte = new byte[4];
            temp_byte[0] = (byte)inData;
            temp_byte[1] = (byte)(inData >> 8);
            temp_byte[2] = (byte)(inData >> 16);
            temp_byte[3] = (byte)(inData >> 24);
            int ip = 0;
            int o_sta = W_API.WriteProcessMemory(hwnd, hAddr, temp_byte, 4, out ip);
            if (o_sta == 0) {
                return false;
            }
            return true;
        }
    }
}
