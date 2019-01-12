. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Connect-PrtgServer_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "can retry request" {

        Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin) -Force -RetryCount 3
        $server = (Settings Server)
        $credential = (New-Credential (Settings WindowsUsername) (Settings WindowsPassword))

        $service = gwmi win32_service -ComputerName $server -Credential $credential -filter "name='PRTGCoreService'"

        LogTestDetail "Stopping service"

        $service.StopService()

        LogTestDetail "Waiting 30 seconds while service stops"

        Sleep 30

        try
        {
            $output = [string]::Join("`n",(&{try { Get-Sensor 3>&1 | %{$_.Message} } catch [exception] { }}))

            $expected = "'Get-Sensor' timed out: Unable to connect to the remote server. Retries remaining: 3`n" +
                "'Get-Sensor' timed out: Unable to connect to the remote server. Retries remaining: 2`n" +
                "'Get-Sensor' timed out: Unable to connect to the remote server. Retries remaining: 1"

            $output | Should Be $expected

            { Get-Sensor | Get-Channel } | Should Throw "Server rejected HTTP connection on port 80"
        }
        finally
        {
            LogTestDetail "Starting service"
            $service.StartService()
            LogTestDetail "Pausing for 20 seconds while service starts"
            Sleep 20
        }
    }
}