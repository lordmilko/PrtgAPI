. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Restart-PrtgCore" -Tag @("PowerShell", "UnitTest") {
    It "restarts the core server" {

        $status = WithResponse "MultiTypeResponse" {
            Get-PrtgStatus
        }

        $startDate = $status.DateTime.ToString("yyyy-MM-dd-HH-mm-ss")

        SetAddressValidatorResponse @(
            [Request]::Status()
            [Request]::Get("api/restartserver.htm?")
            [Request]::Logs("count=500&start=1&filter_status=1&filter_dstart=$startDate", [UrlFlag]::Columns)
        )

        Restart-PrtgCore -Force -Wait -Timeout 3600
    }

    It "completes all waiting stages" {
        SetResponseAndClient "RestartPrtgCoreResponse"

        Restart-PrtgCore -Force
    }
}