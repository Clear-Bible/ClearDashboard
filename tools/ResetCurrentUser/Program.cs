using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ResetCurrentUser
{
    internal class Program
    {
        //just use the current TS server context.
        // ReSharper disable once InconsistentNaming
        private static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        //the User Name is the info we want returned by the query.
        // ReSharper disable once InconsistentNaming
        private static int WTS_UserName = 5;


        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformationW(
            IntPtr hServer,
            // ReSharper disable once InconsistentNaming
            int SessionId,
            // ReSharper disable once InconsistentNaming
            int WTSInfoClass,
            out IntPtr ppBuffer,
            out IntPtr pBytesReturned);

        static void Main(string[] args)
        {
            // get the current logged in user by querying the explorer process
            var loggedInUser = GetCurrentLoggedInUser();
            // make an NTAccount object from the user name
#pragma warning disable CA1416
#pragma warning disable CS8604 // Possible null reference argument.
            var ntAccount = new NTAccount(loggedInUser);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CA1416



            // loop through all files in the directory and subdirectories in the Collaboration folder
            var collabDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Collaboration");

            if (Directory.Exists(collabDir))
            {
                foreach (var file in Directory.GetFiles(collabDir, "*.*", SearchOption.AllDirectories))
                {
                    var fileInfo = new FileInfo(file);
#pragma warning disable CA1416
                    var fileOwner = fileInfo.GetAccessControl().GetOwner(typeof(NTAccount));
                    if (fileOwner!.ToString() != loggedInUser)
                    {
                        // if it is a different user, change the ownership to the current user
                        SetFileOwnershipToUser(fileInfo, ntAccount);
                    }
#pragma warning restore CA1416
                }

                foreach (var dir in Directory.GetDirectories(collabDir, "*.*", SearchOption.AllDirectories))
                {
                    var dirInfo = new DirectoryInfo(dir);
#pragma warning disable CA1416
                    var dirOwner = dirInfo.GetAccessControl().GetOwner(typeof(NTAccount));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (dirOwner.ToString() != loggedInUser)
                    {
                        // if it is a different user, change the ownership to the current user
                        SetDirectoryOwnershipToUser(dirInfo, ntAccount);
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CA1416
                }
            }
        }


        /// <summary>
        /// The explorer process is always running under the currently logged in user.
        /// </summary>
        /// <returns></returns>
        private static string? GetCurrentLoggedInUser()
        {
            Process[] procs = Process.GetProcesses();
            foreach (Process proc in procs)
            {
                if (proc.ProcessName == "explorer")
                {
                    // ReSharper disable once InconsistentNaming
                    IntPtr AnswerBytes;
                    // ReSharper disable once InconsistentNaming
                    // ReSharper disable once NotAccessedOutParameterVariable
                    IntPtr AnswerCount;
                    if (WTSQuerySessionInformationW(WTS_CURRENT_SERVER_HANDLE,
                            proc.SessionId,
                            WTS_UserName,
                            out AnswerBytes,
                            out AnswerCount))
                    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        string? userName = Marshal.PtrToStringUni(AnswerBytes);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                        return userName;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Set the ownership of the file to the current logged in user.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="ntAccount"></param>
        private static void SetFileOwnershipToUser(FileInfo fileInfo, NTAccount ntAccount)
        {
#pragma warning disable CA1416
            var acl = fileInfo.GetAccessControl(System.Security.AccessControl.AccessControlSections.All);

            acl.SetOwner(ntAccount);
            acl.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                ntAccount, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));

            fileInfo.SetAccessControl(acl);
#pragma warning restore CA1416
        }

        /// <summary>
        /// Set the ownership of the directory to the current logged in user.
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="ntAccount"></param>
        private static void SetDirectoryOwnershipToUser(DirectoryInfo dirInfo, NTAccount ntAccount)
        {
#pragma warning disable CA1416
            var acl = dirInfo.GetAccessControl(System.Security.AccessControl.AccessControlSections.All);

            acl.SetOwner(ntAccount);
            acl.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                ntAccount, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));

            dirInfo.SetAccessControl(acl);
#pragma warning restore CA1416
        }
    }
}