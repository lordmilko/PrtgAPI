. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Open-PrtgObject" -Tag @("PowerShell", "UnitTest") {
    $WhatIfPreference = $true

    SetMultiTypeResponse

    $objectCases = @(
        @{name = "sensor"; object = Get-Sensor -Count 1}
        @{name = "device"; object = Get-Device -Count 1}
        @{name = "group"; object = Get-Group -Count 1}
        @{name = "probe"; object = Get-Probe -Count 1}

        @{name = "notification action"; object = Get-NotificationAction -Count 1}
        @{name = "schedule"; object = Get-PrtgSchedule -Count 1}
    )

    It "processes a <name>" -TestCases $objectCases {
        param($object)

        $object | Open-PrtgObject
    }

    It "processes an ID" {
        Open-PrtgObject -Id 4000
    }

    It "throws when an ID doesn't exist" {
        { Open-PrtgObject -Id 6000 -ErrorAction Stop } | Should Throw "Failed to retrieve object with ID '6000': object does not exist"
    }

    It "throws when an ID does not point to an object with a Url" {

        { Open-PrtgObject -Id 7000 -ErrorAction Stop } | Should Throw "Cannot open object 'Volume IO _Total' of type 'System': object does not have a 'Url' property."
    }

    $WhatIfPreference = $false
}