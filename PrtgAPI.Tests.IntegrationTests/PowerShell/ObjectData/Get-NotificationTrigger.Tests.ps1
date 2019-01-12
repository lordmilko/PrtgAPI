. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-NotificationTrigger_IT" -Tag @("PowerShell", "IntegrationTest") {
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

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-Device (Settings Device) | Get-NotificationTrigger
        }
    }
}
