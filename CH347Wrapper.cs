using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TamaSmartApp
{
    public class CH347Wrapper : IDisposable
    {
        private uint _deviceIndex;
        private bool _isOpen = false;
        private bool _spiInitialized = false;
        private byte _currentSpeed = 1; // Track current SPI speed

        public bool IsOpen => _isOpen;
        public uint DeviceIndex => _deviceIndex;
        public byte CurrentSpeed => _currentSpeed;

        public bool OpenDevice(uint deviceIndex)
        {
            if (_isOpen)
            {
                CloseDevice();
            }

            int handle = CH347DLL.CH347OpenDevice(deviceIndex);
            if (handle >= 0)
            {
                _deviceIndex = deviceIndex;
                _isOpen = true;
                return true;
            }

            return false;
        }

        public void CloseDevice()
        {
            if (_isOpen)
            {
                CH347DLL.CH347CloseDevice(_deviceIndex);
                _isOpen = false;
                _spiInitialized = false;
            }
        }

        public bool InitSPI(byte speed = 1) // Default: 30MHz (0=60MHz, 1=30MHz, 2=15MHz, 3=7.5MHz)
        {
            if (!_isOpen) return false;

            SPI_CONFIG config = new SPI_CONFIG
            {
                iMode = 0,                      // SPI Mode 0
                iClock = speed,                 // Speed setting
                iByteOrder = 1,                 // MSB first
                iSpiWriteReadInterval = 0,
                iSpiOutDefaultData = 0,
                iChipSelect = 0,
                CS1Polarity = 0,
                CS2Polarity = 0,
                iIsAutoDeativeCS = 0,
                iActiveDelay = 0,
                iDelayDeactive = 0
            };

            bool result = CH347DLL.CH347SPI_Init(_deviceIndex, ref config);
            if (result)
            {
                _spiInitialized = true;
                _currentSpeed = speed;
            }
            return result;
        }

        public static string GetSpeedString(byte speed)
        {
            switch (speed)
            {
                case 0: return "60MHz";
                case 1: return "30MHz";
                case 2: return "15MHz";
                case 3: return "7.5MHz";
                case 4: return "3.75MHz";
                case 5: return "1.875MHz";
                case 6: return "937.5KHz";
                case 7: return "468.75KHz";
                default: return "Unknown";
            }
        }

        public bool SPIWriteRead(byte[] writeData, out byte[]? readData)
        {
            readData = null;
            if (!_isOpen || !_spiInitialized) return false;

            uint length = (uint)writeData.Length;
            IntPtr buffer = Marshal.AllocHGlobal((int)length);
            try
            {
                Marshal.Copy(writeData, 0, buffer, (int)length);
                bool result = CH347DLL.CH347SPI_WriteRead(_deviceIndex, 0x80, length, buffer);
                if (result)
                {
                    readData = new byte[length];
                    Marshal.Copy(buffer, readData, 0, (int)length);
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }


        public bool SPIWrite(byte[] data, bool autoCS = true)
        {
            if (!_isOpen || !_spiInitialized) return false;

            uint length = (uint)data.Length;
            IntPtr buffer = Marshal.AllocHGlobal((int)length);
            try
            {
                Marshal.Copy(data, 0, buffer, (int)length);
                // autoCS=true: use 0x80 (auto CS), autoCS=false: use 0 (manual CS)
                uint chipSelect = autoCS ? (uint)0x80 : 0;
                return CH347DLL.CH347SPI_Write(_deviceIndex, chipSelect, length, 500, buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public bool SPIRead(uint length, out byte[]? data)
        {
            data = null;
            if (!_isOpen || !_spiInitialized) return false;

            uint readLength = length;
            IntPtr buffer = Marshal.AllocHGlobal((int)length);
            try
            {
                bool result = CH347DLL.CH347SPI_Read(_deviceIndex, 0x80, 0, ref readLength, buffer);
                if (result)
                {
                    data = new byte[readLength];
                    Marshal.Copy(buffer, data, 0, (int)readLength);
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        // Flash Operations
        public bool ReadFlashID(out byte[]? id)
        {
            id = null;
            if (!_isOpen || !_spiInitialized) return false;

            // Send 0x9F (Read ID command)
            byte[] cmd = new byte[] { 0x9F };
            byte[] dummy = new byte[3]; // 3 bytes for ID response
            byte[] writeData = new byte[] { 0x9F, 0x00, 0x00, 0x00 };

            if (SPIWriteRead(writeData, out byte[]? readData))
            {
                if (readData != null && readData.Length >= 4)
                {
                    id = new byte[3];
                    Array.Copy(readData, 1, id, 0, 3);
                    return true;
                }
            }
            return false;
        }

        public bool ReadFlash(uint address, uint length, out byte[]? data, Action<uint, uint>? progressCallback = null)
        {
            data = null;
            if (!_isOpen || !_spiInitialized) return false;

            // Use method similar to AsProgrammer: Write command+address, then Read data
            // But use smaller chunks first to test, then increase if working
            List<byte> result = new List<byte>();
            uint remaining = length;
            uint currentAddr = address;

            while (remaining > 0)
            {
                // Start with smaller chunks (4096 bytes) for reliability, can increase later
                uint chunkSize = Math.Min(remaining, 4096);
                
                // Step 1: Send 0x03 (Read command) + 24-bit address
                byte[] cmd = new byte[4];
                cmd[0] = 0x03; // Read command
                cmd[1] = (byte)((currentAddr >> 16) & 0xFF);
                cmd[2] = (byte)((currentAddr >> 8) & 0xFF);
                cmd[3] = (byte)(currentAddr & 0xFF);

                // Write command+address with CS=0 (manual CS control, like AsProgrammer)
                // This keeps CS low for the read operation
                CH347DLL.CH347SPI_ChangeCS(_deviceIndex, 0); // CS low
                if (!SPIWrite(cmd, autoCS: false))
                {
                    CH347DLL.CH347SPI_ChangeCS(_deviceIndex, (byte)1); // CS high (cleanup)
                    System.Diagnostics.Debug.WriteLine($"ReadFlash: SPIWrite failed at address 0x{currentAddr:X6}");
                    return false;
                }

                // Step 2: Read data using CH347SPI_Read with CS=1 ($80) - auto CS
                if (SPIRead(chunkSize, out byte[]? readData))
                {
                    if (readData != null && readData.Length == chunkSize)
                    {
                        result.AddRange(readData);
                    }
                    else
                    {
                        CH347DLL.CH347SPI_ChangeCS(_deviceIndex, (byte)1); // CS high (cleanup)
                        System.Diagnostics.Debug.WriteLine($"ReadFlash: Invalid readData length. Expected: {chunkSize}, Got: {readData?.Length ?? 0}");
                        return false;
                    }
                }
                else
                {
                    CH347DLL.CH347SPI_ChangeCS(_deviceIndex, (byte)1); // CS high (cleanup)
                    System.Diagnostics.Debug.WriteLine($"ReadFlash: SPIRead failed at address 0x{currentAddr:X6}, chunkSize: {chunkSize}");
                    return false;
                }

                // CS will be deactivated by CH347SPI_Read with $80

                currentAddr += chunkSize;
                remaining -= chunkSize;
                
                // Report progress
                if (progressCallback != null)
                {
                    uint totalRead = length - remaining;
                    progressCallback(totalRead, length);
                }
            }

            data = result.ToArray();
            return true;
        }

        public bool WriteEnable()
        {
            if (!_isOpen || !_spiInitialized) return false;
            byte[] cmd = new byte[] { 0x06 }; // Write Enable
            return SPIWrite(cmd);
        }

        public bool WaitReady()
        {
            if (!_isOpen || !_spiInitialized) return false;

            // Most flash chips are ready within 1-5ms, but some can take up to 100ms
            // Use shorter timeout and faster polling for better performance
            int timeout = 200; // 200ms timeout (reduced from 1000ms)
            int checkCount = 0;
            
            while (timeout-- > 0)
            {
                byte[] cmd = new byte[] { 0x05, 0x00 }; // Read Status Register
                if (SPIWriteRead(cmd, out byte[]? status))
                {
                    if (status != null && status.Length >= 2)
                    {
                        if ((status[1] & 0x01) == 0) // Bit 0 = Busy flag (0 = ready)
                        {
                            return true; // Ready
                        }
                    }
                }
                
                // Poll faster: first few checks with no delay, then small delay
                if (checkCount++ > 10)
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            return false; // Timeout
        }

        public bool EraseSector(uint address)
        {
            if (!_isOpen || !_spiInitialized) return false;

            if (!WriteEnable()) return false;

            // Sector Erase (0x20) - 4KB sector
            byte[] cmd = new byte[4];
            cmd[0] = 0x20;
            cmd[1] = (byte)((address >> 16) & 0xFF);
            cmd[2] = (byte)((address >> 8) & 0xFF);
            cmd[3] = (byte)(address & 0xFF);

            if (!SPIWrite(cmd, autoCS: true)) return false;

            return WaitReady();
        }

        public bool EraseChip()
        {
            if (!_isOpen || !_spiInitialized) return false;

            if (!WriteEnable()) return false;

            // Chip Erase (0xC7 or 0x60)
            byte[] cmd = new byte[] { 0xC7 };
            if (!SPIWrite(cmd, autoCS: true)) return false;

            // Chip erase takes longer, use longer timeout
            int timeout = 30000; // 30 seconds
            while (timeout-- > 0)
            {
                if (WaitReady()) return true;
                System.Threading.Thread.Sleep(100); // Check every 100ms
            }
            return false;
        }

        public bool Unprotect()
        {
            if (!_isOpen || !_spiInitialized) return false;

            // Write Status Register to 0x00 (clear all protection bits)
            if (!WriteEnable()) return false;

            byte[] cmd = new byte[] { 0x01, 0x00 }; // WRSR command + status = 0x00
            if (!SPIWrite(cmd, autoCS: true)) return false;

            return WaitReady();
        }

        public bool Protect()
        {
            if (!_isOpen || !_spiInitialized) return false;

            // Write Status Register with all protection bits set
            // WRITE_PROTECT | BP2 | BP1 | BP0 | TB | SEC = 0xFC
            if (!WriteEnable()) return false;

            byte[] cmd = new byte[] { 0x01, 0xFC }; // WRSR command + status = 0xFC
            if (!SPIWrite(cmd, autoCS: true)) return false;

            return WaitReady();
        }

        public bool ProtectXC2X14()
        {
            if (!_isOpen || !_spiInitialized) return false;

            // Special protection for chip with Vendor 0xC2, Capacity 0x14
            // WRITE_PROTECT | BP2 | BP1 | BP0 = 0x9C (no TB, no SEC)
            if (!WriteEnable()) return false;

            byte[] cmd = new byte[] { 0x01, 0x9C }; // WRSR command + status = 0x9C
            if (!SPIWrite(cmd, autoCS: true)) return false;

            return WaitReady();
        }

        public bool ReadBytes(uint address, uint length, out byte[]? data)
        {
            return ReadFlash(address, length, out data);
        }

        public bool WriteBytes(uint address, byte[] data)
        {
            return WriteFlash(address, data);
        }

        public bool VerifyFlash(uint address, byte[] expectedData, Action<uint, uint>? progressCallback = null)
        {
            if (!_isOpen || !_spiInitialized) return false;

            // Read back the data with progress reporting
            if (ReadFlash(address, (uint)expectedData.Length, out byte[]? readData, (read, total) =>
            {
                // Report reading progress (50% of verify process)
                if (progressCallback != null)
                {
                    progressCallback(read, total);
                }
            }))
            {
                if (readData != null && readData.Length == expectedData.Length)
                {
                    // Compare byte by byte with progress reporting
                    // This is fast because it's just memory comparison
                    for (int i = 0; i < expectedData.Length; i++)
                    {
                        if (readData[i] != expectedData[i])
                        {
                            System.Diagnostics.Debug.WriteLine($"Verify failed at offset {i}: expected 0x{expectedData[i]:X2}, got 0x{readData[i]:X2}");
                            return false;
                        }
                        
                        // Report comparison progress every 1024 bytes
                        if (progressCallback != null && (i % 1024 == 0 || i == expectedData.Length - 1))
                        {
                            progressCallback((uint)(i + 1), (uint)expectedData.Length);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool WriteFlash(uint address, byte[] data, Action<uint, uint>? progressCallback = null)
        {
            if (!_isOpen || !_spiInitialized) return false;

            uint totalSize = (uint)data.Length;
            // Write in pages of 256 bytes
            uint remaining = totalSize;
            uint currentAddr = address;
            int offset = 0;

            while (remaining > 0)
            {
                uint pageSize = Math.Min(remaining, 256);
                uint pageStart = currentAddr % 256;
                uint writeSize = Math.Min(pageSize, 256 - pageStart);

                // Skip pages that are all 0xFF (already erased, like AsProgrammer)
                // This significantly speeds up writing if file contains many 0xFF bytes
                bool skipPage = true;
                for (int i = 0; i < writeSize; i++)
                {
                    if (data[offset + i] != 0xFF)
                    {
                        skipPage = false;
                        break;
                    }
                }

                if (!skipPage)
                {
                    if (!WriteEnable()) return false;

                    // Page Program (0x02)
                    byte[] cmd = new byte[4 + writeSize];
                    cmd[0] = 0x02;
                    cmd[1] = (byte)((currentAddr >> 16) & 0xFF);
                    cmd[2] = (byte)((currentAddr >> 8) & 0xFF);
                    cmd[3] = (byte)(currentAddr & 0xFF);
                    Array.Copy(data, offset, cmd, 4, (int)writeSize);

                    if (!SPIWrite(cmd, autoCS: true)) return false;
                    
                    // Wait for write to complete (most chips are ready in 1-5ms)
                    if (!WaitReady()) return false;
                }

                currentAddr += writeSize;
                offset += (int)writeSize;
                remaining -= writeSize;
                
                // Report progress
                if (progressCallback != null)
                {
                    uint totalWritten = totalSize - remaining;
                    progressCallback(totalWritten, totalSize);
                }
            }

            return true;
        }

        public void Dispose()
        {
            CloseDevice();
        }
    }
}
