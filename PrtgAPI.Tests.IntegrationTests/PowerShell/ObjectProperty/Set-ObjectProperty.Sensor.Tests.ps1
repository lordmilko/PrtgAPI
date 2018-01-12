. $PSScriptRoot\..\Support\ObjectProperty.ps1

Describe "Set-ObjectProperty_Sensors_IT" {
  
    It "Basic Sensor Settings" {

        $object = Get-Sensor -Id (Settings UpSensor)

        SetValue "Name"       "TestName"
        GetValue "ParentTags" (Settings ParentTags)
        #SetValue "Tags"       "TestTag"       (Settings BLAH) #is this going to set or overwrite? we need the user to be able to indicate which one they want. maybe have an add-channelproperty?
        SetValue "Priority"   5
    }

    It "Ping Settings" {
        
        $object = Get-Device -Id (Settings Device) | Get-Sensor Ping

        SetValue "Timeout"     3
        SetValue "PingPacketSize"  33
        SetValue "PingMode"        "SinglePing"
        SetValue "PingCount"       6
        SetValue "PingDelay"       6
        SetValue "AutoAcknowledge" $true
    }

    It "Sensor Display" {
        #if we're going to use Channel as the type for PrimaryChannel, we need to be able to handle the case where the primary channel is downtime

        $object = Get-Sensor -Id (Settings UpSensor)

        #SetValue "PrimaryChannel" BLAH
        SetValue "GraphType" "Stacked"
        #SetValue "StackUnit" BLAH #how do we get a list of valid units? what happens if you set an invalid one? maybe do channel.unit?
    }

    It "Scanning Interval" {

        $object = Get-Sensor -Id (Settings UpSensor)

        SetValue "InheritInterval"   $true

        try
        {
            $object | Set-ObjectProperty InheritInterval $true

            SetChild "Interval"          "00:01:00"         "InheritInterval" $false
            SetChild "IntervalErrorMode" OneWarningThenDown "InheritInterval" $false
        }
        finally
        {
            $object | Set-ObjectProperty InheritInterval $false
        }
    }

    It "Schedules, Dependencies and Maintenance Window" {
        $object = Get-Sensor -Id (Settings PausedByDependencySensor)

        GetValue "Schedule"           "None"
        GetValue "MaintenanceEnabled" $false
        GetValue "MaintenanceStart"   (Settings MaintenanceStart)
        GetValue "MaintenanceEnd"     (Settings MaintenanceStart)
        GetValue "DependencyType"     "Object"
        GetValue "DependentObjectId"  (Settings DownAcknowledgedSensor)
        GetValue "DependencyDelay"    0
        #SetValue "InheritDependency"  $true
        #SetChild "Schedule" BLAH #todo - get rid of the getchild "schedule"
        #SetChild "MaintenanceEnabled" $true
        #SetGrandChild MaintenanceStart
        #SetGrandChild MaintenanceEnd
        #SetChild DependencyType Object #todo: will this not work if i havent also specified the object to use at the same time?
        #should we maybe create a dependency attribute between the two? and would the same be true vice versa? (so when you set it to master,
        #the dependencyvalue goes away? check how its meant to work with fiddler)
        #SetChild Dependency (Settings DownSensor)
        #SetChild DependencyDelay 3
    }

    It "Access Rights" {
        $object = Get-Sensor -Id (Settings UpSensor)

        SetValue "InheritAccess" $false
        #todo: actually modifying access rights
    }

    It "Debug Options" {
        $object = Get-Sensor -Id (Settings WarningSensor)

        SetValue "DebugMode" "WriteToDisk"
    }

    It "WMI Alternative Query" {
        $object = Get-Sensor -Id (Settings WarningSensor)

        SetValue "WmiMode" "Alternative"
    }

    It "WMI Remote Ping Configuration" {
        $object = Get-Sensor -Id (Settings WmiRemotePing)

        SetValue "Target" "8.8.8.8"
        SetValue "Timeout" "200"
        SetValue "PingRemotePacketSize" "1000"
    }

    It "HTTP Specific" {
        $object = Get-Sensor -Id (Settings UpSensor)

        SetValue "Timeout" "500"
        SetValue "Url" "https://"
        SetValue "HttpRequestMethod" "POST"
        #GetValue "SNI" "blah"
        SetValue "UseSNIFromUrl" $true
    }

    It "Sensor Settings (EXE/XML)" {
        $object = Get-Sensor -Id (Settings ExeXml)

        SetValue "ExeFile" "blah.ps1"
        SetValue "ExeParameters" "test parameters `"with test quotes`""
        SetValue "SetExeEnvironmentVariables" $true
        SetValue "UseWindowsAuthentication" $true
        SetValue "Mutex" "Mutex1"
        SetValue "EnableChangeTriggers" $true
        #GetValue "ExeValueType" #todo: need to test that setting readonly on exevaluetype still retrieves the value
        SetValue "DebugMode" "WriteToDiskWhenError"
    }
    
    It "Sensor Factory Specific Settings" {
        $object = Get-Sensor -Id (Settings SensorFactory)

        SetValue "ChannelDefinition" "#1:First Channel`nchannel(1001,0)"
        SetValue "ChannelDefinition" @(
            "#1:First Array Channel"
            "channel(1002,1)"
            "#2:Second Array Channel"
            "channel(1002,1)"
        )

        SetChild "FactoryErrorFormula" "status(1001) AND status (1002)" "FactoryErrorMode" "CustomFormula"
        SetValue "FactoryErrorMode" "WarnOnError"
        SetValue "FactoryMissingDataMode" "CalculateWithZero"
    }
    
    It "WMI Service Monitor" {
        $object = Get-Sensor -Id (Settings WmiService)

        SetValue "StartStopped" $true

        SetChild "NotifyStarted" $false "StartStopped" $true

        SetValue "MonitorPerformance" $true
        SetValue "DebugMode" "WriteToDisk"
    }

    It "sets a sensor factory definition using New-SensorFactoryDefinition" {
        $sensors = Get-Sensor -Tags wmimem*

        $definition = $sensors | New-SensorFactoryDefinition { $_.Name } 0

        $object = Get-Sensor -Id (Settings SensorFactory)

        $object | Set-ObjectProperty ChannelDefinition $definition

        $source = $object | Get-SensorFactorySource

        $source.Id | Should Be $sensors.Id
    }
    
    Context "Other" {
        It "Triggers" {
            $object = Get-Sensor -Id (Settings UpSensor)

            $initialValue = $object.NotificationTypes.InheritTriggers

            $object | Set-ObjectProperty InheritTriggers $false

            $newObject = Get-Sensor -Id (Settings UpSensor)

            $newValue = $newObject.NotificationTypes.InheritTriggers

            $newValue | Should Not Be $initialValue

            $newValue | Should Be $false
        }
    }
}