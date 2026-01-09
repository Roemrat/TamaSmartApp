using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TamaSmartApp
{
    // SPI Configuration Structure
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SPI_CONFIG
    {
        public byte iMode;                    // 0-3: SPI Mode0/1/2/3
        public byte iClock;                   // 0=60MHz, 1=30MHz, 2=15MHz, 3=7.5MHz, 4=3.75MHz, 5=1.875MHz, 6=937.5KHz, 7=468.75KHz
        public byte iByteOrder;                // 0=LSB first, 1=MSB first
        public ushort iSpiWriteReadInterval;  // Interval in uS
        public byte iSpiOutDefaultData;        // Default data when reading
        public uint iChipSelect;              // Chip select control
        public byte CS1Polarity;              // CS1 polarity
        public byte CS2Polarity;              // CS2 polarity
        public ushort iIsAutoDeativeCS;        // Auto deactivate CS
        public ushort iActiveDelay;            // Active delay in us
        public uint iDelayDeactive;            // Delay after deactivate in us
    }

    public static class CH347DLL
    {
        private const string DLL_NAME = "CH347DLL.DLL";
        private static bool _dllLoaded = false;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        static CH347DLL()
        {
            LoadDLL();
        }

        private static void LoadDLL()
        {
            if (_dllLoaded) return;

            // First, try to extract DLL from embedded resource
            string tempDllPath = ExtractEmbeddedDLL();
            
            // Try multiple paths (prioritize extracted DLL, then root directory)
            string[] paths = new string[]
            {
                tempDllPath,  // Extracted DLL from embedded resource
                AppDomain.CurrentDomain.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib"),
                Path.Combine(Directory.GetCurrentDirectory(), "lib")
            };

            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                
                string dllPath = path;
                if (!Path.IsPathRooted(path))
                {
                    dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                }
                
                if (File.Exists(dllPath))
                {
                    // Set DLL directory so DllImport can find it
                    string dllDir = Path.GetDirectoryName(dllPath);
                    if (!string.IsNullOrEmpty(dllDir))
                    {
                        SetDllDirectory(dllDir);
                    }
                    
                    // Try loading directly
                    IntPtr handle = LoadLibrary(dllPath);
                    if (handle != IntPtr.Zero)
                    {
                        _dllLoaded = true;
                        return;
                    }
                }
            }
        }

        private static string ExtractEmbeddedDLL()
        {
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                
                // Try different possible resource names
                string[] possibleResourceNames = new string[]
                {
                    "CH347DLL.DLL",
                    "TamaSmartApp.CH347DLL.DLL",
                    "lib.CH347DLL.DLL",
                    "TamaSmartApp.lib.CH347DLL.DLL"
                };

                System.IO.Stream? stream = null;
                foreach (string name in possibleResourceNames)
                {
                    stream = assembly.GetManifestResourceStream(name);
                    if (stream != null) break;
                }

                if (stream != null)
                {
                    // Extract to temp directory
                    string tempDir = Path.Combine(Path.GetTempPath(), "TamaSmartApp");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }

                    string dllPath = Path.Combine(tempDir, DLL_NAME);
                    
                    // Only extract if file doesn't exist or is different
                    bool needExtract = true;
                    if (File.Exists(dllPath))
                    {
                        // Check if file size matches
                        long fileSize = new FileInfo(dllPath).Length;
                        if (fileSize == stream.Length)
                        {
                            needExtract = false;
                        }
                    }

                    if (needExtract)
                    {
                        using (FileStream fileStream = new FileStream(dllPath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }

                    stream.Close();
                    return dllPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting embedded DLL: {ex.Message}");
            }

            return string.Empty;
        }

        // Common Functions
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern int CH347OpenDevice(uint iIndex);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347CloseDevice(uint iIndex);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347ReadData(uint iIndex, IntPtr oBuffer, ref uint ioLength);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347WriteData(uint iIndex, IntPtr iBuffer, ref uint ioLength);

        // SPI Functions
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_Init(uint iIndex, ref SPI_CONFIG SpiCfg);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_ChangeCS(uint iIndex, byte iStatus);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_SetChipSelect(uint iIndex, ushort iEnableSelect, ushort iChipSelect, uint iIsAutoDeativeCS, uint iActiveDelay, uint iDelayDeactive);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_Write(uint iIndex, uint iChipSelect, uint iLength, uint iWriteStep, IntPtr ioBuffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_Read(uint iIndex, uint iChipSelect, uint oLength, ref uint iLength, IntPtr ioBuffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347SPI_WriteRead(uint iIndex, uint iChipSelect, uint iLength, IntPtr ioBuffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CH347StreamSPI4(uint iIndex, uint iChipSelect, uint iLength, IntPtr ioBuffer);

        // Helper method to get device count (try opening devices 0-15)
        public static int GetDeviceCount()
        {
            int count = 0;
            for (uint i = 0; i < 16; i++)
            {
                int handle = CH347OpenDevice(i);
                if (handle >= 0)
                {
                    count++;
                    CH347CloseDevice(i);
                }
            }
            return count;
        }
    }
}
