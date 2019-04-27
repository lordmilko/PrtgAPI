. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function GetTrigger($type)
{
    $sensor = Get-Sensor
    $sensor.Id = 1

    $trigger = Run NotificationTrigger {
        $sensor | Get-Trigger -Type $type | Select -First 1
    }

    return $trigger
}

function InitializeTriggerChannelResponse($parentId = 4000)
{
    $response = SetMultiTypeResponse

    $sensor = Get-Sensor -Count 1

    $triggerItem = [PrtgAPI.Tests.UnitTests.Support.TestItems.NotificationTriggerItem]::ThresholdTrigger(
        "60", # latency
        "301|Email to all members of group PRTG Users Group", # offNotificationAction
        "Backup State", # channel
        "Not Equal to", # condition
        "5", # threshold
        "303|Email and push notification to admin", # onNotificationAction
        $parentId
    )

    $channelItem = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.ChannelItem -ArgumentList @("26 %", "0000000000000260.0000", "1", "0000000001", "Backup State")
    $response = SetResponseAndClientWithArguments "NotificationTriggerResponse" @($triggerItem, $channelItem)

    $trigger = $sensor | Get-Trigger -Type Threshold | Select -First 1

    return $trigger
}

Describe "Set-NotificationTriggerProperty" -Tag @("PowerShell", "UnitTest") {

    SetResponseAndClient "SetNotificationTriggerResponse"

    It "sets a TriggerChannel from a property" {
        $sensor = Get-Sensor
        $sensor.Id = 1

        $trigger = Run NotificationTrigger {
            $sensor | Get-Trigger -Type Threshold
        }

        $trigger.Count | Should Be 1
        $trigger.Inherited | Should Be $false

        $trigger | Set-NotificationTriggerProperty Channel Primary
    }

    It "sets an invalid property" {
        $sensor = Get-Sensor

        $sensor.Id = 0

        $trigger = $sensor | Get-Trigger

        $trigger.Inherited | Should Be $false

        { $trigger | Set-NotificationTriggerProperty Channel Primary } | Should Throw "Property 'Channel' is not a valid property for a trigger of type 'State'"
    }

    It "processes a state trigger" {
        $sensor = Get-Sensor
        $sensor.Id = 1

        $trigger = Run NotificationTrigger {
            $sensor | Get-Trigger -Type Threshold
        }

        $trigger | Set-NotificationTriggerProperty OffNotificationAction $null
    }

    It "processes a channel trigger" {
        
        $trigger = GetTrigger "Change"
        $trigger.GetType().Name | Should Be "NotificationTrigger"

        $trigger | Set-NotificationTriggerProperty OnNotificationAction $null
    }

    It "processes a speed trigger" {
        $trigger = GetTrigger "Speed"

        $trigger | Set-NotificationTriggerProperty Latency 70
    }

    It "processes a volume trigger" {
        $trigger = GetTrigger "Volume"

        $trigger | Set-NotificationTriggerProperty UnitSize KByte
    }

    It "throws trying to edit an inherited trigger" {
        $trigger = Get-Sensor | Get-Trigger

        $trigger.Inherited | Should Be $true

        { $trigger | Set-NotificationTriggerProperty Channel Primary } | Should Throw "this trigger is inherited"
    }

    It "throws when a specified trigger doesn't exist" {
        { Set-TriggerProperty -ObjectId 1 -SubId 99 OnNotificationAction $null } | Should Throw "Failed to retrieve notification trigger with SubId '99': Notification Trigger does not exist."
    }

    It "executes with -WhatIf" {
        $trigger = GetTrigger "Speed"

        $trigger | Set-NotificationTriggerProperty Latency 70 -WhatIf
    }

    It "passes through" {
        $trigger = GetTrigger "Speed"

        $newTrigger = $trigger | Set-NotificationTriggerProperty Latency 50 -PassThru

        $newTrigger | Should Be $trigger
    }

    It "specifies a NotificationAction wildcard to a normal parameter" {

        $response = SetMultiTypeResponse

        $trigger = Get-Sensor -Count 1 | Get-Trigger | Select -First 1
        $trigger.ObjectId = 0

        $response = SetAddressValidatorResponse @(
            [Request]::Notifications("", [Request]::DefaultObjectFlags)
            [Request]::NotificationProperties(300)
            [Request]::EditSettings(@(
                "id=0"
                "subid=1"
                "onnotificationid_1=300%7CEmail+and+push+notification+to+admin"
            ) -join "&")
        )

        $response.CountOverride = GetCustomCountDictionary @{
            Notifications = 1
        }

        $trigger | Set-TriggerProperty OnNotificationAction *email*
    }

    It "throws when an ambiguous NotificationAction wildcard is specified" {
        SetMultiTypeResponse

        $trigger = Get-Sensor -Count 1 | Get-Trigger | Select -First 1

        { $trigger | Set-TriggerProperty -OnNotificationAction *email* } | Should Throw "Notification Action wildcard '*email*' on parameter 'OnNotificationAction' is ambiguous"
    }

    It "specifies a NotificationAction wildcard to a dynamic parameter" {
        SetMultiTypeResponse

        $trigger = Get-Sensor -Count 1 | Get-Trigger | Select -First 1
        $trigger.ObjectId = 0

        $response = SetAddressValidatorResponse @(
            [Request]::Notifications("", [Request]::DefaultObjectFlags)
            [Request]::NotificationProperties(300)
            [Request]::EditSettings(@(
                "id=0"
                "subid=1"
                "onnotificationid_1=300%7CEmail+and+push+notification+to+admin"
            ) -join "&")
        )

        $response.CountOverride = GetCustomCountDictionary @{
            Notifications = 1
        }

        $trigger | Set-TriggerProperty -OnNotificationAction *email*
    }

    It "specifies a NotificationAction object to a normal parameter" {
        SetMultiTypeResponse

        $trigger = Get-Sensor -Count 1 | Get-Trigger | Select -First 1
        $trigger.ObjectId = 0

        $action = Get-NotificationAction -Count 1

        SetAddressValidatorResponse @(
            [Request]::EditSettings(@(
                "id=0"
                "subid=1"
                "onnotificationid_1=300%7CEmail+and+push+notification+to+admin"
            ) -join "&")
        )

        $trigger | Set-TriggerProperty OnNotificationAction $action
    }

    It "specifies an invalid object type to a NotificationAction parameter" {
        SetMultiTypeResponse

        $trigger = Get-Sensor -Count 1 | Get-Trigger | Select -First 1
        $trigger.ObjectId = 0

        { $trigger | Set-TriggerProperty OnNotificationAction $true } | Should Throw "Expected a value of type 'PrtgAPI.NotificationAction' while parsing property 'OnNotificationAction' however received a value of type 'System.Boolean'."
    }

    It "specifies a Channel wildcard to a normal parameter" {

        $trigger = InitializeTriggerChannelResponse

        SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::EditSettings(@(
                "id=4000"
                "subid=7"
                "channel_7=1"
            ) -join "&")
        )

        $trigger | Set-TriggerProperty Channel "*Avail*"
    }

    It "specifies a Channel wildcard to a dynamic parameter" {
        $trigger = InitializeTriggerChannelResponse

        SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::EditSettings(@(
                "id=4000"
                "subid=7"
                "channel_7=1"
            ) -join "&")
        )

        $trigger | Set-TriggerProperty -Channel "*Avail*"
    }

    It "specifies a Channel object to a normal parameter" {
        $trigger = InitializeTriggerChannelResponse

        SetMultiTypeResponse

        $channel = Get-Channel -SensorId 4000 "*Avail*"

        SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::EditSettings(@(
                "id=4000"
                "subid=7"
                "channel_7=1"
            ) -join "&")
        )

        $trigger | Set-TriggerProperty Channel $channel
    }

    It "specifies a channel ID" {
        $trigger = InitializeTriggerChannelResponse

        SetMultiTypeResponse

        SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, 1)
            [Request]::EditSettings(@(
                "id=4000"
                "subid=7"
                "channel_7=1"
            ) -join "&")
        )

        $trigger | Set-TriggerProperty Channel 1
    }

    It "throws specifying an invalid channel ID" {

        $trigger = InitializeTriggerChannelResponse

        SetMultiTypeResponse

        { $trigger | Set-TriggerProperty Channel 2 } | Should Throw "Failed to retrieve channel with ID '2': Channel does not exist."
    }

    It "specifies a standard channel for a sensor" {
        $trigger = InitializeTriggerChannelResponse

        SetMultiTypeResponse

        $response = SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, -1)
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Channels(4000)
            [Request]::ChannelProperties(4000, -1)
            [Request]::EditSettings(@(
                "id=4000"
                "subid=7"
                "channel_7=-1"
            ) -join "&")
        )
        $response.ItemOverride = GetCustomItemOverrideDictionary @{
            "Channel" = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.ChannelItem -ArgumentList @("26 %", "0000000000000260.0000", "-1", "0000000001", "Total")
        }

        $trigger | Set-TriggerProperty Channel "total"
    }

    It "specifies a standard channel for a container" {
        
        $trigger = InitializeTriggerChannelResponse 3000
        $trigger.ObjectId = 3000

        $response = SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=3000", [Request]::DefaultObjectFlags)
            [Request]::Sensors("filter_objid=3000", [Request]::DefaultObjectFlags)
            [Request]::EditSettings(@(
                "id=3000"
                "subid=7"
                "channel_7=-1"
            ) -join "&")#>
        )

        $response.CountOverride = GetCustomCountDictionary @{
            "Sensor" = 0
        }

        $trigger | Set-TriggerProperty Channel "total"
    }
}