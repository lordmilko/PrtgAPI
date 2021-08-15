. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Add-NotificationTrigger" -Tag @("PowerShell", "UnitTest") {

    SetResponseAndClient "SetNotificationTriggerResponse"

    It "throws setting an unsupported trigger type" {

        $params = New-TriggerParameters 1001 State

        { $params | Add-NotificationTrigger } | Should Throw "is not a valid trigger type"
    }

    It "executes with -WhatIf" {
        $params = New-TriggerParameters 1001 State

        $params | Add-NotificationTrigger -WhatIf
    }

    It "resolves a created trigger" {
        SetResponseAndClientWithArguments "DiffBasedResolveResponse" $false

        $params = New-TriggerParameters 1001 Threshold
        $params.Channel = 1
        $params.OnNotificationAction.Id = 301

        $trigger = $params | Add-NotificationTrigger -Resolve

        $trigger.SubId | Should Be 2
    }

    Context "New-Trigger" {

        $sensor = Run Sensor { Get-Sensor } | Select -First 1
        
        It "creates a state trigger" {
            SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=state"
                    "escnotificationid_new=-1%7CNone"
                    "offnotificationid_new=-1%7CNone"
                    "nodest_new=0"
                    "latency_new=60"
                    "esclatency_new=300"
                    "repeatival_new=0"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $sensor | New-Trigger -Type State -Resolve:$false
        }

        It "creates a volume trigger" {
            $response = SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::Sensors("filter_objid=2203", [Request]::DefaultObjectFlags)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=volume"
                    "channel_new=-999"
                    "unitsize_new=6"
                    "period_new=0"
                    "threshold_new=0"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $response.CountOverride = GetCustomCountDictionary @{
                Sensors = 0
            }

            $sensor | New-Trigger -Type Volume -Resolve:$false
        }

        It "creates a speed trigger" {
            $response = SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::Sensors("filter_objid=2203", [Request]::DefaultObjectFlags)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=speed"
                    "offnotificationid_new=-1%7CNone"
                    "channel_new=-999"
                    "condition_new=0"
                    "unitsize_new=6"
                    "unittime_new=3"
                    "threshold_new=0"
                    "latency_new=60"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $response.CountOverride = GetCustomCountDictionary @{
                Sensors = 0
            }

            $sensor | New-Trigger -Type Speed -Resolve:$false
        }

        It "creates a threshold trigger" {
            $response = SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::Sensors("filter_objid=2203", [Request]::DefaultObjectFlags)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=threshold"
                    "offnotificationid_new=-1%7CNone"
                    "channel_new=-999"
                    "condition_new=0"
                    "threshold_new=0"
                    "latency_new=60"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $response.CountOverride = GetCustomCountDictionary @{
                Sensors = 0
            }

            $sensor | New-Trigger -Type Threshold -Resolve:$false
        }

        It "creates a change trigger" {
            SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=change"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $sensor | New-Trigger -Type Change -Resolve:$false
        }

        It "parses a channel name" {

            SetAddressValidatorResponse @(
                [Request]::Channels(2203)
                [Request]::ChannelProperties(2203, 1)
                [Request]::TriggerTypes(2203)
                [Request]::Sensors("filter_objid=2203", [Request]::DefaultObjectFlags)
                [Request]::Channels(2203)
                [Request]::ChannelProperties(2203, 1)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=threshold"
                    "offnotificationid_new=-1%7CNone"
                    "channel_new=1"
                    "condition_new=0"
                    "threshold_new=0"
                    "latency_new=60"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $sensor | New-Trigger -Type Threshold -Channel "*Avail*" -Resolve:$false
        }

        It "parses a standard channel for a sensor" {
            $response = SetAddressValidatorResponse @(
                [Request]::Channels(2203)
                [Request]::ChannelProperties(2203, -1)
                [Request]::TriggerTypes(2203)
                [Request]::Sensors("filter_objid=2203", [Request]::DefaultObjectFlags)
                [Request]::Channels(2203)
                [Request]::ChannelProperties(2203, -1)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=threshold"
                    "offnotificationid_new=-1%7CNone"
                    "channel_new=-1"
                    "condition_new=0"
                    "threshold_new=0"
                    "latency_new=60"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $response.ItemOverride = GetCustomItemOverrideDictionary @{
                "Channel" = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.ChannelItem -ArgumentList @("26 %", "0000000000000260.0000", "-1", "0000000001", "Total")
            }

            $sensor | New-Trigger -Type Threshold -Channel "Total" -Resolve:$false
        }

        It "parses a standard channel for a container" {
            $device = Run Device { Get-Device }

            $response = SetAddressValidatorResponse @(
                [Request]::TriggerTypes(40)
                [Request]::Sensors("filter_objid=40", [Request]::DefaultObjectFlags)
                [Request]::EditSettings(@(
                    "id=40"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=threshold"
                    "offnotificationid_new=-1%7CNone"
                    "channel_new=-1"
                    "condition_new=0"
                    "threshold_new=0"
                    "latency_new=60"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $response.CountOverride = GetCustomCountDictionary @{
                Sensors = 0
            }

            $device | New-Trigger -Type Threshold -Channel "Total" -Resolve:$false
        }

        It "parses a channel ID" {
            SetAddressValidatorResponse @(
                [Request]::Channels(2203)
                [Request]::ChannelProperties(2203, 1)
                [Request]::TriggerTypes(2203)
                [Request]::Sensors("filter_objid=2203", [Request]::DefaultObjectFlags)
                [Request]::Channels(2203)
                [Request]::ChannelProperties(2203, 1)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=threshold"
                    "offnotificationid_new=-1%7CNone"
                    "channel_new=1"
                    "condition_new=0"
                    "threshold_new=0"
                    "latency_new=60"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $sensor | New-Trigger -Type Threshold -Channel 1 -Resolve:$false
        }

        It "throws when an invalid channel ID is specified" {
            SetMultiTypeResponse

            { $sensor | New-Trigger -Type Threshold -Channel 5 } | Should Throw "Failed to retrieve channel with ID '5': Channel does not exist."
        }

        It "parses a TriggerChannel object" {
            SetMultiTypeResponse

            $trigger = $sensor | Get-Trigger -Type Threshold

            $trigger.Channel | Should Be "Primary"

            $response = SetAddressValidatorResponse @(
                [Request]::TriggerTypes(40)
                [Request]::Sensors("filter_objid=40", [Request]::DefaultObjectFlags)
                [Request]::EditSettings(@(
                    "id=40"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=threshold"
                    "offnotificationid_new=-1%7CNone"
                    "channel_new=-999"
                    "condition_new=0"
                    "threshold_new=0"
                    "latency_new=60"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $response.CountOverride = GetCustomCountDictionary @{
                Sensors = 0
            }

            $device = Run Device { Get-Device }

            $device | New-Trigger -Type Threshold -Channel $trigger.Channel -Resolve:$false
        }

        It "parses a NotificationAction name" {
            $response = SetAddressValidatorResponse @(
                [Request]::Notifications("", [Request]::DefaultObjectFlags)
                [Request]::NotificationProperties(300)
                [Request]::TriggerTypes(2203)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=300%7CEmail+and+push+notification+to+admin"
                    "class=state"
                    "escnotificationid_new=-1%7CNone"
                    "offnotificationid_new=-1%7CNone"
                    "nodest_new=0"
                    "latency_new=60"
                    "esclatency_new=300"
                    "repeatival_new=0"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $response.CountOverride = GetCustomCountDictionary @{
                Notifications = 1
            }

            $sensor | New-Trigger -Type State -OnNotificationAction *email* -Resolve:$false
        }

        It "parses a NotificationAction object" {
            SetMultiTypeResponse

            $action = Get-NotificationAction -Count 1
            $action.Id | Should Be 300

            SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=300%7CEmail+and+push+notification+to+admin"
                    "class=state"
                    "escnotificationid_new=-1%7CNone"
                    "offnotificationid_new=-1%7CNone"
                    "nodest_new=0"
                    "latency_new=60"
                    "esclatency_new=300"
                    "repeatival_new=0"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $sensor | New-Trigger -Type State -OnNotificationAction $action -Resolve:$false
        }

        It "parses a `$null NotificationAction" {
            SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=state"
                    "escnotificationid_new=-1%7CNone"
                    "offnotificationid_new=-1%7CNone"
                    "nodest_new=0"
                    "latency_new=60"
                    "esclatency_new=300"
                    "repeatival_new=0"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $sensor | New-Trigger -Type State -OnNotificationAction $null -Resolve:$false
        }

        It "parses a 'None' NotificationAction" {

            SetMultiTypeResponse

            $action = ($sensor | Get-Trigger | select -First 1).OffNotificationAction

            $action | Should Be "None"

            SetAddressValidatorResponse @(
                [Request]::TriggerTypes(2203)
                [Request]::EditSettings(@(
                    "id=2203"
                    "subid=new"
                    "onnotificationid_new=-1%7CNone"
                    "class=state"
                    "escnotificationid_new=-1%7CNone"
                    "offnotificationid_new=-1%7CNone"
                    "nodest_new=0"
                    "latency_new=60"
                    "esclatency_new=300"
                    "repeatival_new=0"
                    "objecttype=nodetrigger"
                ) -join "&")
            )

            $sensor | New-Trigger -Type State -OnNotificationAction $action -Resolve:$false
        }

        It "throws when an invalid NotificationAction is specified" {
            SetMultiTypeResponse

            { $sensor | New-Trigger -Type State -OnNotificationAction *banana* -Resolve:$false } | Should Throw "Could not find a notification action matching the wildcard expression '*banana*' for use with parameter 'OnNotificationAction'."
        }

        It "throws when an ambiguous NotificationAction wildcard is specified" {
            SetMultiTypeResponse

            { $sensor | New-Trigger -Type State -OnNotificationAction *email* -Resolve:$false } | Should Throw "Notification Action wildcard '*email*' on parameter 'OnNotificationAction' is ambiguous"
        }

        It "throws when an invalid parameter is specified for a trigger type" {
            SetMultiTypeResponse

            $device = Run Device { Get-Device }

            { $device | New-Trigger -Type State -Channel Primary -Resolve:$false } | Should Throw "Property 'Channel' is not a valid property for a trigger of type 'State'."
        }

        It "throws when an invalid channel is specified for a sensor" {
            SetMultiTypeResponse

            { $sensor | New-Trigger -Type Threshold -Channel "test" -Resolve:$false } | Should Throw "Channel wildcard 'test' does not exist on sensor 'Volume IO _Total' (ID: 2203). Specify one of the following channel names and try again: 'Percent Available Memory'"
        }

        It "throws when an invalid channel is specified for a container" {
            $response = SetMultiTypeResponse

            $response.CountOverride = GetCustomCountDictionary @{
                Sensors = 0
            }

            $device = Get-Device -Count 1

            { $device | New-Trigger -Type Threshold -Channel test } | Should Throw "Cannot convert value 'test' of type 'System.String' to type 'TriggerChannel'. Value type must be convertable to one of PrtgAPI.StandardTriggerChannel, PrtgAPI.Channel or System.Int32."
        }

        It "throws when a null channel is specified" {
            SetMultiTypeResponse

            $device = Get-Device -Count 1

            { $device | New-Trigger -Type Threshold -Channel $null } | Should Throw "Cannot specify 'null' for parameter 'Channel'"
        }

        It "ignores null when assigned to a property that doesn't convert it" {

            # Note: as of writing we don't have any nullable NotificationTrigger properties besides *NotificationAction
            # and Channel, which are both special cases

            SetMultiTypeResponse

            $device = Get-Device -Count 1

            { $device | New-Trigger -Type State -State $null } | Should Throw "Value 'null' could not be assigned to property 'State' of type 'PrtgAPI.TriggerSensorState'"
        }
    }
}