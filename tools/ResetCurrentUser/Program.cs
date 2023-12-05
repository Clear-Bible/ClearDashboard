using System.Diagnostics;
using System.Net.Mime;
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

            Console.WriteLine($"Logged In User: {loggedInUser}");

                        // make an NTAccount object from the user name
#pragma warning disable CA1416
#pragma warning disable CS8604 // Possible null reference argument.
            var ntAccount = new NTAccount(loggedInUser);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CA1416

            Console.WriteLine($"NT Account info for: {ntAccount.Value}");

            // loop through all files in the directory and subdirectories in the Collaboration folder
            var collabDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Collaboration");

            Console.WriteLine($"Collab Dir: {collabDir}");

            if (Directory.Exists(collabDir))
            {
                Console.WriteLine($"Resetting Collab Dir Files");
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
                Console.WriteLine($"Resetting Collab Dir Files - Complete");


                foreach (var dir in Directory.GetDirectories(collabDir, "*.*", SearchOption.AllDirectories))
                {
                    var dirInfo = new DirectoryInfo(dir);
#pragma warning disable CA1416
                    var dirOwner = dirInfo.GetAccessControl().GetOwner(typeof(NTAccount));
#pragma warning disable CS8602 // Dereference of a possibly null reference.

                    Console.WriteLine($"Dir: {dirInfo.FullName}\n\tOwner: {dirOwner.Value}");

                    if (dirOwner.ToString() != loggedInUser)
                    {
                        // if it is a different user, change the ownership to the current user
                        SetDirectoryOwnershipToUser(dirInfo, ntAccount);

                        Console.WriteLine($"Reset: {dirInfo.FullName}\n\tTo: {ntAccount.Value}");
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CA1416
                }
            }


            // exit the app after 5 seconds unless key press
            var done = new ManualResetEvent(false);
            
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                done.Set();
            };

            WaitForExitOrBreak();

            if (!done.WaitOne(5000))
                Console.WriteLine("Time out, exiting program.");

            Console.WriteLine("Exit program.");

        }


        static void WaitForExitOrBreak()
        {
            for (int i = 5 - 1; i >= 0; i--)
            {
                Console.Write("Working {0}... ", i);
                Thread.Sleep(1000);
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