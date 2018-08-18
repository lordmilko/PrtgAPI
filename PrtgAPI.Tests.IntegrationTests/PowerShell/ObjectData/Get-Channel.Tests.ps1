. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-Channel_IT" {
    It "retrieves all channels" {
        (Get-Sensor -Id (Settings ChannelSensor) | Get-Channel).Count | Should Be (Settings ChannelsInTestSensor)
    }
}