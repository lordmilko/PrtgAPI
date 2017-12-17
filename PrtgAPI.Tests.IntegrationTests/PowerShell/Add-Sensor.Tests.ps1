. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Add-Sensor_IT" {

    Context "ExeXml" {
        It "adds a new sensor" {

            $values = @{
                Name = "New Sensor"
                Tags = @("new sensor")
                Priority = "Four"
                ExeName = "blah.ps1"
                ExeParameters = "arg1 arg2 arg 3"
                SetExeEnvironmentVariables = $true
                UseWindowsAuthentication = $true
                Mutex = "textMutex"
                Timeout = 27
                DebugMode = "WriteToDiskWhenError"
                InheritInterval = $false
                Interval = "00:00:30"
                IntervalErrorMode = "ThreeWarningsThenDown"
                InheritTriggers = $false
            }

            $device = Get-Device -Id (Settings Device)
            $params = New-SensorParameters ExeXml

            function SetValue($name)
            {
                $params.$name = $values.$name
            }

            foreach($key in $values.Keys)
            {
                SetValue $key
            }

            $sensors = $device | Get-Sensor

            $device | Add-Sensor $params

            $newSensors = $device | Get-Sensor

            $newSensors.Count | Should BeGreaterThan $sensors.Count

            $newSensor = $newSensors | where name -EQ $values.Name
            $newSensor.Count | Should Be 1

            $properties = $newSensor | Get-ObjectProperty

            function CheckValue($name)
            {
                $properties.$name | Assert-Equal $values.$name -Message "Expected $name to be $($values.$name) but was <actual> instead"
            }

            CheckValue "Name"
            $properties.Tags -join " " | Should Be  $values.Tags
            CheckValue "Priority"
            CheckValue "ExeName"
            CheckValue "ExeParameters"
            CheckValue "SetExeEnvironmentVariables"
            CheckValue "UseWindowsAuthentication"
            CheckValue "Mutex"
            CheckValue "Timeout"
            CheckValue "DebugMode"
            CheckValue "InheritInterval"
            CheckValue "Interval"
            CheckValue "IntervalErrorMode"

            $newSensor.NotificationTypes.InheritTriggers | Should Be $false
        }

        It "adds a new sensor using raw parameters" {
            $table = @{
                "name_" = "my raw sensor"
                "tags_" = "xmlexesensor"
                "priority_" = 4
                "exefile_" = "test.ps1|test.ps1||"
                "exeparams_" = "arg1 arg2 arg3"
                "environment_" = 1
                "usewindowsauthentication_" = 1
                "mutexname_" = "testMutex"
                "timeout_" = 70
                "writeresult_" = 1
                "intervalgroup" = 0
                "interval_" = "30|30 seconds"
                "errorintervalsdown_" = 2
                "sensortype" = "exexml"
                "inherittriggers_" = 0
            }

            $params = New-SensorParameters $table

            $device = Get-Device -Id (Settings Device)
            $sensors = $device | Get-Sensor

            $device | Add-Sensor $params

            $newSensors = $device | Get-Sensor

            $newSensors.Count | Should BeGreaterThan $sensors.Count

            $newSensor = $newSensors | where name -EQ $table."name_"
            $newSensor.Count | Should Be 1

            $properties = $newSensor | Get-ObjectProperty

            $properties.Name | Should Be "my raw sensor"
            $properties.Tags | Should Be "xmlexesensor"
            $properties.Priority | Should Be "Four"
            $properties.ExeName | Should Be "test.ps1"
            $properties.ExeParameters | Should Be "arg1 arg2 arg3"
            $properties.SetExeEnvironmentVariables | Should Be $true
            $properties.UseWindowsAuthentication | Should Be $true
            $properties.Mutex | Should Be "testMutex"
            $properties.Timeout | Should Be 70
            $properties.DebugMode | Should Be "WriteToDisk"
            $properties.InheritInterval | Should Be $false
            $properties.Interval | Should Be "00:00:30"
            $properties.IntervalErrorMode | Should Be "TwoWarningsThenDown"
            $newSensor.NotificationTypes.InheritTriggers | Should Be $false
        }

        It "resolves a new sensor" {
            $params = New-SensorParameters ExeXml resolveSensor test.ps1

            $device = Get-Device -Id (Settings Device)
            $originalSensors = Get-Sensor

            $newSensor = $device | Add-Sensor $params

            $newSensors = Get-Sensor

            $newSensors.Count | Should BeGreaterThan $originalSensors.Count

            $diffSensor = $newSensors|where name -EQ $params.Name

            $diffSensor.Id | Should Be $newSensor.Id

            $newSensor | Remove-Object -Force
        }
    }
}