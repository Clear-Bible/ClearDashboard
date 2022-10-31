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
                sb = OperatingSystemInfo(sb);
                sb = UserInfo(sb);
                sb = LocaleInfo(sb);
                sb = IpAddressInfo(sb);
                sb = VideoInfo(sb);
                sb = RamInfo(sb);
                sb = GetNoRamSlots(sb);

                sw.Stop();
                
                elapsed = sw.ElapsedMilliseconds;
            }).ConfigureAwait(false);

            _logger.LogInformation($"GetComputerInfo() took {elapsed} ms");

            if (filePath != "")
            {
                await File.WriteAllTextAsync(filePath, sb.ToString());
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
            string? cpuMan = String.Empty;
            //create an instance of the Management class with the
            //Win32_Processor class
            ManagementClass managementClass = new ManagementClass("Win32_Processor");
            //create a ManagementObjectCollection to loop through
            ManagementObjectCollection objCol = managementClass.GetInstances();
            //start our loop for all processors found
            foreach (var o in objCol)
            {
                var obj = (ManagementObject)o;
                if (cpuMan == String.Empty)
                {
                    // only return manufacturer from first CPU
                    cpuMan = obj.Properties["Manufacturer"].Value.ToString();
                }
            }

            sb.AppendLine($"CPU Brand: {cpuMan}");

            return sb;
        }

        private StringBuilder GetCpuSpeedInGHz(StringBuilder sb)
        {
            // ReSharper disable once InconsistentNaming
            double? GHz = null;
            using (ManagementClass mc = new ManagementClass("Win32_Processor"))
            {
                foreach (var o in mc.GetInstances())
                {
                    var mo = (ManagementObject)o;
                    GHz = 0.001 * (UInt32)mo.Properties["CurrentClockSpeed"].Value;
                    break;
                }
            }

            sb.AppendLine($"CPU Speed: {GHz} GHz");

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
            //sb.AppendLine(string.Format("User Display Name: {0}", System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName));
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

        #endregion // Methods

    }
}
