. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "Get-NotificationTrigger_IT" {
	It "can retrieve all triggers" {
		$triggers = Get-Device -Id (Settings Device) | Get-NotificationTrigger

		($triggers|where inherited -EQ $true).Count|Should Be 1
		($triggers|where inherited -NE $true).Count|Should Be 5
	}

	It "can retrieve uninherited triggers" {
		$triggers = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Inherited $false

		($triggers|where type -EQ state).Count|Should Be 1
		($triggers|where type -EQ volume).Count|Should Be 1
		($triggers|where type -EQ speed).Count|Should Be 1
		($triggers|where type -EQ change).Count|Should Be 1
		($triggers|where type -EQ threshold).Count|Should Be 1
	}

	It "can filter by OnNotificationAction" {
		$triggers = Get-Device -Id (Settings Device) | Get-NotificationTrigger *ticket* -Inherited $false

        $triggers.Count | Should Be 1
	}

	It "can filter by type" {
		$triggers = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false

		$triggers.Count|Should Be 1
	}

	It "resolves the channel of an inherited trigger" {

		$triggers = @(Get-Sensor -Id (Settings UpSensor) | Get-NotificationTrigger -Type Threshold | where Inherited -EQ $true)

		$triggers.Count | Should Be 1

		$triggers.Channel | Should Be "Total"
	}

	It "throws when a device has an invalid trigger channel" {
		#also: we should check what the value of commentStr was when we failed to get the channel
		#we DO need those corruption tests, and we need async ones as well! (i dont think we've implemented it at all and im not sure what sort of exception we need to catch
		throw
	}

	It "throws when a sensor has an invalid trigger channel" {
		throw
	}
}
