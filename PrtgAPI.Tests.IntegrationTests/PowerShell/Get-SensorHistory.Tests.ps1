. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "Get-SensorHistory_IT" {
    It "retrieves records for the previous hour" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $history = $sensor | Get-SensorHistory

        $first = $history | select -First 1
        $last = $history | select -Last 1

        $seconds = [int]($last.DateTime - $first.DateTime).TotalSeconds

        $seconds | Should BeGreaterThan (60 * 58)
        $seconds | Should BeLessThan (60 * 60 + 1)
    }

    It "retrieves records for a specified time frame" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $start = (get-date).adddays(-1).addhours(-1)
        $end = (get-date).adddays(-1)

        $history = $sensor | Get-SensorHistory -StartDate $start -EndDate $end

        $first = ($history | select -First 1).DateTime
        $last = ($history | select -Last 1).DateTime

        ($start - $first).TotalSeconds | Should BeLessThan 60
        ($last - $end) | Should BeLessThan 60
    }

    It "retrieves the raw average" {
        $sensor = Get-Sensor -Id (Settings UpSensor)
        
        $history = $sensor | Get-SensorHistory

        $first = $history[0].DateTime
        $second = $history[1].DateTime
        $third = $history[2].DateTime

        [int]($second - $first).TotalSeconds | Should Be 30
        [int]($third - $second).TotalSeconds | Should Be 30
    }

    It "uses a specific average" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $history = $sensor | Get-SensorHistory -Average 300
        
        $first = $history[0].DateTime
        $second = $history[1].DateTime
        $third = $history[2].DateTime

        [int]($second - $first).TotalSeconds | Should Be 300
        [int]($third - $second).TotalSeconds | Should Be 300
    }

    It "uses a non-standard average" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $history = $sensor | Get-SensorHistory -Average 320
        
        $first = $history[0].DateTime
        $second = $history[1].DateTime
        $third = $history[2].DateTime

        [int]($second - $first).TotalSeconds | Should Be 320
        [int]($third - $second).TotalSeconds | Should Be 320
    }

    It "uses a custom TypeName and creates custom FormatData" {
        $history = (Get-Sensor -Id (Settings UpSensor) | Get-SensorHistory)[0]

        $typeName = $history.PSObject.TypeNames[0]

        $typeName | Should BeLike "PrtgAPI.DynamicFormatPSObject*"

        $typeData = Get-FormatData $typeName

        $typeData | Should Not BeNullOrEmpty
    }

    It "uses a custom TypeName for A, a different one for B, and the same one for A again" {
        $first = (Get-Sensor -Id (Settings UpSensor) | Get-SensorHistory)[0]
        $second = (Get-Sensor -Id (Settings DownSensor) | Get-SensorHistory)[0]

        $firstTypeName = $first.PSObject.TypeNames[0]
        $secondTypeName = $second.PSObject.TypeNames[0]

        $firstTypeName | Should BeLike "PrtgAPI.DynamicFormatPSObject*"
        $secondTypeName | Should BeLike "PrtgAPI.DynamicFormatPSObject*"

        $firstTypeData = Get-FormatData $firstTypeName
        $secondTypeData = Get-FormatData $secondTypeName

        $firstTypeData | Should Not BeNullOrEmpty
        $secondTypeData | Should Not BeNullOrEmpty

        $firstAgain = (Get-Sensor -Id (Settings UpSensor) | Get-SensorHistory)[0]
        $firstAgainTypeName = $first.PSObject.TypeNames[0]

        $firstAgainTypeName | Should Be $firstTypeName

        $firstAgainTypeData = Get-FormatData $firstAgainTypeName

        $firstTypeData | Should Be $firstAgainTypeName
    }

    It "does not include unit details in property names" {
        $sensor = Get-Sensor -Id (Settings ChannelSensor)

        $history = $sensor | Get-SensorHistory

        $channel = $history | select (Settings ChannelName)

        $channel | Should Not BeNullOrEmpty
    }

    It "parses an object that uses value lookups" {

        $sensor = Get-Sensor -Id (Settings SSLSecurityCheck)

        $channel = $sensor | Get-Channel | where LastValue -EQ "Denied" | select -First 1

        $channel | Should Not BeNullOrEmpty

        $history = $sensor | Get-SensorHistory

        $column = $history | select -expand $channel.Name.Replace(" ","")

        $lookup = $column | where { $_ -eq "Denied" } | select -First 1

        $lookup | Should Be "Denied"
    }

    It "processes all sensors" {
        Get-Sensor | Get-SensorHistory
    }
}