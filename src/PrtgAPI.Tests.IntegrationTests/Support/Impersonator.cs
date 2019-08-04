using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace PrtgAPI.Tests.IntegrationTests
{
    static class Impersonator
    {
        [DllImport("advapi32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LogonUser(
            [MarshalAs(UnmanagedType.LPStr)] string pszUserName,
            [MarshalAs(UnmanagedType.LPStr)] string pszDomain,
            [MarshalAs(UnmanagedType.LPStr)] string pszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken
        );

        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static SafeAccessTokenHandle GetHandle(string domain, string username, string password)
        {
            var tokenHandle = IntPtr.Zero;

            var result = LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);

            if (!result)
            {
                var ret = Marshal.GetLastWin32Error();
                throw new Win32Exception(ret);
            }

            var safeToken = new SafeAccessTokenHandle(tokenHandle);

            return safeToken;
        }

        public static void ExecuteAction(Action action)
        {
            using (var token = GetHandle(Settings.Server, Settings.WindowsUserName, Settings.WindowsPassword))
            {
                WindowsIdentity.RunImpersonated(token, action);
            }
        }

        public static T ExecuteAction<T>(Func<T> action)
        {
            using (var token = GetHandle(Settings.Server, Settings.WindowsUserName, Settings.WindowsPassword))
            {
                return WindowsIdentity.RunImpersonated(token, action);
            }
        }
    }
}
