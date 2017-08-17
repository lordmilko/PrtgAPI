. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Edit-NotificationTriggerProperty_IT" {
	It "can edit OnNotificationAction" {
		$trigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false
		$action = Get-NotificationAction *ticket*

		$trigger.Count | Should Be 1
		$action.Count | Should Be 1

		$trigger.OnNotificationAction | Should Not Be $action.Name

		$trigger | Edit-NotificationTriggerProperty OnNotificationAction $action

		$newTrigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false

		$newTrigger.OnNotificationAction.Name | Should Be $action.Name
	}

	It "can edit types requiring Parse()" {

		$device = Get-Device -Id (Settings Device)

		$trigger = $device | Get-Trigger -Type Threshold -Inherited $false

		$trigger | Edit-NotificationTriggerProperty Channel Primary

		$postTrigger = $device | Get-Trigger -Type Threshold -Inherited $false
		$postTrigger.Channel | Should Be "Primary"

		$postTrigger | Edit-NotificationTriggerProperty Channel Total

		$finalTrigger = $device | Get-Trigger -Type Threshold -Inherited $false
		$finalTrigger.Channel | Should Be "Total"
	}

	It "ignores Parse() errors" {
		$device = Get-Device -Id (Settings Device)

		$trigger = $device | Get-Trigger -Type Threshold -Inherited $false

		{ $trigger | Edit-NotificationTriggerProperty Channel "blah" } | Should Throw "Object of type 'System.String' cannot be converted to type 'PrtgAPI.TriggerChannel'"
	}

	It "throws setting an Channel TriggerChannel on a device" {
		$device = Get-Device -Id (Settings Device)

		$trigger = $device | Get-Trigger -Type Threshold -Inherited $false
		
		$channel = Get-Channel -SensorId (Settings ChannelSensor) | where Id -EQ (Settings Channel)

		{ $trigger | Edit-NotificationTriggerProperty Channel $channel } | Should Throw "is not a valid value"
	}

	It "throws setting an enum TriggerChannel on a sensor" {
		$sensor = Get-Sensor -Id (Settings DownSensor)
		$channel = $sensor | Get-Channel | select -First 1

		$param = New-TriggerParameter $sensor.Id Threshold
		$param.Channel = $channel

		$param | Add-Trigger

		$trigger = @($sensor | Get-Trigger -Type Threshold -Inherited $false)

		$trigger.Count | Should Be 1

		try
		{
			{ $trigger | Edit-TriggerProperty Channel "Primary" } | Should Throw "Channel 'Primary' is not a valid value"
		}
		finally
		{
			$trigger | Remove-Trigger
		}
	}

	It "throws setting an invalid Channel TriggerChannel on a sensor" {
		$sensor = Get-Sensor -Id (Settings DownSensor)
		$channel = $sensor | Get-Channel | select -First 1

		$param = New-TriggerParameter $sensor.Id Threshold
		$param.Channel = $channel

		$param | Add-Trigger

		$trigger = @($sensor | Get-Trigger -Type Threshold -Inherited $false)

		$trigger.Count | Should Be 1

		$badChannel = Get-Channel -SensorId (Settings ChannelSensor)|select -First 1

		try
		{
			{ $trigger | Edit-TriggerProperty Channel $badChannel } | Should Throw "is not a valid value"
		}
		finally
		{
			$trigger | Remove-Trigger
		}
	}
}