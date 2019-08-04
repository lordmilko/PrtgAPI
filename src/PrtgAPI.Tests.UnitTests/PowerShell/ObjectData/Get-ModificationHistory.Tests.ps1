. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-ModificationHistory" -Tag @("PowerShell", "UnitTest") {
    It "retrieves history" {
        $sensor = Run Sensor {
            Get-Sensor
        }

        $history = $sensor | Get-ModificationHistory

        $history.UserName | Should Be "PRTG System Administrator"
        $history.Message | Should Be "Created. 17.2.31.2018"
    }
}