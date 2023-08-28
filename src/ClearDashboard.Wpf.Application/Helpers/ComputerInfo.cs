using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ObjectQuery = System.Management.ObjectQuery;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class ComputerInfo
    {
        #region Member Variables      
        
        private readonly ILogger<ComputerInfo> _logger;

        #endregion //Member Variables


        #region Public Properties
        #endregion //Public Properties


        #region Constructor

        public ComputerInfo()
        {
            _logger = IoC.Get<ILogger<ComputerInfo>>();
        }
        
        // ReSharper disable once UnusedMember.Global
        public ComputerInfo(ILogger<ComputerInfo> logger)
        {
            _logger = logger;
        }

        #endregion //Constructor


        #region Methods
        
        public async Task<StringBuilder> GetComputerInfo(string filePath = "")
        {
            StringBuilder sb = new StringBuilder();
            long elapsed = 0;

            await Task.Run(() =>
            {
                Stopwatch sw = new();
                sw.Start();
                sb.AppendLine($"Date Time: {DateTime.Now}");
                
                sb = MachineInfo(sb);
                sb = GetCpuManufacturer(sb);
                sb = GetCpuSpeedInGHz(sb);
                sb = GetMonitorInfo(sb);
                sb = OperatingSystemInfo(sb);
                sb = UserInfo(sb);
                sb = LocaleInfo(sb);
                sb = IpAddressInfo(sb);
                sb = VideoInfo(sb);
                sb = RamInfo(sb);
                sb = GetNoRamSlots(sb);

                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sb.AppendLine($"User's Document Folder: {path}");
                DirectoryInfo di = new DirectoryInfo(path);
                var drive = di.Root.ToString().Substring(0, 2);

                sb = GetBitLockerStatus(sb, drive);
                sb = GetDriveInfo(sb);
                
                sw.Stop();
                
                elapsed = sw.ElapsedMilliseconds;
            });

            _logger.LogInformation($"GetComputerInfo() took {elapsed} ms");

            if (filePath != "")
            {
                await File.WriteAllTextAsync(filePath, sb.ToString());
            }
            
            return sb;
        }

        private StringBuilder GetMonitorInfo(StringBuilder sb)
        {
            var monitors = Monitor.AllMonitors;
            if (monitors != null)
            {
                int i = 1;
                foreach (var monitor in monitors)
                {
                    sb.AppendLine($"Monitor #{i} {monitor.Name}");
                    sb.AppendLine($"  Primary: {monitor.IsPrimary}");
                    sb.AppendLine($"  Size: {monitor.Bounds}");

                    i++;
                }
            }
            return sb;
        }

        private StringBuilder MachineInfo(StringBuilder sb)
        {
            sb.AppendLine($"Computer Name: {Environment.MachineName}");
            sb.AppendLine($"CPU Processor Count: {Environment.ProcessorCount}");
            return sb;
        }

        private StringBuilder GetCpuManufacturer(StringBuilder sb)
        {
            sb.AppendLine(string.Format("CPU Type: {0}", System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")));

            return sb;
        }

        private StringBuilder GetCpuSpeedInGHz(StringBuilder sb)
        {
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT *, Name FROM Win32_Processor").Get())
            {
                double maxSpeed = Convert.ToDouble(obj["MaxClockSpeed"]) / 1000;
                sb.AppendLine(string.Format("{0} Running at {1:0.00} Ghz", obj["Name"], maxSpeed));
            }

            return sb;
        }

        private StringBuilder OperatingSystemInfo(StringBuilder sb)
        {
            sb.AppendLine($"OS Version: {Environment.OSVersion}");
            sb.AppendLine($"Is 64-Bit: {Environment.Is64BitOperatingSystem}");


            switch (Environment.OSVersion.ToString())
            {
                case "10.0.22621":
                    sb.AppendLine("Version Details: Windows 11 (22H2)");
                    break;
                case "10.0.22000":
                    sb.AppendLine("Version Details: Windows 11 (21H2)");
                    break;

                case "10.0.19044":
                    sb.AppendLine("Version Details: Windows 10 (21H2)");
                    break;
                case "10.0.19043":
                    sb.AppendLine("Version Details: Windows 10 (21H1)");
                    break;
                case "10.0.19042":
                    sb.AppendLine("Version Details: Windows 10 (20H2)");
                    break;
                case "10.0.19041":
                    sb.AppendLine("Version Details: Windows 10 (2004)");
                    break;
                case "10.0.18363":
                    sb.AppendLine("Version Details: Windows 10 (1909)");
                    break;
                case "10.0.18362":
                    sb.AppendLine("Version Details: Windows 10 (1903)");
                    break;
                case "10.0.17763":
                    sb.AppendLine("Version Details: Windows 10 (1809)");
                    break;
                case "10.0.17134":
                    sb.AppendLine("Version Details: Windows 10 (1803)");
                    break;
                case "10.0.16299":
                    sb.AppendLine("Version Details: Windows 10 (1709)");
                    break;
                case "10.0.15063":
                    sb.AppendLine("Version Details: Windows 10 (1703)");
                    break;
                case "10.0.14393":
                    sb.AppendLine("Version Details: Windows 10 (1607)");
                    break;
                case "10.0.10586":
                    sb.AppendLine("Version Details: Windows 10 (1511)");
                    break;
                case "10.0.10240":
                    sb.AppendLine("Version Details: Windows 10");
                    break;

                case "6.3.9600":
                    sb.AppendLine("Version Details: Windows 8.1 (Update 1)");
                    break;
                case "6.3.9200":
                    sb.AppendLine("Version Details: Windows 8.1");
                    break;
                case "6.2.9200":
                    sb.AppendLine("Version Details: Windows 8");
                    break;

                case "6.1.7601":
                    sb.AppendLine("Version Details: Windows 7 SP1");
                    break;
                case "6.1.7600":
                    sb.AppendLine("Version Details: Windows 7");
                    break;
            }

            return sb;
        }

        private StringBuilder UserInfo(StringBuilder sb)
        {
            sb.AppendLine($"User Name: {Environment.UserName}");
            sb.AppendLine($"Domain Name: {Environment.UserDomainName}");
            return sb;
        }

        private StringBuilder LocaleInfo(StringBuilder sb)
        {
            sb.AppendLine($"Locale DateTime Format: {CultureInfo.CurrentCulture}");
            sb.AppendLine($"Display Language: {CultureInfo.CurrentUICulture}");
            sb.AppendLine($"TimeZone: {TimeZoneInfo.Local.Id}");
            return sb;
        }

        private StringBuilder IpAddressInfo(StringBuilder sb)
        {
            var hostName = System.Net.Dns.GetHostName();

            sb.AppendLine($"DNS Hostname: {hostName}");
            System.Net.IPAddress[] ipList = System.Net.Dns.GetHostEntry(hostName).AddressList;
            foreach (var ip in ipList)
            {
                sb.AppendLine($"IP Address: {ip}");
            }

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces()
                .Where(niStats => niStats.OperationalStatus == OperationalStatus.Up
                && niStats.NetworkInterfaceType != NetworkInterfaceType.Loopback))
            {
                foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                {
                    sb.AppendLine($"IP Address: ({ni.Name}), {ip.Address.ToString()}");
                }
            }
            return sb;
        }

        private StringBuilder VideoInfo(StringBuilder sb)
        {
            var wmiMonitor = new ManagementObject("Win32_VideoController.DeviceID=\"VideoController1\"");
            var width = wmiMonitor["CurrentHorizontalResolution"];
            var height = wmiMonitor["CurrentVerticalResolution"];
            sb.AppendLine($"Video Resolution: {width} x {height}");
            return sb;
        }

        private StringBuilder RamInfo(StringBuilder sb)
        {
            ObjectQuery wmiObj = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var findObj = new ManagementObjectSearcher(wmiObj);
            ManagementObjectCollection ramInfo = findObj.Get();

            foreach (var element in ramInfo)
            {
                sb.AppendLine(
                    $"Total Visible Memory: {(Convert.ToDouble(element["TotalVisibleMemorySize"]) / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)} GB");
                sb.AppendLine(
                    $"Free Physical Memory: {(Convert.ToDouble(element["FreePhysicalMemory"]) / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)} GB");
                sb.AppendLine(
                    $"Total Virtual Memory: {(Convert.ToDouble(element["TotalVirtualMemorySize"]) / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)} GB");
                sb.AppendLine(
                    $"Free Virtual Memory: {(Convert.ToDouble(element["FreeVirtualMemory"]) / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)} GB");
            }
            return sb;
        }

        private StringBuilder GetNoRamSlots(StringBuilder sb)
        {
            int memSlots = 0;
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery2 = new ObjectQuery("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
            ManagementObjectSearcher oSearcher2 = new ManagementObjectSearcher(oMs, oQuery2);
            ManagementObjectCollection oCollection2 = oSearcher2.Get();
            foreach (var o in oCollection2)
            {
                var obj = (ManagementObject)o;
                memSlots = Convert.ToInt32(obj["MemoryDevices"]);
            }
            sb.AppendLine($"Memory Slots: {memSlots}");
            return sb;
        }

        public StringBuilder GetBitLockerStatus(StringBuilder sb, string drive)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = $"-command (New-Object -ComObject Shell.Application).NameSpace('{drive}').Self.ExtendedProperty('System.Volume.BitLockerProtection')";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd().Substring(0, 1); //needed as output would otherwise be 1\r\n (if encrypted)
            process.WaitForExit();


            string status = $"User's Document Drive {drive} is ";
            switch (output)
            {
                case "1":
                    status += "BitLocker on [Fully Encrypted]";
                    break;
                case "2":
                    status += "BitLocker off [Fully Decrypted]";
                    break;
                case "3":
                    status += "BitLocker Encrypting [Encryption In Progress]";
                    break;
                case "4":
                    status += "BitLocker Decrypting [Decrypting In Progress]";
                    break;
                case "5":
                    status += "BitLocker suspended [Fully Encrypted]";
                    break;
                default:
                    status += $"UNKNOWN CODE {output}";
                    break;
            }
            sb.AppendLine(status);

            return sb;
        }


        private StringBuilder GetDriveInfo(StringBuilder sb)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                sb.AppendLine($"Drive {d.Name}");
                sb.AppendLine($"  Drive type: {d.DriveType}");
                if (d.IsReady)
                {
                    sb.AppendLine($"  Volume label: {d.VolumeLabel}");
                    sb.AppendLine($"  File system: {d.DriveFormat}");
                    sb.AppendLine($"  Available space to current user:{d.AvailableFreeSpace, 15} bytes");
                    sb.AppendLine($"  Total available space:          {d.TotalFreeSpace, 15} bytes");
                    sb.AppendLine($"  Total size of drive:            {d.TotalSize, 15} bytes ");
                }
            }
            return sb;
        }
        
        #endregion // Methods

    }
}
