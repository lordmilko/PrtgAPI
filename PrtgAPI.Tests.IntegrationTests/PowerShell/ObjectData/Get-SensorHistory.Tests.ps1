. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-SensorHistory_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "retrieves records for the previous hour" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $history = $sensor | Get-SensorHistory

        $first = $history | select -First 1
        $last = $history | select -Last 1

        $seconds = [int]($first.DateTime - $last.DateTime).TotalSeconds

        $seconds | Should BeGreaterThan (60 * 57)
        $seconds | Should BeLessThan (60 * 60 + 1)
    }

    It "retrieves records for a specified time frame" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $start = (Get-Date).AddDays(-1)
        $end = (Get-Date).AddDays(-1).AddHours(-1)

        $history = $sensor | Get-SensorHistory -StartDate $start -EndDate $end

        $history | Assert-NotNull -Message "Could not retrieve any historical records within the specified time frame"

        $first = ($history | select -First 1).DateTime
        $last = ($history | select -Last 1).DateTime

        ($first - $start).TotalSeconds | Should BeLessThan 60
        ($end - $last).TotalSeconds | Should BeLessThan 60
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

        $channel = $sensor | Get-Channel | where DisplayLastValue -EQ "Denied" | select -First 1

        $channel | Should Not BeNullOrEmpty

        $history = $sensor | Get-SensorHistory

        $column = $history | select -expand $channel.Name.Replace(" ","")

        $lookup = $column | where { $_ -eq "Denied" } | select -First 1

        $lookup | Should Be "Denied"
    }

    It "includes all channels when processing traffic sensors" {

        $sensor = Get-Sensor -Tags wmiband* -Count 1

        $sensor | Should Not BeNullOrEmpty

        $history = $sensor | Get-SensorHistory

        $history."Total(volume)" | Should Not BeNullOrEmpty
        $history."Total(speed)" | Should Not BeNullOrEmpty
    }

    It "processes all sensors" {
        Get-Sensor | Get-SensorHistory
    }

    Context "Average" {
        It "retrieves the raw average" {
            $sensor = Get-Sensor -Id (Settings UpSensor)
        
            $history = $sensor | Get-SensorHistory

            $firstDiff = $null
            $secondDiff = $null

            for($i = 0; $i -lt $history.Length - 2; $i++)
            {
                $first = $history[$i].DateTime
                $second = $history[$i+1].DateTime
                $third = $history[$i+2].DateTime

                $firstDiff = [int]($first - $second).TotalSeconds
                $secondDiff = [int]($second - $third).TotalSeconds

                if($firstDiff -ne 30 -or $secondDiff -ne 30)
                {
                    continue
                }
                else
                {
                    return
                }
            }

            $firstDiff | Should Be 30
            $secondDiff | Should Be 30
        }

        It "uses a specific average" {
            $sensor = Get-Sensor -Id (Settings UpSensor)

            $history = $sensor | Get-SensorHistory -Average 300
        
            $first = $history[0].DateTime
            $second = $history[1].DateTime
            $third = $history[2].DateTime

            [int]($first - $second).TotalSeconds | Should Be 300
            [int]($second - $third).TotalSeconds | Should Be 300
        }

        It "uses a non-standard average" {
            $sensor = Get-Sensor -Id (Settings UpSensor)

            $history = $sensor | Get-SensorHistory -Average 320
        
            $first = $history[0].DateTime
            $second = $history[1].DateTime
            $third = $history[2].DateTime

            [int]($first - $second).TotalSeconds | Should Be 320
            [int]($second - $third).TotalSeconds | Should Be 320
        }

    }

    Context "Count" {
        It "retrieves the specified number of records when a count is specified" {

            $sensor = Get-Sensor -Id (Settings UpSensor)

            $history = $sensor | Get-SensorHistory -Count 10

            $history.Count | Should Be 10
        }

        It "retrieves the specified number of records when an end date and a count is specified" {

            $sensor = Get-Sensor -Id (Settings UpSensor)

            $history = $sensor | Get-SensorHistory -EndDate (Get-Date).AddDays(-1) -Count 600

            $history.Count | Should Be 600
        }

        It "streams the specified number of records when a large count larger than one page is specified" {

            $history = Get-SensorHistory -Id (Settings UpSensor) -EndDate (Get-Date).AddDays(-7) -Count 620

            $history.Count | Should Be 620
        }

        It "streams the specified number of records when a large count smaller than one page is specified" {
            $history = Get-SensorHistory -Id (Settings UpSensor) -EndDate (Get-Date).AddDays(-7) -Count 300

            $history.Count | Should Be 300
        }

        It "adjusts the missing end date when a large count is specified" {

            $sensor = Get-Sensor -Id (Settings UpSensor)

            $history = $sensor | Get-SensorHistory -Count 240

            $history.Count | Should Be 240
        }
        
        It "retrieves the specified number of reocords when a small count is specified" {

            $sensor = Get-Sensor -Id (Settings UpSensor)

            $history = $sensor | Get-SensorHistory -Count 40

            $history.Count | Should Be 40
        }

        It "doesn't adjust the specified end date when a large count is specified" {
            
            $start = (Get-Date)
            $end = $start.AddHours(-1)
            
            $sensor = Get-Sensor -Id (Settings UpSensor)

            $sensor.Interval | Should Be "00:00:30"

            $history = $sensor | Get-SensorHistory -Count 240 -StartDate $start -EndDate $end

            $history.Count | Should BeLessThan 180
            $history.Count | Should BeGreaterThan 110
        }
        
        It "adjusts the end date based on an interval of 24 hours" {

            $sensor = Get-Sensor -Id (Settings UpSensor)

            $originalInterval = $sensor.Interval

            try
            {
                $sensor.Interval | Should Not Be "1.00:00:00"

                $sensor | Set-ObjectProperty Interval "1.00:00:00"

                $sensor = Get-Sensor -Id (Settings UpSensor)

                $sensor.Interval | Should Be "1.00:00:00"

                $history = $sensor | Get-SensorHistory -Count 20

                $history.Count | Should Be 20
            }
            finally
            {
                $sensor | Set-ObjectProperty Interval $originalInterval
            }
        }
        
        It "adjusts the missing end date when a sensor ID is specified" {

            $history = Get-SensorHistory -Id (Settings UpSensor) -Count 70

            $history.Count | Should Be 70
        }
        
        It "adjusts the missing end date when an average is specified" {

            $sensor = Get-Sensor -Id (Settings UpSensor)

            $history = $sensor | Get-SensorHistory -Count 80 -Average 300

            $history.Count | Should Be 80
        }

        It "doesn't resolve the sensor ID when an average is specified" {

            $history = Get-SensorHistory -Id (Settings UpSensor) -Count 70 -Average 300

            $history.Count | Should Be 70
        }
    }

    Context "Downtime" {
        It "doesn't include downtime by default" {
            $record = Get-Sensor -Id (Settings UpSensor) | Get-SensorHistory | Select -First 1

            $record | Should Not BeNullOrEmpty

            $downtime = $record.PSObject.Properties | where Name -EQ "Downtime"

            $downtime | Should BeNullOrEmpty
        }

        It "includes downtime when -Downtime is specified" {
            $record = Get-Sensor -Id (Settings UpSensor) | Get-SensorHistory -Downtime | Select -First 1

            $record | Should Not BeNullOrEmpty

            $downtime = $record.PSObject.Properties | where Name -EQ "Downtime"

            $downtime | Should Not BeNullOrEmpty
            $downtime.Name | Should Be "Downtime"
        }

        It "includes downtime when an average is specified" {
            $record = Get-Sensor -Id (Settings UpSensor) | Get-SensorHistory -Average 60 | Select -First 1

            $record | Should Not BeNullOrEmpty

            $downtime = $record.PSObject.Properties | where Name -EQ "Downtime"

            $downtime | Should Not BeNullOrEmpty
            $downtime.Name | Should Be "Downtime"
        }

        It "throws specifying -Downtime with an average of 0" {
            $sensor = Get-Sensor -Id (Settings UpSensor)

            { $sensor | Get-SensorHistory -Average 0 -Downtime } | Should Throw "Cannot retrieve downtime with an Average of 0"
        }

        It "retrieves as a readonly user" {
            ReadOnlyClient {
                $sensor = Get-Sensor -Id (Settings UpSensor)

                $sensor | Get-SensorHistory
            }
        }
    }
}