. $PSScriptRoot\Support\Standalone.ps1

function SetSensorHistoryResponse
{
    $item1 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", $null, "100 %", "0000010000")

    $channel1 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryChannelItem -ArgumentList @("0", "Percent Available Memory", "<1 %", "0")
    $channel2 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryChannelItem -ArgumentList @("1", "Available Memory", "< 1 MByte", "0")

    $item2 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", @($channel1, $channel2), "100 %", "0000010000")

    SetResponseAndClientWithArguments "SensorHistoryResponse" @($item2, $item1)
}

Describe "Get-SensorHistory" {
    SetSensorHistoryResponse

    It "creates a custom format" {
        $path = "$env:temp\PrtgAPIFormats"

        $sensor = Get-Sensor

        $sensor | Get-SensorHistory

        (gci $path).Count | Should BeGreaterThan 0
    }

    It "specifies a custum timespan and average" {
        $sensor = Get-Sensor

        $sensor | Get-SensorHistory -StartDate (get-date).AddHours(-3) -EndDate (get-date).AddHours(-1) -Average 300
    }

    It "processes a ValueLookup" {
        try
        {
            $channel = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryChannelItem -ArgumentList @("0", "Backup State", "Success", "Success")
            $item = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", $channel, "100 %", "0000010000")

            SetResponseAndClientWithArguments "SensorHistoryResponse" $item

            $sensor = Get-Sensor

            $history = $sensor | Get-SensorHistory

            $history.BackupState | Should Be "Success"
        }
        finally
        {
            SetSensorHistoryResponse
        }
    }
}