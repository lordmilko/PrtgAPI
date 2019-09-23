. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-Object_IT" -Tag @("PowerShell", "IntegrationTest") {

    It "filters by enum types" {

        $objects = Get-Object -Type System

        $objects.Count | Should BeGreaterThan 0

        $objects | Assert-All { $_.Type -eq "System" }
    }

    It "filters by string types" {
        $objects = Get-Object -Type ping

        $objects.Count | Should BeGreaterThan 0

        $objects | Assert-All { $_.Type -eq "ping" }
    }

    It "filters by enum types and string types at once" {
        $objects = Get-Object -Type System,ping

        $objects | Assert-All { $_.Type -eq "ping" -or $_.Type -eq "System" }
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-Object
        }
    }

    It "pipes from another object" {
        $notifications = Get-Object -Id -3 | Get-Object

        $actions = Get-NotificationAction
        $notifications.Count | Should Be $actions.Count

        foreach($action in $notifications)
        {
            $action.Type | Should Be "Notification"
        }
    }
}