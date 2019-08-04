. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Restart-PrtgCore_IT" -Tag @("PowerShell", "IntegrationTest") {

    It "waits for PRTG to restart" {
        Restart-PrtgCore

        $sensor = Get-Sensor -Id (Settings UpSensor)

        $sensor.Id | Should Be (Settings UpSensor)
    }

    It "times out restarting PRTG" {
        { Restart-PrtgCore -Timeout 1 } | Should Throw "Timed out waiting for PRTG Core Service to restart"

        # Wait for the server to come back online
        Restart-PrtgCore -Wait
    }
}