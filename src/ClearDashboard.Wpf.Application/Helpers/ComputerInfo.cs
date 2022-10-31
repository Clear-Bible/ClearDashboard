using ClearDashboard.Wpf.Application.ViewModels.Main;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ObjectQuery = System.Management.ObjectQuery;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class ComputerInfo
    {
        private readonly ILogger<ComputerInfo> _logger;

        #region Member Variables      

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Constructor

        public ComputerInfo()
        {
            _logger = IoC.Get<ILogger<ComputerInfo>>();
        }
        
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
                sb.AppendLine(string.Format("Date Time: {0}", DateTime.Now));
                
                sb = MachineInfo(sb);
                sb = GetCPUManufacturer(sb);
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


            if (_logger is not null)
            {
                _logger.LogInformation($"GetComputerInfo() took {elapsed} ms");
            }

            if (filePath != "")
            {
                File.WriteAllText(filePath, sb.ToString());
            }
            
            return sb;
        }
        
        private StringBuilder MachineInfo(StringBuilder sb)
        {
            sb.AppendLine(string.Format("Computer Name: {0}", Environment.MachineName));
            sb.AppendLine(string.Format("CPU Processor Count: {0}", Environment.ProcessorCount));
            return sb;
        }

        private StringBuilder GetCPUManufacturer(StringBuilder sb)
        {
            string cpuMan = String.Empty;
            //create an instance of the Managemnet class with the
            //Win32_Processor class
            ManagementClass mgmt = new ManagementClass("Win32_Processor");
            //create a ManagementObjectCollection to loop through
            ManagementObjectCollection objCol = mgmt.GetInstances();
            //start our loop for all processors found
            foreach (ManagementObject obj in objCol)
            {
                if (cpuMan == String.Empty)
                {
                    // only return manufacturer from first CPU
                    cpuMan = obj.Properties["Manufacturer"].Value.ToString();
                }
            }

            sb.AppendLine(string.Format("CPU Brand: {0}", cpuMan));

            return sb;
        }

        private StringBuilder GetCpuSpeedInGHz(StringBuilder sb)
        {
            double? GHz = null;
            using (ManagementClass mc = new ManagementClass("Win32_Processor"))
            {
                foreach (ManagementObject mo in mc.GetInstances())
                {
                    GHz = 0.001 * (UInt32)mo.Properties["CurrentClockSpeed"].Value;
                    break;
                }
            }

            sb.AppendLine(string.Format("CPU Speed: {0} GHz", GHz));

            return sb;
        }

        private StringBuilder OperatingSystemInfo(StringBuilder sb)
        {
            sb.AppendLine(string.Format("OS Version: {0}", Environment.OSVersion));
            sb.AppendLine(string.Format("Is 64-Bit: {0}", Environment.Is64BitOperatingSystem));


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
            sb.AppendLine(string.Format("User Name: {0}", Environment.UserName));
            sb.AppendLine(string.Format("Domain Name: {0}", Environment.UserDomainName));
            //sb.AppendLine(string.Format("User Display Name: {0}", System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName));
            return sb;
        }

        private StringBuilder LocaleInfo(StringBuilder sb)
        {
            sb.AppendLine(string.Format("Locale DateTime Format: {0}", System.Globalization.CultureInfo.CurrentCulture));
            sb.AppendLine(string.Format("Display Language: {0}", System.Globalization.CultureInfo.CurrentUICulture));
            sb.AppendLine(string.Format("TimeZone: {0}", TimeZoneInfo.Local.Id));
            return sb;
        }

        private StringBuilder IpAddressInfo(StringBuilder sb)
        {
            var hostName = System.Net.Dns.GetHostName();

            sb.AppendLine(string.Format("DNS Hostname: {0}", hostName));
            System.Net.IPAddress[] ipList = System.Net.Dns.GetHostEntry(hostName).AddressList;
            foreach (var ip in ipList)
            {
                sb.AppendLine(string.Format("IP Address: {0}", ip));
            }

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces()
                .Where(niStats => niStats.OperationalStatus == OperationalStatus.Up
                && niStats.NetworkInterfaceType != NetworkInterfaceType.Loopback))
            {
                foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                {
                    sb.AppendLine(string.Format("IP Address: ({0}), {1}", ni.Name, ip.Address.ToString()));
                }
            }
            return sb;
        }

        private StringBuilder VideoInfo(StringBuilder sb)
        {
            var wmiMonitor = new ManagementObject("Win32_VideoController.DeviceID=\"VideoController1\"");
            var width = wmiMonitor["CurrentHorizontalResolution"];
            var height = wmiMonitor["CurrentVerticalResolution"];
            sb.AppendLine(string.Format("Video Resolutin: {0} x {1}", width, height));
            return sb;
        }

        private StringBuilder RamInfo(StringBuilder sb)
        {
            ObjectQuery wmi_obj = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var findObj = new ManagementObjectSearcher(wmi_obj);
            ManagementObjectCollection ramInfo = findObj.Get();

            foreach (var element in ramInfo)
            {
                sb.AppendLine(string.Format("Total Visible Memory: {0} GB", (Convert.ToDouble(element["TotalVisibleMemorySize"]) / (1024 * 1024)).ToString()));
                sb.AppendLine(string.Format("Free Physical Memory: {0} GB", (Convert.ToDouble(element["FreePhysicalMemory"]) / (1024 * 1024)).ToString()));
                sb.AppendLine(string.Format("Total Virtual Memory: {0} GB", (Convert.ToDouble(element["TotalVirtualMemorySize"]) / (1024 * 1024)).ToString()));
                sb.AppendLine(string.Format("Free Virtual Memory: {0} GB", (Convert.ToDouble(element["FreeVirtualMemory"]) / (1024 * 1024)).ToString()));
            }
            return sb;
        }

        private StringBuilder GetNoRamSlots(StringBuilder sb)
        {
            int MemSlots = 0;
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery2 = new ObjectQuery("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
            ManagementObjectSearcher oSearcher2 = new ManagementObjectSearcher(oMs, oQuery2);
            ManagementObjectCollection oCollection2 = oSearcher2.Get();
            foreach (ManagementObject obj in oCollection2)
            {
                MemSlots = Convert.ToInt32(obj["MemoryDevices"]);

            }
            sb.AppendLine(string.Format("Memory Slots: {0}", MemSlots));
            return sb;
        }

        #endregion // Methods

    }
}
