. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Add-Sensor_IT" {

    Context "ExeXml" {
        It "adds a new sensor" {

            $values = @{
                Name = "New Sensor"
                Tags = @("new sensor")
                Priority = "Four"
                ExeFile = "blah.ps1"
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
            CheckValue "ExeFile"
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
            $properties.ExeFile | Should Be "test.ps1"
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

    Context "WmiService" {
        It "adds a new sensor" {

            $device = Get-Device -Id (Settings Device)

            $params = New-SensorParameters WmiService

            $services = $device | Get-SensorTarget WmiService *prtg*
            $services.Count | Should Be 2

            $params.Services = $services

            $sensors = $device | Add-Sensor $params

            $sensors.Count | Should Be 2

            $sensors | where Name -eq "Service: PRTG Core Server Service" | Should Not BeNullOrEmpty
            $sensors | where Name -eq "Service: PRTG Probe Service" | Should Not BeNullOrEmpty

            $sensors | Remove-Object -Force
        }
    }

    Context "HTTP" {
        It "adds a new sensor" {
            $device = Get-Device -Id (Settings Device)

            $params = New-SensorParameters Http "HTTPS" "https://"

            $newSensor = $device | Add-Sensor $params

            $newSensor.Count | Should Be 1

            $properties = $newSensor | Get-ObjectProperty

            $properties.Name | Should Be "HTTPS"
            $properties.Url | Should Be "https://"

            $newSensor | Remove-Object -Force
        }
    }

    It "adds sensor parameters piped from Get-SensorTarget" {
        $sensors = Get-Device -Id (Settings Device) | Get-SensorTarget WmiService *prtg* -Parameters | Add-Sensor

        $sensors.Count | Should Be 2

        $sensors | where Name -eq "Service: PRTG Core Server Service" | Should Not BeNullOrEmpty
        $sensors | where Name -eq "Service: PRTG Probe Service" | Should Not BeNullOrEmpty

        $sensors | Remove-Object -Force
    }
    
    It "adds a sensor constructed through empty parameters" {

        $device = Get-Device -Id (Settings Device)

        $target = $device | Get-SensorTarget -RawType exexml *test*

        $params = New-SensorParameters -Empty
        $params["name_"] = "empty sensor"
        $params["sensortype"] = "exexml"
        $params["exefile_"] = $target
        $params["interval_"] = "300|5 minutes"
        $params["timeout_"] = 70

        $params["exefile_"] | Should Be $target

        $newSensor = $device | Add-Sensor $params

        $newSensor.Count | Should Be 1

        $properties = $newSensor | Get-ObjectProperty

        $newSensor.Type | Should Be "Sensor (exexml)"
        $properties.Name | Should Be "empty sensor"
        $properties.ExeFile.ToString() | Should Be "testScript.bat"
        $properties.Interval.ToString() | Should Be ([TimeSpan]"00:05:00").ToString()
        $properties.Timeout | Should Be 70
        
        $newSensor | Remove-Object -Force
    }
    
    It "pipes dynamic parameters to Add-Sensor" {
        $sensor = Get-Device -Id (Settings Device) | New-SensorParameters -RawType http | Add-Sensor

        $sensor.Name | Should Be "HTTP"
        $sensor.Type | Should Be "Sensor (http)"

        $sensor | Remove-Object -Force
    }

    It "adds sensor targets to dynamic parameters retrieved from Where-Object" {

        $device = Get-Device -Id (Settings Device)

        $params = $device | New-SensorParameters -RawType wmiservice
        $params.service__check = $params.Targets.service__check | where name -Like *prtg*
        $params.service__check.Count | Should Be 2

        $sensors = $params | Add-Sensor

        try
        {
            $sensors.Count | Should Be 2
        }
        finally
        {
            $sensors | Remove-Object -Force
        }
    }

    It "adds a sensor using a hashtable that contain a multi parameter" {
        $table = @{
            name_ = "Base Sensor"
            interfacenumber_ = 1
            interfacenumber__check = "3:Data and Voice VLAN|(003) Data and Voice VLAN Traffic|Connected|100 MBit/s|Ethernet|1|Data and Voice VLAN|100000000|3|2|"
            trafficmode_ = "standinfornoselection","errors","discards"
            monitorstate_ = 2
            namein_ = "Traffic In"
            nameout_ = "Traffic Out"
            namesum_ = "Traffic Total"
            stack_ = 1
            sensortype = "snmptraffic"
        }

        $params = New-SensorParameters $table

        $device = Get-Device -Id (Settings Device)

        $sensor = $device | Add-Sensor $params

        $sensor.Name | Should Be "(003) Data and Voice VLAN Traffic"

        $sensor | Remove-Object -Force
    }

    It "adds a sensor from empty parameters that contain a multi parameter" {

        $params = New-SensorParameters -Empty
        $params.Name = "Base Sensor"
        $params["interfacenumber_"] = 1
        $params["interfacenumber__check"] = "3:Data and Voice VLAN|(003) Data and Voice VLAN Traffic|Connected|100 MBit/s|Ethernet|1|Data and Voice VLAN|100000000|3|2|"
        $params["trafficmode_"] = "standinfornoselection","errors","discards"
        $params["monitorstate_"] = 2
        $params["namein_"] = "Traffic In"
        $params["nameout_"] = "Traffic Out"
        $params["namesum_"] = "Traffic Total"
        $params["stack_"] = 1
        $params.SensorType = "snmptraffic"

        $device = Get-Device -Id (Settings Device)

        $sensor = $device | Add-Sensor $params

        $sensor.Name | Should Be "(003) Data and Voice VLAN Traffic"

        $sensor | Remove-Object -Force
    }
}