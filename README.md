# Tama Smart App

A C# WinForms application for reading/writing SPI Flash Memory via CH347 USB-to-SPI Bridge, specifically designed for managing Tamagotchi SMA Card (Smart Card).

## Features

### 🔌 Device Management
- **Auto-detect CH347 devices** - Automatically detects connected CH347 devices
- **COM Port Display** - Displays COM port instead of device number (e.g., COM3, COM4)
- **Device Connection** - Connect/disconnect devices

### 📊 Chip Information
- **Chip ID Detection** - Automatically reads Chip ID upon connection
- **Chip Database** - Database of 1000+ chips from `chiplist.xml`
- **Auto Chip Identification** - Automatically identifies chip from Chip ID
- **Chip Selection Dialog** - Select chip when multiple chips share the same ID
- **Display Information** - Shows IC Name, Size, Theme

### 🎴 Tamagotchi Smart Card Support
- **Theme Detection** - Reads and displays card theme (e.g., Sanrio, Rainbow, Sweets)
- **Data Validation** - Validates Tama Smart Card data before reading theme
- **Theme Display** - Shows theme number and name (e.g., "10 - Sanrio")

### 📖 Read Chip
- **Full Chip Read** - Reads entire chip data automatically based on chip size
- **Progress Bar** - Real-time progress display
- **Save to File** - Saves as .bin file
- **Default Path** - Uses `{AppPath}\Backups\` as default path

### ✍️ Write Chip
- **4-Step Process** - Unprotect → Erase → Write → Verify
- **Smart Write** - Skips 0xFF pages (no need to write)
- **Progress Bar** - Shows progress for each step
- **Auto Verification** - Automatically verifies data after writing
- **Default Path** - Uses `{AppPath}\Backups\` as default path

### 🗑️ Erase Chip
- **Full Chip Erase** - Erases all data in chip
- **Auto Unprotect** - Unprotects before erasing
- **Progress Bar** - Shows progress (Marquee style)
- **Warning Log** - Warning message in log

### 🔄 Reset Tama
- **Tamagotchi Memory Reset** - Resets memory to factory state
- **Data Validation** - Validates Tama Smart Card data
- **Theme Preservation** - Reads and displays theme before reset
- **MD5 Digest** - Calculates and saves MD5 hash
- **Auto Lock** - Locks chip after reset completes
- **Special Chip Support** - Supports special chips (0xC2/0x14)

### 🔒 Protection Management
- **Auto Unprotect** - Unprotects before write/erase
- **Auto Lock** - Locks chip after reset completes
- **Special Chip Handling** - Supports special chips with different protection modes

## System Requirements

- **OS**: Windows (32-bit)
- **.NET Framework**: 4.8
- **Hardware**: CH347 USB-to-SPI Bridge
- **Target**: SPI Flash Memory Chips (25-series)

## Installation

1. Download `TamaSmartApp.exe` from `bin\Release\net48\win-x86\publish\`
2. Run `TamaSmartApp.exe` (no installation required)
3. Connect CH347 device to your computer

## Usage

### 1. Connect Device
1. Launch the application
2. Select COM port from dropdown (or device number if COM port not found)
3. Click **Connect** button
4. System will automatically read Chip ID

### 2. Read Chip
1. Click **Read Chip** button
2. Wait for read to complete (Progress Bar will be displayed)
3. Select save location (default: `Backups\`)
4. File will be saved as `.bin`

### 3. Write Chip
1. Click **Write Chip** button
2. Select `.bin` file to write (default: `Backups\`)
3. System will perform:
   - Unprotect
   - Erase all data
   - Write data
   - Verify data
4. Wait for completion (Progress Bar will be displayed)

### 4. Erase All Data
1. Click **Erase All Data** button
2. System will:
   - Unprotect
   - Erase all data in chip
3. Wait for completion (Progress Bar will be displayed)

### 5. Reset Tama
1. Click **Reset Tama** button
2. System will:
   - Validate Tama Smart Card data
   - Read theme
   - Reset memory to factory state
   - Lock chip
3. Wait for completion

## Chip Information

### Chip Database
- Chip database from `chiplist.xml` (embedded in exe)
- Supports 1000+ chips
- Supports multiple manufacturers: Macronix, Winbond, GigaDevice, ISSI, and more

### Chip Information Display
- **Chip ID**: Chip ID (e.g., C2 23 15)
- **IC**: Chip name (e.g., MX25V1635F)
- **Size**: Chip size (bytes)
- **Theme**: Tama Smart Card theme (if Tama data)

## Project Structure

```
TamaSmartApp/
├── MainForm.cs              # Main UI logic
├── MainForm.Designer.cs      # UI design
├── CH347Wrapper.cs          # CH347 API wrapper
├── CH347DLL.cs              # CH347 DLL interop
├── ChipInfo.cs              # Chip database management
├── FindChipDialog.cs        # Chip selection dialog
├── Program.cs               # Entry point
├── lib/
│   ├── CH347DLL.DLL        # CH347 driver (embedded)
│   └── chiplist.xml        # Chip database (embedded)
└── README.md               # This file
```

## Warnings

⚠️ **Important Warnings:**
- Writing/erasing will delete all existing data - **cannot be recovered**
- **Always backup data before writing/erasing**
- Verify that you have the correct chip before proceeding
- Use with Tamagotchi SMA Card only (for Reset Tama)

## Building

### Requirements
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- Windows SDK

### Build Command
```bash
dotnet build -c Release
```

### Publish Command
```bash
.\publish.bat
```

or

```bash
dotnet publish -c Release -r win-x86 --self-contained false
```

## Output Files

After Build/Publish, you will get:
- `bin\Release\net48\win-x86\publish\TamaSmartApp.exe` - Main executable
- `bin\Release\net48\win-x86\publish\TamaSmartApp.exe.config` - Configuration file

**Note**: `CH347DLL.DLL` and `chiplist.xml` are embedded in the exe, no separate files needed.

## Log and Progress

- **Log Section**: Displays all operations with timestamps
- **Progress Bar**: Shows read/write/erase progress
- **Progress Label**: Shows detailed progress (bytes, percentage)

## Troubleshooting

### CH347 Device Not Found
- Verify CH347 driver is installed
- Check USB cable connection
- Click **Refresh** button to search for devices again

### Cannot Read/Write
- Check if chip is protected (try Unprotect first)
- Verify Chip ID is correct
- Check SPI connection

### Theme Not Displayed
- Verify it's actual Tama Smart Card data
- Check if validation passed (see Log)

## Credits

- CH347 Driver: WCH (Nanjing Qinheng Microelectronics)
- Chip Database: Based on UsbAsp-flash chiplist.xml