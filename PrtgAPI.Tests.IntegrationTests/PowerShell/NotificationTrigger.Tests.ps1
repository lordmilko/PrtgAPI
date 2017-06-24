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
}

. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Add-NotificationTrigger_IT" {
	Context "Create from scratch" {
		It "creates a state trigger" {

			$existing = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false
			$existing | Should Be $null

			$param = New-TriggerParameter (Settings Group) State
			$param | Add-Trigger

			$new = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false

			$new | Remove-Trigger

			$new.Count | Should Be 1
			$new.Type | Should Be State

		}

		It "creates a change trigger" {
			$existing = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false
			$existing | Should Be $null

			$param = New-TriggerParameter (Settings Group) Change
			$param | Add-Trigger

			$new = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false

			$new | Remove-Trigger

			$new.Count | Should Be 1
			$new.Type | Should Be Change
		}

		It "creates a volume trigger" {
			$existing = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false
			$existing | Should Be $null

			$param = New-TriggerParameter (Settings Group) Volume
			$param | Add-Trigger

			$new = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false

			$new | Remove-Trigger

			$new.Count | Should Be 1
			$new.Type | Should Be Volume
		}

		It "creates a speed trigger" {
			$existing = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false
			$existing | Should Be $null

			$param = New-TriggerParameter (Settings Group) Speed
			$param | Add-Trigger

			$new = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false

			$new | Remove-Trigger

			$new.Count | Should Be 1
			$new.Type | Should Be Speed
		}

		It "creates a threshold trigger for a group" {
			$existing = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false
			$existing | Should Be $null

			$param = New-TriggerParameter (Settings Group) Threshold
			$param | Add-Trigger

			$new = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false

			$new | Remove-Trigger

			$new.Count | Should Be 1
			$new.Type | Should Be Threshold
		}

		It "creates a threshold trigger for a sensor with a Channel" {
			throw
		}

		It "creates a threshold trigger for a sensor with a Channel ID" {
			#todo: when adding a trigger we need to ask prtg for the device's device type, and if we're not adding a valid channel
			#type for that object we need to throw an exception
			throw
		}
	}

	Context "Clone existing" {
		It "clones a state trigger" {
			throw
		}

		It "clones a change trigger" {
			throw
		}

		It "clones a volume trigger" {
			throw
		}

		It "clones a speed trigger" {
			throw
		}

		It "clones a threshold trigger from a device" {
			throw
		}

		It "clones a threshold trigger from a sensor" {
			throw
		}
	}
}

Describe "Edit-NotificationTriggerProperty_IT" {
	It "can edit OnNotificationAction" {
		$trigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false
		$action = Get-NotificationAction *ticket*

		$trigger.Count | Should Be 1
		$action.Count | Should Be 1

		$trigger.OnNotificationAction | Should Not Be $action.Name

		# WARNING - when running our tests its important the assembly tests are run first. if theyre not, thats a huge problem and so we'll
		# need to do a backup from within powershell

		#maybe, in order to determine whether we've run initialization or not, we have a private static variable we set in c#. both the c# and powershell
		# versions check if its been set and if so dont do the preinit stuff

		#we dont need to implement that functionality in powershell, we can put the init stuff in c# in a static method that takes no arguments and just call it from powershell

		#throw

		$trigger | Edit-NotificationTriggerProperty OnNotificationAction $action

		$newTrigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false

		$newTrigger.OnNotificationAction.Name | Should Be $action.Name
	}
}