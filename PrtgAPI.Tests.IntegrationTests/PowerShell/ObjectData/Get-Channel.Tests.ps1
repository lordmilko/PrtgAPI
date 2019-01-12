. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-Channel_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "retrieves all channels" {
        (Get-Sensor -Id (Settings ChannelSensor) | Get-Channel).Count | Should Be (Settings ChannelsInTestSensor)
    }

    It "retrieves all channels from a read only user" {
        ReadOnlyClient {
            Get-Sensor -Id (Settings ChannelSensor) | Get-Channel
        }
    }
}