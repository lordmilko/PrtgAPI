. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

function SetChannelObjectResponse
{
    $triggerItem = [PrtgAPI.Tests.UnitTests.Support.TestItems.NotificationTriggerItem]::ThresholdTrigger("60", "301|Email to all members of group PRTG Users Group", "Backup State")
    $channelItem = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.ChannelItem -ArgumentList @("26 %", "0000000000000260.0000", "1", "0000000001", "Backup State")

    $response = SetResponseAndClientWithArguments "NotificationTriggerResponse" @($triggerItem, $channelItem)

    $s = Run Sensor { Get-Sensor }
    $s.Id = 4001

    return $s
}

Describe "Get-NotificationTrigger" -Tag @("PowerShell", "UnitTest") { # notificationtrigger doesnt support getitem; how do we make it use getitems
    
    $sensor = Run Sensor { Get-Sensor }
    $sensor.Count | Should Be 1

    Context "Object" {
         It "can pipe from sensors" {

            $triggers = $sensor | Get-NotificationTrigger
            $triggers.Count | Should Be 5
        }

        It "filters by Type" {
            $triggers = $sensor | Get-Trigger -Type Change,Speed
            $triggers.Count | Should Be 2

            $triggers[0].SubId | Should Be 8
            $triggers[1].SubId | Should Be 5
        }

        It "filters by SubId" {
            $triggers = $sensor | Get-Trigger -Id 7,5
            $triggers.Count | Should Be 2

            $triggers[0].SubId | Should Be 7
            $triggers[1].SubId | Should Be 5
        }

        It "filters by ParentId" {

            $triggers = $sensor | Get-Trigger -ParentId 0
            $triggers.Count | Should Be 1

            $triggers.SubId | Should Be 1
        }
    
        It "filters by dynamic parameters" {
        
            $triggers = $sensor | Get-Trigger -Latency 60 -Condition Above

            $triggers.Count | Should Be 1
            $triggers.SubId | Should Be 5
        }

        It "retrieves notification trigger types" {

            WithResponse "MultiTypeResponse" {
                $types = $sensor | Get-Trigger -Types

                $types.State | Should Be $true
            }
        }

        It "ignores null when assigned to a dynamic parameter" {
            WithResponse "MultiTypeResponse" {
                $device = Get-Device -Count 1

                $withoutFilters = $device | Get-Trigger
                $withFilters = $device | Get-Trigger -Threshold $null

                $withoutFilters.Count | Should Be $withFilters.Count
            }
        }
    }

    Context "Manual" {
        It "specifies an object ID" {
            $triggers = Get-NotificationTrigger -ObjectId 4000
            $triggers.Count | Should Be 5
        }

        It "filters by Type" {
            $triggers = Get-Trigger -ObjectId 4000 -Type Change,Speed
            $triggers.Count | Should Be 2

            $triggers[0].SubId | Should Be 8
            $triggers[1].SubId | Should Be 5
        }

        It "filters by SubId" {
            $triggers = Get-Trigger -ObjectId 4000 -Id 7,5
            $triggers.Count | Should Be 2

            $triggers[0].SubId | Should Be 7
            $triggers[1].SubId | Should Be 5
        }

        It "filters by ParentId" {
            $triggers = Get-Trigger -ObjectId 4000 -ParentId 0
            $triggers.Count | Should Be 1

            $triggers.SubId | Should Be 1
        }

        It "filters by dynamic parameters" {
            $triggers = Get-Trigger -ObjectId 4000 -Latency 60 -Condition Above

            $triggers.Count | Should Be 1
            $triggers.SubId | Should Be 5
        }

        It "retrieves notification trigger types" {
            WithResponse "MultiTypeResponse" {
                $types = Get-Trigger -ObjectId 4000 -Types

                $types.State | Should Be $true
            }
        }

        It "retrieves sensor channels" {
            $triggerItem = [PrtgAPI.Tests.UnitTests.Support.TestItems.NotificationTriggerItem]::ThresholdTrigger("60", "301|Email to all members of group PRTG Users Group", "Backup State")
            $channelItem = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.ChannelItem -ArgumentList @("26 %", "0000000000000260.0000", "1", "0000000001", "Backup State")

            WithResponseArgs "NotificationTriggerResponse" @($triggerItem, $channelItem) {
                $triggers = Get-Trigger -ObjectId 4000 -Channel "Backup State"
                $triggers.Count | Should Be 1
                $triggers.Channel.ToString() | Should Be "Backup State"
            }
        }
    }

    Context "NotificationAction" {
        It "filters by an OnNotificationAction wildcard" {

            $triggers = $sensor | Get-Trigger *push*
            $triggers.Count | Should Be 2

            $triggers[0].SubId | Should Be 7
            $triggers[1].SubId | Should Be 5
        }

        It "filters by an OffNotificationAction wildcard" {
            $triggers = $sensor | Get-Trigger -OffNotificationAction *ticket*
            $triggers.Count | Should Be 1

            $triggers.SubId | Should Be 5
        }

        It "filters by an EscalationNotificationAction wildcard" {
            $triggers = $sensor | Get-Trigger -EscalationNotificationAction None
            $triggers.Count | Should Be 1

            $triggers.SubId | Should Be 1
        }

        It "filters by an OnNotificationAction object" {
            $action = Get-NotificationAction|where id -eq 301 # NotificationTriggerResponse ignores Id, and GetNotificationAction doesn't post-filter by ID
            $action.Count | Should Be 1

            $triggers = $sensor | Get-Trigger -OnNotificationAction $action
            $triggers.Count | Should Be 3
            $triggers[0].SubId | Should Be 8
            $triggers[1].SubId | Should Be 1
            $triggers[2].SubId | Should Be 6
        }

        It "filters by an OffNotificationAction object" {
            $action = Get-NotificationAction|where id -eq 302
            $action.Count | Should Be 1

            $triggers = $sensor | Get-Trigger -OffNotificationAction $action
            $triggers.Count | Should Be 1
            $triggers.SubId | Should Be 5
        }

        It "filters by an EscalationNotificationAction object" {
            $action = ($sensor | Get-Trigger -Id 1).EscalationNotificationAction
            $action.Name | Should Be "None"

            $triggers = $sensor | Get-Trigger -EscalationNotificationAction $action
            $triggers.SubId | Should Be 1
        }
    }

    Context "Channel" {

        It "filters by standard trigger channel name" {

            $triggers = $sensor | Get-Trigger -Channel Primary
            $triggers.Count | Should Be 3
            $triggers[0].Channel.ToString() | Should Be "Primary"
            $triggers[1].Channel.ToString() | Should Be "Primary"
            $triggers[2].Channel.ToString() | Should Be "Primary"
        }

        It "filters by standard trigger channel enum value" {
            $triggers = $sensor | Get-Trigger -Channel ([PrtgAPI.StandardTriggerChannel]::Primary)
            $triggers.Count | Should Be 3
            $triggers[0].Channel.ToString() | Should Be "Primary"
            $triggers[1].Channel.ToString() | Should Be "Primary"
            $triggers[2].Channel.ToString() | Should Be "Primary"
        }

        It "filters by TriggerChannel object" {
            $trigger = $sensor | Get-Trigger -SubId 7

            $triggers = $sensor | Get-Trigger -Channel $trigger.Channel
            $triggers.Count | Should Be 3
            $triggers[0].Channel.ToString() | Should Be "Primary"
            $triggers[1].Channel.ToString() | Should Be "Primary"
            $triggers[2].Channel.ToString() | Should Be "Primary"
        }

        It "filters by channel name" {

            $s = SetChannelObjectResponse

            $triggers = $s | Get-Trigger -Channel "Backup State"
            $triggers.Count | Should Be 1
            $triggers.Channel.ToString() | Should Be "Backup State"
        }

        It "filters by a wildcard name" {
            $s = SetChannelObjectResponse

            $triggers = $s | Get-Trigger -Channel *backup*
            $triggers.Count | Should Be 1
            $triggers.Channel.ToString() | Should Be "Backup State"
        }

        It "filters by an invalid wildcard name" {
            $s = SetChannelObjectResponse

            $triggers = $s | Get-Trigger -Channel *banana*
            $triggers.Count | Should Be 0
        }

        It "filters by channel object" {
            $s = SetChannelObjectResponse

            $channel = $s | Get-Channel
            $channel.Count | Should Be 1
            $channel.Name | Should Be "Backup State"

            $triggers = $s | Get-Trigger -Channel $channel
            $triggers.Count | Should Be 1
            $triggers.Channel.ToString() | Should Be "Backup State"
        }

        It "filters by channel ID" {
            $s = SetChannelObjectResponse

            $channel = $s | Get-Channel
            $channel.Count | Should Be 1
            $channel.Name | Should Be "Backup State"

            $triggers = $s | Get-Trigger -Channel $channel.Id
            $triggers.Count | Should Be 1
            $triggers.Channel.ToString() | Should Be "Backup State"
        }
    }

    # should we maybe make getitem return an array, and have everyone use it?
}