using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.IntegrationTests
{
    public static class Logger
    {
        private static bool newFile = false;

        public static void Log(string message, bool error = false, string engine = "C#")
        {
            var path = Environment.GetEnvironmentVariable("temp") + "\\PrtgAPI.IntegrationTests.log";

            if (newFile == false)
            {
                newFile = true;
                File.AppendAllText(path, "\n");
            }

            var pid = Process.GetCurrentProcess().Id.ToString();

            if (pid.Length == 4)
                pid = " " + pid;

            var tid = Thread.CurrentThread.ManagedThreadId.ToString();

            if (tid.Length == 1)
                tid = " " + tid;

            var errText = error ? "!!!" : "   ";

            File.AppendAllText(path, $"{DateTime.Now} [{pid}:{tid}] {engine} {errText} : {message}\r\n");
        }

        public static void LogTest(string message, bool error = false, string engine = "C#")
        {
            Log("    " + message, error, engine);
        }

        public static void LogTestDetail(string message, bool error = false, string engine = "C#")
        {
            Log("        " + message, error, engine);
        }
    }
}
