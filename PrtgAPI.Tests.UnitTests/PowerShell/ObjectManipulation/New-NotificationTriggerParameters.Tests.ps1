. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "New-NotificationTriggerParameters" -Tag @("PowerShell", "UnitTest") {

    $device = Run Device { Get-Device }
    $triggers = Run NotificationTrigger { $device | Get-NotificationTrigger -Type State }

    It "can create Add parameter set" {
        $parameters = $device | New-TriggerParameters State
    }

    It "can create AddManual parameter set" {
        $triggers.Count | Should Be 1

        $parameters = New-TriggerParameters $device.Id State
    }

    It "can create EditManual parameter set" {
        $triggers.Count | Should Be 1

        $parameters = New-TriggerParameters $device.Id $triggers.SubId State
    }

    It "can create AddFrom parameter set" {
        $triggers.Count | Should Be 1

        $parameters = $triggers | New-TriggerParameters $device.Id

        $parameters.Action | Should Be Add
    }

    It "can create EditFrom parameter set" {
        $device2 = Run Device { Get-Device }
        $device2.Id = 0

        $trigger = Run NotificationTrigger { $device2 | Get-NotificationTrigger -Type State }

        $trigger.Count | Should Be 1

        $parameters = $trigger | New-TriggerParameters

        $parameters.Action | Should Be Edit
    }

    It "can convert StandardTriggerChannel to Channel" {
        $parameters = New-TriggerParameters $device.Id Threshold

        $parameters.Channel = "Total"

        $parameters.Channel | Should Be "Total"
    }

    It "can assign integer to Channel" {
        $parameters = New-TriggerParameters $device.Id Threshold

        $parameters.Channel = 3

        $parameters.Channel | Should Be "3"
    }

    It "can assign a PrtgAPI.Channel to Channel" {

        $sensor = Run Sensor { Get-Sensor }
        $channel = Run Channel { $sensor | Get-Channel }

        $parameters = New-TriggerParameters $device.Id Threshold

        $parameters.Channel = $channel

        $parameters.Channel | Should Be "Percent Available Memory"
    }

    $cases = @(
        @{ name = "Change" }
        @{ name = "Speed" }
        @{ name = "State" }
        @{ name = "Threshold" }
        @{ name = "Volume" }
    )

    It "creates a set of <name> trigger parameters" -TestCases $cases {

        param($name)

        $params = New-TriggerParameters 1001 $name

        $params.GetType().Name | Should Be "$($name)TriggerParameters"
    }

    It "throws assigning a random value to Channel" {
        $parameters = New-TriggerParameters $device.Id Threshold

        { $parameters.Channel = "Banana" } | Should Throw "type must be convertable"
    }

    It "throws creating parameters from an inherited trigger" {
        $triggers.Count | Should Be 1

        { $triggers | New-TriggerParameters } | Should Throw "trigger is inherited"
    }
}