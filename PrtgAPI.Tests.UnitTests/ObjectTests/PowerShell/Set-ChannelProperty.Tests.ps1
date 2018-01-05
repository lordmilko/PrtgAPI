. $PSScriptRoot\Support\Standalone.ps1

Describe "Set-ChannelProperty" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    $channel = Get-Sensor | Get-Channel

    It "sets a property with a valid type" {
        $channel | Set-ChannelProperty ErrorLimitMessage "oh no!"
    }

    It "sets a property with an invalid type" {
        $timeSpan = New-TimeSpan -Seconds 10

        { $channel | Set-ChannelProperty LimitsEnabled $timeSpan } | Should Throw "Expected type: 'System.Boolean'. Actual type: 'System.TimeSpan'"
    }

    It "sets a property with an empty string" {
        $channel | Set-ChannelProperty LowerErrorLimit ""
    }

    It "sets a property with null on a type that allows null" {
        $channel | Set-ChannelProperty LowerErrorLimit $null
    }

    It "sets a property with null on a type that disallows null" {
        { $channel | Set-ChannelProperty ColorMode $null } | Should Throw "Null may only be assigned to properties of type string, int and double"
    }

    It "sets a nullable type with its underlying type" {
        $val = $true
        $val.GetType() | Should Be "bool"

        $channel | Set-ChannelProperty LimitsEnabled $val
    }

    It "requires Value be specified" {
        { $channel | Set-ChannelProperty UpperErrorLimit } | Should Throw "Value parameter is mandatory"
    }
    
    It "sets a property using the manual parameter set" {
        Set-ChannelProperty -SensorId 1001 -ChannelId 1 LimitsEnabled $true
    }

    It "setting an invalid enum value lists all valid possibilities" {

        $expected = "'banana' is not a valid value for enum AutoMode. Please specify one of 'Automatic' or 'Manual'"

        { $channel | Set-ChannelProperty ColorMode "banana" } | Should Throw $expected
    }

    It "executes with -Batch:`$true" {

        $channel.Count | Should Be 2

        SetAddressValidatorResponse "editsettings?id=4000,4001&limiterrormsg_1=oh+no!&limitmode_1=1&"

        $channel | Set-ChannelProperty ErrorLimitMessage "oh no!" -Batch:$true
    }

    It "executes with -Batch:`$false" {

        $channel.Count | Should Be 2

        SetAddressValidatorResponse @(
            "editsettings?id=4000&limiterrormsg_1=oh+no!&limitmode_1=1&"
            "editsettings?id=4001&limiterrormsg_1=oh+no!&limitmode_1=1&"
        )

        $channel | Set-ChannelProperty ErrorLimitMessage "oh no!" -Batch:$false
    }
}