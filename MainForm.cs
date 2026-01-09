using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Linq;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace TamaSmartApp
{
    public partial class MainForm : Form
    {
        private CH347Wrapper ch347;
        private bool isConnected = false;
        private ChipInfo? currentChipInfo = null;

        public MainForm()
        {
            InitializeComponent();
            ch347 = new CH347Wrapper();
            RefreshDeviceList();
        }

        private void RefreshDeviceList()
        {
            try
            {
                int count = CH347DLL.GetDeviceCount();
                deviceCountLabel.Text = $"พบอุปกรณ์: {count} เครื่อง";

                deviceComboBox.Items.Clear();
                
                // Get COM ports for CH347 devices
                var comPorts = GetCH347ComPorts();
                
                for (int i = 0; i < count; i++)
                {
                    string displayText;
                    if (i < comPorts.Count && !string.IsNullOrEmpty(comPorts[i]))
                    {
                        displayText = $"COM{comPorts[i]}";
                    }
                    else
                    {
                        displayText = $"Device {i}";
                    }
                    deviceComboBox.Items.Add(displayText);
                }

                if (count > 0)
                {
                    deviceComboBox.SelectedIndex = 0;
                    AddLog($"✅ พบ CH347 {count} เครื่อง", "success");
                }
                else
                {
                    AddLog("⚠️ ไม่พบอุปกรณ์ CH347", "warning");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ ข้อผิดพลาด: {ex.Message}", "error");
            }
        }

        private System.Collections.Generic.List<string> GetCH347ComPorts()
        {
            var comPorts = new System.Collections.Generic.List<string>();
            
            try
            {
                // Method 1: Use Registry to find COM ports for CH347 (VID_1A86, PID_55XX)
                string registryPath = @"SYSTEM\CurrentControlSet\Enum\USB";
                using (RegistryKey usbKey = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (usbKey != null)
                    {
                        foreach (string vidPidKey in usbKey.GetSubKeyNames())
                        {
                            // Look for VID_1A86&PID_55XX (CH347)
                            if (vidPidKey.Contains("VID_1A86") && vidPidKey.Contains("PID_55"))
                            {
                                using (RegistryKey deviceKey = usbKey.OpenSubKey(vidPidKey))
                                {
                                    if (deviceKey != null)
                                    {
                                        foreach (string instanceKey in deviceKey.GetSubKeyNames())
                                        {
                                            using (RegistryKey instance = deviceKey.OpenSubKey(instanceKey))
                                            {
                                                if (instance != null)
                                                {
                                                    // Check Device Parameters for PortName
                                                    using (RegistryKey paramsKey = instance.OpenSubKey("Device Parameters"))
                                                    {
                                                        if (paramsKey != null)
                                                        {
                                                            object portName = paramsKey.GetValue("PortName");
                                                            if (portName != null)
                                                            {
                                                                string port = portName.ToString();
                                                                if (port.StartsWith("COM"))
                                                                {
                                                                    var match = System.Text.RegularExpressions.Regex.Match(port, @"COM(\d+)");
                                                                    if (match.Success)
                                                                    {
                                                                        string portNum = match.Groups[1].Value;
                                                                        if (!comPorts.Contains(portNum))
                                                                        {
                                                                            comPorts.Add(portNum);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Method 2: If not found, try WMI Win32_SerialPort
                if (comPorts.Count == 0)
                {
                    using (var searcher = new ManagementObjectSearcher(
                        "SELECT DeviceID, PNPDeviceID, Description FROM Win32_SerialPort"))
                    {
                        var devices = searcher.Get().Cast<ManagementObject>()
                            .Where(d => 
                            {
                                var pnpId = d["PNPDeviceID"]?.ToString() ?? "";
                                var desc = d["Description"]?.ToString() ?? "";
                                // CH347 typically has VID_1A86 (WCH) and PID_55XX
                                return (pnpId.Contains("VID_1A86") && pnpId.Contains("PID_55")) ||
                                       (desc.IndexOf("CH347", StringComparison.OrdinalIgnoreCase) >= 0);
                            })
                            .OrderBy(d => d["DeviceID"]?.ToString() ?? "")
                            .ToList();

                        foreach (var device in devices)
                        {
                            var deviceId = device["DeviceID"]?.ToString() ?? "";
                            if (deviceId.Contains("COM"))
                            {
                                var match = System.Text.RegularExpressions.Regex.Match(deviceId, @"COM(\d+)");
                                if (match.Success)
                                {
                                    string portNum = match.Groups[1].Value;
                                    if (!comPorts.Contains(portNum))
                                    {
                                        comPorts.Add(portNum);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If all methods fail, return empty list (will fallback to Device X)
                System.Diagnostics.Debug.WriteLine($"Error getting COM ports: {ex.Message}");
            }

            return comPorts;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                DisconnectDevice();
            }
            else
            {
                ConnectDevice();
            }
        }

        private void ConnectDevice()
        {
            try
            {
                if (deviceComboBox.SelectedIndex < 0)
                {
                    AddLog("⚠️ กรุณาเลือกอุปกรณ์", "warning");
                    return;
                }

                uint deviceIndex = (uint)deviceComboBox.SelectedIndex;
                AddLog($"กำลังเชื่อมต่อ Device {deviceIndex}...", "info");

                if (ch347.OpenDevice(deviceIndex))
                {
                    if (ch347.InitSPI(1)) // 30MHz (balanced speed and reliability)
                    {
                        isConnected = true;
                        connectButton.Text = "Disconnect";
                        connectButton.BackColor = System.Drawing.Color.OrangeRed;
                        deviceComboBox.Enabled = false;
                        refreshButton.Enabled = false;
                        AddLog($"✅ เชื่อมต่อสำเร็จ (Device {deviceIndex})", "success");
                        
                        // Auto-read Flash ID after connection
                        ReadFlashID();
                    }
                    else
                    {
                        ch347.CloseDevice();
                        AddLog("❌ ไม่สามารถ Initialize SPI ได้", "error");
                    }
                }
                else
                {
                    AddLog("❌ ไม่สามารถเปิดอุปกรณ์ได้", "error");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ ข้อผิดพลาด: {ex.Message}", "error");
            }
        }

        private void DisconnectDevice()
        {
            ch347.CloseDevice();
            isConnected = false;
            currentChipInfo = null;
            connectButton.Text = "Connect";
            connectButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            deviceComboBox.Enabled = true;
            refreshButton.Enabled = true;
            
            // Clear chip info display
            icNameLabel.Text = "-";
            chipSizeLabel.Text = "-";
            chipThemeLabel.Text = "-";
            flashIdLabel.Text = "Chip ID: -";
            
            AddLog("🔌 ตัดการเชื่อมต่อแล้ว", "info");
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshDeviceList();
        }

        private void readIdButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                AddLog("⚠️ กรุณาเชื่อมต่ออุปกรณ์ก่อน", "warning");
                return;
            }

            ReadFlashID();
        }

        private void ReadFlashID()
        {
            try
            {
                AddLog("กำลังอ่าน Chip ID...", "info");
                if (ch347.ReadFlashID(out byte[]? id) && id != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Chip ID: ");
                    foreach (byte b in id)
                    {
                        sb.Append($"{b:X2} ");
                    }
                    AddLog($"✅ {sb.ToString()}", "success");
                    flashIdLabel.Text = sb.ToString();

                    // Find chip info from database
                    var matchingChips = ChipDatabase.FindAllByFlashId(id);
                    if (matchingChips.Count > 0)
                    {
                        if (matchingChips.Count == 1)
                        {
                            // Single match - use it directly
                            currentChipInfo = matchingChips[0];
                        }
                        else
                        {
                            // Multiple matches - show selection dialog
                            using (FindChipDialog dialog = new FindChipDialog(matchingChips))
                            {
                                if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedChip != null)
                                {
                                    currentChipInfo = dialog.SelectedChip;
                                }
                                else
                                {
                                    // User cancelled - use first match
                                    currentChipInfo = matchingChips[0];
                                    AddLog("⚠️ ใช้ Chip แรกที่พบ (ผู้ใช้ยกเลิกการเลือก)", "warning");
                                }
                            }
                        }

                        if (currentChipInfo != null)
                        {
                            icNameLabel.Text = currentChipInfo.Name;
                            chipSizeLabel.Text = currentChipInfo.Size.ToString();
                            AddLog($"✅ พบ Chip: {currentChipInfo.Name} ({currentChipInfo.Manufacturer})", "success");
                            AddLog($"   Size: {currentChipInfo.SizeFormatted} ({currentChipInfo.Size} bytes)", "info");
                            
                            // Read theme from address 0x32
                            ReadTheme();
                        }
                    }
                    else
                    {
                        currentChipInfo = null;
                        icNameLabel.Text = "Unknown";
                        chipSizeLabel.Text = "-";
                        chipThemeLabel.Text = "-";
                        AddLog("⚠️ ไม่พบข้อมูล Chip ในฐานข้อมูล", "warning");
                    }
                }
                else
                {
                    AddLog("❌ ไม่สามารถอ่าน Chip ID ได้", "error");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ ข้อผิดพลาด: {ex.Message}", "error");
            }
        }

        private void readFlashButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                AddLog("⚠️ กรุณาเชื่อมต่ออุปกรณ์ก่อน", "warning");
                return;
            }

            if (currentChipInfo == null)
            {
                AddLog("⚠️ กรุณาอ่าน Chip ID ก่อนเพื่อระบุ Chip", "warning");
                return;
            }

            try
            {
                uint address = 0;
                uint length = currentChipInfo.Size;

                AddLog($"กำลังอ่าน Chip ทั้งหมด จำนวน {length} bytes ({currentChipInfo.SizeFormatted})...", "info");
                
                // Show progress bar
                progressBar.Visible = true;
                progressBar.Minimum = 0;
                progressBar.Maximum = (int)length;
                progressBar.Value = 0;
                progressLabel.Visible = true;
                progressLabel.Text = "0 / 0 bytes";
                readFlashButton.Enabled = false;
                writeFlashButton.Enabled = false;
                eraseButton.Enabled = false;
                resetButton.Enabled = false;
                Application.DoEvents();

                if (ch347.ReadFlash(address, length, out byte[]? data, (read, total) =>
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            progressBar.Value = (int)read;
                            progressLabel.Text = $"{read:N0} / {total:N0} bytes ({(read * 100 / total):F1}%)";
                            Application.DoEvents();
                        }));
                    }
                    else
                    {
                        progressBar.Value = (int)read;
                        progressLabel.Text = $"{read:N0} / {total:N0} bytes ({(read * 100 / total):F1}%)";
                        Application.DoEvents();
                    }
                }) && data != null)
                {
                    progressBar.Value = progressBar.Maximum;
                    progressLabel.Text = $"✅ อ่านเสร็จสิ้น: {data.Length:N0} bytes";
                    Application.DoEvents();

                    // Log การอ่านเสร็จสิ้น
                    AddLog($"✅ อ่าน Chip สำเร็จ: {data.Length:N0} bytes ({currentChipInfo.SizeFormatted})", "success");

                    // แสดง dialog สำหรับบันทึกไฟล์
                    string appPath = Application.StartupPath;
                    string defaultDir = Path.Combine(appPath, "Backups");
                    if (!Directory.Exists(defaultDir))
                    {
                        Directory.CreateDirectory(defaultDir);
                    }

                    SaveFileDialog saveDialog = new SaveFileDialog
                    {
                        Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*",
                        FileName = $"{currentChipInfo.Name}_{length}.bin",
                        InitialDirectory = defaultDir
                    };

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            File.WriteAllBytes(saveDialog.FileName, data);
                            AddLog($"💾 บันทึกไฟล์สำเร็จ: {saveDialog.FileName}", "success");
                        }
                        catch (Exception ex)
                        {
                            AddLog($"❌ ไม่สามารถบันทึกไฟล์ได้: {ex.Message}", "error");
                        }
                    }
                    else
                    {
                        AddLog("ℹ️ ยกเลิกการบันทึกไฟล์", "info");
                    }
                }
                else
                {
                    AddLog("❌ ไม่สามารถอ่าน Chip ได้", "error");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ ข้อผิดพลาด: {ex.Message}", "error");
            }
            finally
            {
                progressBar.Visible = false;
                progressLabel.Visible = false;
                readFlashButton.Enabled = true;
                writeFlashButton.Enabled = true;
                eraseButton.Enabled = true;
                resetButton.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void writeFlashButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                AddLog("⚠️ กรุณาเชื่อมต่ออุปกรณ์ก่อน", "warning");
                return;
            }

            if (currentChipInfo == null)
            {
                AddLog("⚠️ กรุณาอ่าน Chip ID ก่อนเพื่อระบุ Chip", "warning");
                return;
            }

            try
            {
                string appPath = Application.StartupPath;
                string defaultDir = Path.Combine(appPath, "Backups");
                if (!Directory.Exists(defaultDir))
                {
                    Directory.CreateDirectory(defaultDir);
                }

                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*",
                    InitialDirectory = defaultDir
                };

                if (openDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                uint address = 0;
                byte[] data = File.ReadAllBytes(openDialog.FileName);

                if (data.Length > currentChipInfo.Size)
                {
                    AddLog($"⚠️ ไฟล์ใหญ่เกินไป! ขนาดสูงสุด: {currentChipInfo.SizeFormatted} ({currentChipInfo.Size} bytes)", "warning");
                    return;
                }

                AddLog($"กำลังเขียน Chip ที่ Address 0x{address:X6} จำนวน {data.Length} bytes...", "info");
                
                // Show progress bar
                // Progress is divided into 4 steps, each step = data.Length
                progressBar.Visible = true;
                progressBar.Minimum = 0;
                progressBar.Maximum = data.Length * 4; // Unprotect + Erase + Write + Verify
                progressBar.Value = 0;
                progressLabel.Visible = true;
                readFlashButton.Enabled = false;
                writeFlashButton.Enabled = false;
                eraseButton.Enabled = false;
                resetButton.Enabled = false;
                Application.DoEvents();

                bool success = true;
                int currentStep = 0;

                // Step 1: Unprotect
                progressLabel.Text = "ขั้นตอนที่ 1/4: ยกเลิกการป้องกัน...";
                Application.DoEvents();
                AddLog("🔓 กำลังยกเลิกการป้องกัน Flash...", "info");
                if (ch347.Unprotect())
                {
                    AddLog("✅ ยกเลิกการป้องกันสำเร็จ", "success");
                    currentStep += data.Length;
                    progressBar.Value = currentStep;
                }
                else
                {
                    AddLog("⚠️ ไม่สามารถยกเลิกการป้องกันได้ (อาจจะไม่ได้ป้องกันอยู่แล้ว)", "info");
                    currentStep += data.Length;
                    progressBar.Value = currentStep;
                }

                // Step 2: Erase
                progressLabel.Text = "ขั้นตอนที่ 2/4: กำลังลบข้อมูล...";
                Application.DoEvents();
                AddLog("🗑️ กำลังลบข้อมูล Flash...", "info");
                if (ch347.EraseChip())
                {
                    AddLog("✅ ลบข้อมูลสำเร็จ", "success");
                    currentStep += data.Length;
                    progressBar.Value = currentStep;
                }
                else
                {
                    AddLog("❌ ไม่สามารถลบข้อมูลได้", "error");
                    success = false;
                }

                if (success)
                {
                    // Step 3: Write
                    progressLabel.Text = "ขั้นตอนที่ 3/4: กำลังเขียนข้อมูล...";
                    Application.DoEvents();
                    AddLog("✍️ กำลังเขียนข้อมูล Flash...", "info");
                    if (ch347.WriteFlash(address, data, (written, total) =>
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                int writeProgress = currentStep + (int)written;
                                progressBar.Value = writeProgress;
                                progressLabel.Text = $"ขั้นตอนที่ 3/4: กำลังเขียน... {written:N0} / {total:N0} bytes ({(written * 100 / total):F1}%)";
                                Application.DoEvents();
                            }));
                        }
                        else
                        {
                            int writeProgress = currentStep + (int)written;
                            progressBar.Value = writeProgress;
                            progressLabel.Text = $"ขั้นตอนที่ 3/4: กำลังเขียน... {written:N0} / {total:N0} bytes ({(written * 100 / total):F1}%)";
                            Application.DoEvents();
                        }
                    }))
                    {
                        currentStep += data.Length;
                        progressBar.Value = currentStep;
                        AddLog($"✅ เขียน Chip สำเร็จ: {data.Length} bytes", "success");
                    }
                    else
                    {
                        AddLog("❌ ไม่สามารถเขียน Chip ได้", "error");
                        success = false;
                    }
                }

                if (success)
                {
                    // Step 4: Verify
                    progressLabel.Text = "ขั้นตอนที่ 4/4: กำลังตรวจสอบข้อมูล...";
                    Application.DoEvents();
                    AddLog("🔍 กำลังตรวจสอบข้อมูลที่เขียน...", "info");
                    if (ch347.VerifyFlash(address, data, (verified, total) =>
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                int verifyProgress = currentStep + (int)verified;
                                progressBar.Value = verifyProgress;
                                progressLabel.Text = $"ขั้นตอนที่ 4/4: กำลังตรวจสอบ... {verified:N0} / {total:N0} bytes ({(verified * 100 / total):F1}%)";
                                Application.DoEvents();
                            }));
                        }
                        else
                        {
                            int verifyProgress = currentStep + (int)verified;
                            progressBar.Value = verifyProgress;
                            progressLabel.Text = $"ขั้นตอนที่ 4/4: กำลังตรวจสอบ... {verified:N0} / {total:N0} bytes ({(verified * 100 / total):F1}%)";
                            Application.DoEvents();
                        }
                    }))
                    {
                        progressBar.Value = progressBar.Maximum;
                        progressLabel.Text = $"✅ เสร็จสิ้น: {data.Length:N0} bytes";
                        AddLog("✅ ตรวจสอบข้อมูลสำเร็จ - ข้อมูลถูกต้อง", "success");
                    }
                    else
                    {
                        AddLog("❌ ตรวจสอบข้อมูลล้มเหลว - ข้อมูลไม่ตรงกัน", "error");
                        success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ ข้อผิดพลาด: {ex.Message}", "error");
            }
            finally
            {
                progressBar.Visible = false;
                progressLabel.Visible = false;
                readFlashButton.Enabled = true;
                writeFlashButton.Enabled = true;
                eraseButton.Enabled = true;
                resetButton.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void eraseButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                AddLog("⚠️ กรุณาเชื่อมต่ออุปกรณ์ก่อน", "warning");
                return;
            }

            if (currentChipInfo == null)
            {
                AddLog("⚠️ กรุณาอ่าน Chip ID ก่อนเพื่อระบุ Chip", "warning");
                return;
            }

            try
            {
                AddLog("🗑️ เริ่มลบข้อมูลทั้งหมดใน Flash...", "info");
                AddLog("⚠️ คำเตือน: การดำเนินการนี้จะลบข้อมูลทั้งหมดและไม่สามารถกู้คืนได้!", "warning");
                {
                    this.Cursor = Cursors.WaitCursor;
                    eraseButton.Enabled = false;
                    readFlashButton.Enabled = false;
                    writeFlashButton.Enabled = false;
                    resetButton.Enabled = false;

                    // Show progress bar
                    progressBar.Visible = true;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = 100;
                    progressBar.Value = 0;
                    progressBar.Style = ProgressBarStyle.Marquee; // Use marquee for indeterminate progress
                    progressLabel.Visible = true;
                    progressLabel.Text = "กำลังลบข้อมูลทั้งหมด...";
                    Application.DoEvents();

                    // Step 1: Unprotect
                    progressLabel.Text = "ขั้นตอนที่ 1/2: กำลังยกเลิกการป้องกัน...";
                    Application.DoEvents();
                    AddLog("🔓 กำลังยกเลิกการป้องกัน...", "info");
                    if (ch347.Unprotect())
                    {
                        AddLog("✅ ยกเลิกการป้องกันสำเร็จ", "success");
                    }
                    else
                    {
                        AddLog("⚠️ ไม่สามารถยกเลิกการป้องกันได้ (อาจจะไม่ได้ป้องกันอยู่แล้ว)", "warning");
                    }
                    Application.DoEvents();

                    // Step 2: Erase Chip (ลบข้อมูลทั้งหมด)
                    progressLabel.Text = "ขั้นตอนที่ 2/2: กำลังลบข้อมูลทั้งหมด... (อาจใช้เวลานาน)";
                    Application.DoEvents();
                    AddLog("🗑️ กำลังลบข้อมูลทั้งหมดใน Flash...", "info");
                    AddLog("   ⏳ กรุณารอสักครู่ (อาจใช้เวลานาน)...", "info");
                    
                    // Erase chip (this will take time, but we can't run it in background thread due to CH347DLL)
                    bool success = ch347.EraseChip();
                    
                    // Update progress bar
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Value = 100;
                    progressLabel.Text = success ? "✅ ลบข้อมูลทั้งหมดสำเร็จ" : "❌ ไม่สามารถลบข้อมูลทั้งหมดได้";
                    Application.DoEvents();
                    
                    if (success)
                    {
                        AddLog("✅ ลบข้อมูลทั้งหมดสำเร็จ", "success");
                    }
                    else
                    {
                        AddLog("❌ ไม่สามารถลบข้อมูลทั้งหมดได้", "error");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ ข้อผิดพลาด: {ex.Message}", "error");
            }
            finally
            {
                progressBar.Visible = false;
                progressLabel.Visible = false;
                eraseButton.Enabled = true;
                readFlashButton.Enabled = true;
                writeFlashButton.Enabled = true;
                resetButton.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                AddLog("⚠️ กรุณาเชื่อมต่ออุปกรณ์ก่อน", "warning");
                return;
            }

            if (currentChipInfo == null)
            {
                AddLog("⚠️ กรุณาอ่าน Chip ID ก่อนเพื่อระบุ Chip", "warning");
                return;
            }

            AddLog("🔄 เริ่ม Reset Tamagotchi Memory...", "info");
            AddLog("   - ตรวจสอบว่าเป็น Tamagotchi data", "info");
            AddLog("   - Reset memory กลับเป็นสถานะเริ่มต้น", "info");
            AddLog("   - Lock chip หลังเสร็จ", "info");

            try
            {
                this.Cursor = Cursors.WaitCursor;
                resetButton.Enabled = false;
                readFlashButton.Enabled = false;
                writeFlashButton.Enabled = false;
                eraseButton.Enabled = false;
                Application.DoEvents();

                // Step 1: Read theme from address 0x32
                AddLog("📖 กำลังอ่าน Theme...", "info");
                if (!ch347.ReadBytes(0x32, 1, out byte[]? themeData) || themeData == null || themeData.Length < 1)
                {
                    AddLog("❌ ไม่สามารถอ่าน Theme ได้", "error");
                    return;
                }

                byte themeValue = themeData[0];
                string[] themes = {
                    "", "", "1996Friends", "Rainbow", "Sweets", "Nizoo", "Cosmetic",
                    "Gourmet", "Pastel", "Melody", "Sanrio", "Marine", "PuiPuiMolcar",
                    "Magical", "OnePiece", "Anniversary", "Kei-Tama", "En-Tam", "Pixar"
                };
                string themeName = (themeValue < themes.Length) ? themes[themeValue] : "Unknown";
                AddLog($"✅ Theme: {themeValue} ({themeName})", "success");

                // Step 2: Validation - Check bytes at 0x10-0x20
                AddLog("🔍 กำลังตรวจสอบ Tama Smart Card data...", "info");
                if (!ValidateTamagotchiData())
                {
                    AddLog("❌ Validation ล้มเหลว - ไม่ใช่ Tama Smart Card data", "error");
                    AddLog("⚠️ กรุณาตรวจสอบว่าเป็น Tamagotchi SMA card", "warning");
                    return;
                }

                AddLog("✅ Validation สำเร็จ - พบ Tama Smart Card data", "success");

                // Step 3: Unlock protection
                AddLog("🔓 กำลังยกเลิกการป้องกัน...", "info");
                if (!ch347.Unprotect())
                {
                    AddLog("⚠️ ไม่สามารถยกเลิกการป้องกันได้ (อาจจะไม่ได้ป้องกันอยู่แล้ว)", "warning");
                }
                else
                {
                    AddLog("✅ ยกเลิกการป้องกันสำเร็จ", "success");
                }

                // Step 4: Read header (64 bytes from 0x00-0x3F)
                AddLog("📖 กำลังอ่าน Header...", "info");
                const uint headerAddr = 0x00;
                const int headerSize = 0x40;
                if (!ch347.ReadBytes(headerAddr, headerSize, out byte[]? header) || 
                    header == null || header.Length < headerSize)
                {
                    AddLog("❌ ไม่สามารถอ่าน Header ได้", "error");
                    return;
                }

                // Step 5: Modify header[0x04..0x10] = 0x00
                AddLog("✏️ กำลังแก้ไข Header...", "info");
                for (int i = 0x04; i < 0x10; i++)
                {
                    header[i] = 0x00;
                }

                // Step 6: Calculate MD5 of header[0x00..0x3F]
                AddLog("🔐 กำลังคำนวณ MD5...", "info");
                byte[] digest;
                using (MD5 md5 = MD5.Create())
                {
                    digest = md5.ComputeHash(header, 0, headerSize);
                }
                AddLog($"✅ MD5: {BitConverter.ToString(digest).Replace("-", " ")}", "success");

                // Step 7: Erase first sector (4KB at 0x000000)
                AddLog("🗑️ กำลังลบ Sector แรก (4KB)...", "info");
                if (!ch347.EraseSector(0x000000))
                {
                    AddLog("❌ ไม่สามารถลบ Sector ได้", "error");
                    return;
                }
                AddLog("✅ ลบ Sector สำเร็จ", "success");

                // Step 8: Verify erase (first 16 bytes should be 0xFF)
                AddLog("🔍 กำลังตรวจสอบการลบ...", "info");
                if (!ch347.ReadBytes(0x00, 16, out byte[]? erasedCheck) || 
                    erasedCheck == null || erasedCheck.Length < 16)
                {
                    AddLog("❌ ไม่สามารถตรวจสอบการลบได้", "error");
                    return;
                }

                bool allFF = true;
                for (int i = 0; i < 16; i++)
                {
                    if (erasedCheck[i] != 0xFF)
                    {
                        allFF = false;
                        break;
                    }
                }

                if (!allFF)
                {
                    AddLog("⚠️ การลบอาจไม่สมบูรณ์ (ไม่ใช่ 0xFF ทั้งหมด)", "warning");
                }
                else
                {
                    AddLog("✅ ตรวจสอบการลบสำเร็จ", "success");
                }

                // Step 9: Write header back (64 bytes at 0x00-0x3F)
                AddLog("✍️ กำลังเขียน Header กลับ...", "info");
                if (!ch347.WriteBytes(0x00, header))
                {
                    AddLog("❌ ไม่สามารถเขียน Header ได้", "error");
                    return;
                }
                AddLog("✅ เขียน Header สำเร็จ", "success");

                // Step 10: Write MD5 digest (16 bytes at 0x40-0x4F)
                AddLog("✍️ กำลังเขียน MD5 Digest...", "info");
                if (!ch347.WriteBytes(0x40, digest))
                {
                    AddLog("❌ ไม่สามารถเขียน MD5 Digest ได้", "error");
                    return;
                }
                AddLog("✅ เขียน MD5 Digest สำเร็จ", "success");

                // Step 11: Zero-fill from 0x50 to 0x1000
                AddLog("✍️ กำลัง Zero-fill (0x50-0x1000)...", "info");
                const uint zeroStart = 0x50;
                const uint zeroEnd = 0x1000;
                uint pageSize = (uint)(currentChipInfo?.Page ?? 256);
                byte[] zeroBuf = new byte[pageSize];
                Array.Clear(zeroBuf, 0, zeroBuf.Length);

                uint addr = zeroStart;
                while (addr < zeroEnd)
                {
                    uint remaining = zeroEnd - addr;
                    uint chunk = Math.Min(remaining, pageSize);
                    byte[] chunkData = new byte[chunk];
                    Array.Copy(zeroBuf, chunkData, chunk);

                    if (!ch347.WriteBytes(addr, chunkData))
                    {
                        AddLog($"❌ ไม่สามารถ Zero-fill ที่ 0x{addr:X6} ได้", "error");
                        return;
                    }

                    addr += chunk;
                }
                AddLog("✅ Zero-fill สำเร็จ", "success");

                // Step 12: Verify digest
                AddLog("🔍 กำลังตรวจสอบ Digest...", "info");
                if (!ch347.ReadBytes(0x40, 16, out byte[]? verifyDig) || 
                    verifyDig == null || verifyDig.Length < 16)
                {
                    AddLog("❌ ไม่สามารถตรวจสอบ Digest ได้", "error");
                    return;
                }

                bool digestMatch = true;
                for (int i = 0; i < 16; i++)
                {
                    if (verifyDig[i] != digest[i])
                    {
                        digestMatch = false;
                        break;
                    }
                }

                if (digestMatch)
                {
                    AddLog($"✅ ตรวจสอบ Digest สำเร็จ: {BitConverter.ToString(verifyDig).Replace("-", " ")}", "success");
                }
                else
                {
                    AddLog("⚠️ Digest ไม่ตรงกัน", "warning");
                }

                // Step 13: Lock chip
                AddLog("🔒 กำลัง Lock Chip...", "info");
                bool lockSuccess = false;
                
                // Check if chip is 0xC2/0x14 (special case)
                if (ch347.ReadFlashID(out byte[]? flashId) && flashId != null && flashId.Length >= 2)
                {
                    if (flashId[0] == 0xC2 && flashId[1] == 0x14)
                    {
                        lockSuccess = ch347.ProtectXC2X14();
                    }
                    else
                    {
                        lockSuccess = ch347.Protect();
                    }
                }
                else
                {
                    lockSuccess = ch347.Protect();
                }

                if (lockSuccess)
                {
                    AddLog("✅ Lock Chip สำเร็จ", "success");
                }
                else
                {
                    AddLog("⚠️ ไม่สามารถ Lock Chip ได้", "warning");
                }

                AddLog("🎉 Reset Tamagotchi Memory เสร็จสิ้น!", "success");
            }
            catch (Exception ex)
            {
                AddLog($"❌ ข้อผิดพลาด: {ex.Message}", "error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                resetButton.Enabled = true;
                readFlashButton.Enabled = true;
                writeFlashButton.Enabled = true;
                eraseButton.Enabled = true;
            }
        }

        private bool ValidateTamagotchiData()
        {
            try
            {
                // Validation - Check bytes at 0x10-0x20
                const uint validationAddr = 0x10;
                const int validationSize = 32;
                byte[] expectedBytes = {
                    0x42, 0x41, 0x4E, 0x44, 0x41, 0x49, 0x4E, 0x54, 0x50, 0x44, 0x5F, 0x30, 0x5F, 0x30, 0x5F, 0x30,
                    0x54, 0x41, 0x4D, 0x41, 0x53, 0x55, 0x4D, 0x41, 0x5F, 0x54, 0x49, 0x4D, 0x30, 0x30, 0x30, 0x30
                };

                if (!ch347.ReadBytes(validationAddr, validationSize, out byte[]? validationData) || 
                    validationData == null || validationData.Length < validationSize)
                {
                    return false;
                }

                for (int i = 0; i < validationSize; i++)
                {
                    if (validationData[i] != expectedBytes[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReadTheme()
        {
            try
            {
                // First validate that this is Tamagotchi data
                if (!ValidateTamagotchiData())
                {
                    chipThemeLabel.Text = "-";
                    AddLog("⚠️ ไม่ใช่ Tama Smart Card Data - ไม่สามารถอ่าน Theme ได้", "warning");
                    return;
                }

                // Read 1 byte from address 0x32
                if (ch347.ReadBytes(0x32, 1, out byte[]? themeData) && themeData != null && themeData.Length >= 1)
                {
                    byte themeValue = themeData[0];
                    string[] themes = {
                        "",
                        "",
                        "1996Friends",
                        "Rainbow",
                        "Sweets",
                        "Nizoo",
                        "Cosmetic",
                        "Gourmet",
                        "Pastel",
                        "Melody",
                        "Sanrio",
                        "Marine",
                        "PuiPuiMolcar",
                        "Magical",
                        "OnePiece",
                        "Anniversary",
                        "Kei-Tama",
                        "En-Tam",
                        "Pixar"
                    };

                    string themeName = "N/A";
                    if (themeValue < themes.Length && !string.IsNullOrEmpty(themes[themeValue]))
                    {
                        themeName = themes[themeValue];
                    }
                    else if (themeValue == 0 || themeValue == 1)
                    {
                        themeName = "Unknown";
                    }

                    chipThemeLabel.Text = $"{themeValue} - {themeName}";
                    AddLog($"🎴 Theme: {themeValue} ({themeName})", "success");
                }
                else
                {
                    chipThemeLabel.Text = "-";
                    AddLog("⚠️ ไม่สามารถอ่าน Theme ได้", "warning");
                }
            }
            catch (Exception ex)
            {
                chipThemeLabel.Text = "-";
                AddLog($"❌ ข้อผิดพลาดในการอ่าน Theme: {ex.Message}", "error");
            }
        }

        private void AddLog(string message, string type = "info")
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => AddLog(message, type)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}\r\n";

            logTextBox.AppendText(logMessage);
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isConnected)
            {
                DisconnectDevice();
            }
            ch347?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
