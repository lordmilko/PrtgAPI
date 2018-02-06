. $PSScriptRoot\..\Support\IntegrationTest.ps1

function TestScanningInterval($expectedString, $value)
{
    $sensor = Get-Sensor -Id (Settings UpSensor)

    $initialInterval = $($sensor | Get-ObjectProperty).Interval
    $initialInterval | Should Not Be $expectedString

    $sensor | Set-ObjectProperty interval $value

    $newInterval = $($sensor | Get-ObjectProperty).Interval

    $newInterval | Should Be $expectedString
}

Describe "Set-ObjectProperty_IT" {
    
    It "sets a raw property" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $initialTimeout = ($sensor | Get-ObjectProperty).Timeout
        $initialTimeout | Should Not Be 7

        $sensor | Set-ObjectProperty -RawProperty "timeout_" -RawValue "7" -Force

        $newTimeout = ($sensor | Get-ObjectProperty).Timeout

        $newTimeout | Should Be 7
    }

    It "sets a scanning interval from an enum"       { TestScanningInterval "01:00:00" OneHour }

    It "sets a scanning interval from a TimeSpan"    { TestScanningInterval "00:10:00" ([TimeSpan]"00:10") }

    It "sets a scanning interval from an integer"    { TestScanningInterval "00:05:00" 300 }

    It "can compare a ScanningInterval with a ScanningInterval" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $initialInterval = $($sensor | Get-ObjectProperty).Interval

        $sensor | Set-ObjectProperty "Interval" $initialInterval

        $newInterval = $($sensor | Get-ObjectProperty).Interval

        $initialInterval | Should Be $newInterval
    }

    It "sets a custom scanning interval"             { TestScanningInterval "00:00:10" (Settings CustomInterval) }

    It "sets a custom unsupported scanning interval" { TestScanningInterval "00:00:05" (Settings CustomUnsupportedInterval) }

    It "throws setting an empty value on a required property" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $message = "failed due to the following: Sensor Name: Required field, not defined. The object has not been changed"

        { $sensor | Set-ObjectProperty Name $null } | Should Throw $message
    }

    It "setting an invalid scanning interval sets the nearest valid interval" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $initialInterval = $($sensor | Get-ObjectProperty).Interval
        $initialInterval | Should Not Be "00:00:55"

        $sensor | Set-ObjectProperty interval "00:00:55"

        $newInterval = $($sensor | Get-ObjectProperty).Interval

        $newInterval | Should Be "00:01:00"
    }

    It "sets a grandchild, and enables its parent and grandparent" {
        
        $device = Get-Device -Id (Settings Device)

        $initialSettings = $device | Get-ObjectProperty
        $initialInherit = $initialSettings.InheritDBCredentials
        $initialAuthMode = $initialSettings.DBAuthMode
        $initialUserName = $initialSettings.DBUserName

        $device | Set-ObjectProperty DBUserName "grandChildTest"

        $newSettings = $device | Get-ObjectProperty
        $newInherit = $newSettings.InheritDBCredentials
        $newAuthMode = $newSettings.DBAuthMode
        $newUserName = $newSettings.DBUserName

        $initialInherit | Assert-NotEqual $newInherit -Message "Initial inherit and new inherit were both <actual>"
        $initialAuthMode | Assert-NotEqual $newAuthMode -Message "Initial authmode and new authmode were both <actual>"
        $initialUserName | Assert-NotEqual $newUserName -Message "Initial username and new username were both <actual>"

        $newInherit | Should Be $false
        $newAuthMode | Should Be SQL
        $newUserName | Should Be "grandChildTest"
    }

    It "can set the properties of multiple in a single request" {
        $upSensor = Get-Sensor -Id (Settings UpSensor)
        $upSensor.Interval | Should Not Be "00:05:00"

        $device = Get-Device -Id (Settings Device)
        $device.Interval | Should Be "00:01:00"

        $objects = $upSensor,$device

        $objects | Set-ObjectProperty Interval "00:05:00"

        LogTestDetail "Sleeping for 10 seconds while settings apply"
        Sleep 10

        $newUpSensor = Get-Sensor -Id (Settings UpSensor)
        $newUpSensor.Interval | Should Be "00:05:00"

        $newDevice = Get-Device -Id (Settings Device)
        $newDevice.Interval | Should Be "00:05:00"
    }

    function SetDirect($property, $value)
    {
        $object = Get-Sensor -Id (Settings UpSensor)

        $initialValue = $object | Get-ObjectProperty $property
        $object | Set-ObjectProperty $property $value

        $newValue = $object | Get-ObjectProperty $property

        $newValue | Should Not Be $initialValue
        $newValue | Should Be $value

        $object | Set-ObjectProperty $property $initialValue
    }

    It "can set direct properties" {
        SetDirect "InheritTriggers" $false
        SetDirect "Comments" "test comment!"
    }
}