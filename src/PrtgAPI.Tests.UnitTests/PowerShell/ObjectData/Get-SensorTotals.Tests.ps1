. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Get-SensorTotals" -Tag @("PowerShell", "UnitTest") {

    $item = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.SensorTotalsItem -ArgumentList @("1.2.3.4", "241", "6", "6", "1", "1", "12", "203", "9", "13/01/2017 9:00:17 PM")

    SetResponseAndClientWithArguments "SensorTotalsResponse" $item

    It "can execute" {
        $totals = Get-SensorTotals

        $totals.GetType().Name | Should Be "SensorTotals"
    }
}