using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ResetCurrentUser
{
    internal class Program
    {
        //just use the current TS server context.
        private static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        //the User Name is the info we want returned by the query.
        private static int WTS_UserName = 5;


        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformationW(
            IntPtr hServer,
            int SessionId,
            int WTSInfoClass,
            out IntPtr ppBuffer,
            out IntPtr pBytesReturned);

        static void Main(string[] args)
        {
            // get the current logged in user by querying the explorer process
            var loggedInUser = GetCurrentLoggedInUser();
            // make an NTAccount object from the user name
            var ntAccount = new NTAccount(loggedInUser);



            // loop through all files in the directory and subdirectories in the Collaboration folder
            var collabDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Collaboration");
            foreach (var file in Directory.GetFiles(collabDir, "*.*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(file);
                var fileOwner = fileInfo.GetAccessControl().GetOwner(typeof(NTAccount));
                if (fileOwner.ToString() != loggedInUser)
                {
                    // if it is a different user, change the ownership to the current user
                    SetFileOwnershipToUser(fileInfo, ntAccount);
                }
            }

            foreach (var dir in Directory.GetDirectories(collabDir, "*.*", SearchOption.AllDirectories))
            {
                var dirInfo = new DirectoryInfo(dir);
                var dirOwner = dirInfo.GetAccessControl().GetOwner(typeof(NTAccount));
                if (dirOwner.ToString() != loggedInUser)
                {
                    // if it is a different user, change the ownership to the current user
                    SetDirectoryOwnershipToUser(dirInfo, ntAccount);
                }
            }
        }


        /// <summary>
        /// The explorer process is always running under the currently logged in user.
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentLoggedInUser()
        {
            Process[] procs = Process.GetProcesses();
            foreach (Process proc in procs)
            {
                if (proc.ProcessName == "explorer")
                {
                    IntPtr AnswerBytes;
                    IntPtr AnswerCount;
                    if (WTSQuerySessionInformationW(WTS_CURRENT_SERVER_HANDLE,
                            proc.SessionId,
                            WTS_UserName,
                            out AnswerBytes,
                            out AnswerCount))
                    {
                        string userName = Marshal.PtrToStringUni(AnswerBytes);
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
            var acl = fileInfo.GetAccessControl(System.Security.AccessControl.AccessControlSections.All);

            acl.SetOwner(ntAccount);
            acl.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                ntAccount, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));

            fileInfo.SetAccessControl(acl);
        }

        /// <summary>
        /// Set the ownership of the directory to the current logged in user.
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="ntAccount"></param>
        private static void SetDirectoryOwnershipToUser(DirectoryInfo dirInfo, NTAccount ntAccount)
        {
            var acl = dirInfo.GetAccessControl(System.Security.AccessControl.AccessControlSections.All);

            acl.SetOwner(ntAccount);
            acl.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                ntAccount, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));

            dirInfo.SetAccessControl(acl);
        }
    }
}