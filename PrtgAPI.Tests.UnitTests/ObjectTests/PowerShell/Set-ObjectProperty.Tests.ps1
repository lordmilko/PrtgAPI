. $PSScriptRoot\Support\Standalone.ps1

Describe "Set-ObjectProperty" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    $sensor = Get-Sensor

    It "sets a property with a valid type" {
        $sensor | Set-ObjectProperty Name "Test"
    }

    It "sets a property with an invalid type" {
        $timeSpan = New-TimeSpan -Seconds 10

        { $sensor | Set-ObjectProperty InheritAccess $timeSpan } | Should Throw "could not be assigned to property 'InheritAccess'. Expected type: 'System.Boolean'"
    }

    It "sets a property with an empty string" {
        $sensor | Set-ObjectProperty Name ""
    }

    It "sets a property with null on a type that allows null" {
        $sensor | Set-ObjectProperty Name $null
    }

    It "sets a property with null on a type that disallows null" {
        { $sensor | Set-ObjectProperty InheritAccess $null } | Should Throw "Null may only be assigned to properties of type string, int and double"
    }

    It "sets a nullable type with its underlying type" {
        $val = $true
        $val.GetType() | Should Be "bool"

        $sensor | Set-ObjectProperty InheritAccess $val
    }

    It "requires Value be specified" {
        { $sensor | Set-ObjectProperty Name } | Should Throw "Value parameter is mandatory"
    }

    It "setting an invalid enum value lists all valid possibilities" {

        $expected = "'test' is not a valid value for enum IntervalErrorMode. Please specify one of " +
            "'DownImmediately', 'OneWarningThenDown', 'TwoWarningsThenDown', 'ThreeWarningsThenDown', 'FourWarningsThenDown' or 'FiveWarningsThenDown'"

        { $sensor | Set-ObjectProperty IntervalErrorMode "test" } | Should Throw $expected
    }

    $intervalCases = @(
        @{value = "ThirtySeconds"; name = "static property" }
        @{value = "00:00:30"; name = "string" }
        @{value = ([TimeSpan]"00:00:30"); name = "TimeSpan" }
    )

    It "parses a ScanningInterval from a <name>" -TestCases $intervalCases {

        param($value)

        $sensor | Set-ObjectProperty Interval $value
    }

    It "sets a numeric enum" {

        $sensor | Set-ObjectProperty Priority 2
    }

    It "sets a raw property" {
        $sensor | Set-ObjectProperty -RawProperty name_ -RawValue "testName" -Force
    }

    It "sets a raw property with -Batch:`$false" {
        $sensor | Set-ObjectProperty -RawProperty name_ -RawValue "testName" -Force -Batch:$false
    }

    It "executes a supported property with -WhatIf" {
        $sensor | Set-ObjectProperty InheritAccess $true -WhatIf
    }

    It "executes a raw property with -WhatIf" {
        $sensor | Set-ObjectProperty -RawProperty name_ -RawValue "testName" -Force -WhatIf
    }

    It "executes a raw property with ShouldContinue" {
        try
        {
            $sensor | Set-ObjectProperty -RawProperty name_ -RawValue "testName"
        }
        catch
        {
        }
    }

    It "executes with -Batch:`$true" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse "editsettings?id=4000,4001&interval_=300%7c5+minutes&intervalgroup=0&"

        $sensors | Set-ObjectProperty Interval 00:05:00 -Batch:$true
    }

    It "sets raw properties on multiple objects with -Batch:`$true" {

        SetMultiTypeResponse

        $sensors= Get-Sensor -Count 2

        SetAddressValidatorResponse "editsettings?id=4000,4001&interval_=300%7c5+minutes&"

        $sensors | Set-ObjectProperty -RawProperty "interval_" -RawValue "300|5 minutes"
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            "editsettings?id=4000&interval_=300%7c5+minutes&intervalgroup=0&"
            "editsettings?id=4001&interval_=300%7c5+minutes&intervalgroup=0&"
        )

        $sensors | Set-ObjectProperty Interval 00:05:00 -Batch:$false
    }

    It "passes through with -Batch:`$false" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Set-ObjectProperty name blah -PassThru -Batch:$false

        $newSensor | Should Be $sensor
    }

    It "passes through with -Batch:`$true" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Set-ObjectProperty name blah -PassThru -Batch:$true

        $newSensor | Should Be $sensor
    }
}