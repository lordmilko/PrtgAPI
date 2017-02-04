using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.IntegrationTests
{
    //http://stackoverflow.com/questions/6866104/c-sharp-service-status-on-remote-machine
    class Impersonator : IDisposable
    {
        [DllImport("advapi32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LogonUser(
            [MarshalAs(UnmanagedType.LPStr)] string pszUserName,
            [MarshalAs(UnmanagedType.LPStr)] string pszDomain,
            [MarshalAs(UnmanagedType.LPStr)] string pszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        private static IntPtr tokenHandle = new IntPtr(0);
        private static WindowsImpersonationContext impersonatedUser;

        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Impersonator(string domain, string username, string password)
        {
            tokenHandle = IntPtr.Zero;

            var result = LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);

            if (!result)
            {
                var ret = Marshal.GetLastWin32Error();
                throw new Win32Exception(ret);
            }

            var newIdentity = new WindowsIdentity(tokenHandle);

            impersonatedUser = newIdentity.Impersonate();
        }

        public void RevertToSelf()
        {
            impersonatedUser.Undo();

            if (tokenHandle != IntPtr.Zero)
            {
                CloseHandle(tokenHandle);
            }
        }

        public void Dispose()
        {
            RevertToSelf();
        }

        public static void ExecuteAction(Action action)
        {
            using (var impersonator = new Impersonator(Settings.Server, Settings.WindowsUsername, Settings.WindowsPassword))
            {
                action();
            }
        }
    }
}
