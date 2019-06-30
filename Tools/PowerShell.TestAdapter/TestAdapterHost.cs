using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PowerShell.TestAdapter
{
    class TestAdapterHost : PSHost, IHostSupportsInteractiveSession
    {
        private Guid instanceGuid;
        private HostUi hostUi;

        public TestAdapterHost()
        {
            instanceGuid = Guid.NewGuid();
            hostUi = new HostUi();
        }

        public override void SetShouldExit(int exitCode)
        {
        }

        public override void EnterNestedPrompt()
        {
        }

        public override void ExitNestedPrompt()
        {
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override string Name => "PowerShell Tools for Visual Studio Test Adapter";

        public override Version Version => new Version(1, 0);

        public override Guid InstanceId => instanceGuid;

        public override PSHostUserInterface UI => hostUi;

        public HostUi HostUi => hostUi;

        public override CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.CurrentUICulture;

        public void PushRunspace(Runspace runspace)
        {
        }

        public void PopRunspace()
        {
        }

        public bool IsRunspacePushed { get; private set; }

        public Runspace Runspace { get; private set; }
    }
}
