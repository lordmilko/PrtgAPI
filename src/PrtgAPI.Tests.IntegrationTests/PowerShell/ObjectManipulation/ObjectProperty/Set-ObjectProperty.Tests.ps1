. $PSScriptRoot\..\..\..\Support\PowerShell\ObjectProperty.ps1

function TestScanningInterval($expectedString, $value)
{
    $sensor = Get-Sensor -Id (Settings UpSensor)

    $initialInterval = $($sensor | Get-ObjectProperty).Interval
    $initialInterval | Should Not Be $expectedString

    $sensor | Set-ObjectProperty interval $value

    $newInterval = $($sensor | Get-ObjectProperty).Interval

    $newInterval | Should Be $expectedString
}

Describe "Set-ObjectProperty_IT" -Tag @("PowerShell", "IntegrationTest") {
    
    Context "TableSettings" {
        $testCases = @(
            @{name = "Sensors"; obj = { Get-Sensor -Id (Settings UpSensor) }}
            @{name = "Devices"; obj = { Get-Device -Id (Settings Device) }}
            @{name = "Groups";  obj = { Get-Group  -Id (Settings Group)  }}
            @{name = "Probes";  obj = { Get-Probe  -Id (Settings Probe)  }}
        )
        
        It "Scanning Interval" -TestCases $testCases {
            param($name, $obj)

            $object = (& $obj)

            try
            {
                if($name -ne "Probes") {
                    if($name -ne "Sensors") {
                        $object | Set-ObjectProperty InheritInterval $false
                    }

                    SetValue "InheritInterval"   $true
                    $object | Set-ObjectProperty InheritInterval $true
                }

                SetChild "Interval"          "00:05:00"         "InheritInterval" $false
                SetChild "IntervalErrorMode" TwoWarningsThenDown "InheritInterval" $false
            }
            finally
            {
                $object | Set-ObjectProperty InheritInterval $false
            }
        }
        
        It "Schedules, Dependencies and Maintenance Window" -TestCases $testCases {
            param($name, $obj)

            $object = (& $obj)

            if($name -eq "Sensors") {
                $object = Get-Sensor -Id (Settings PausedByDependencySensor)
            }

            $dependentId = (Settings DownAcknowledgedSensor)
            $dependencyType = "Object"
            $dependencyDelay = 0

            switch($name)
            {
                "Devices" {
                    $dependentId = ($object | Get-Sensor Ping).Id
                    $dependencyDelay = 60
                }

                "Groups" {
                    $dependencyType = "Parent"
                    $dependentId = 0
                }

                "Probes" {
                    $dependencyType = "Parent"
                    $dependentId = 0
                }
            }

            #$schedule = Get-PrtgSchedule -Id (Settings Schedule)
            
            GetValue "Schedule"           "None"
            GetValue "MaintenanceEnabled" $false
            GetValue "MaintenanceStart"   (Settings MaintenanceStart)
            GetValue "MaintenanceEnd"     (Settings MaintenanceStart)
            GetValue "DependencyType"     $dependencyType
            GetValue "DependentObjectId"  $dependentId
            GetValue "DependencyDelay"    $dependencyDelay

            #todo: group and probe need these flipped
            #SetValue "InheritDependency"  $true $true
            #SetChild "Schedule"           $schedule "InheritDependency" $false
            
            #
            #SetChild "MaintenanceEnabled" $true
            #SetGrandChild MaintenanceStart
            #SetGrandChild MaintenanceEnd
            #SetChild DependencyType Object #todo: will this not work if i havent also specified the object to use at the same time?
            #should we maybe create a dependency attribute between the two? and would the same be true vice versa? (so when you set it to master,
            #the dependencyvalue goes away? check how its meant to work with fiddler)
            #SetChild Dependency (Settings DownSensor)
            #SetChild DependencyDelay 3
        }
        
        It "Proxy Settings for HTTP Sensors" -TestCases $testCases {
            param($name, $obj)

            $object = (& $obj)

            if($name -ne "Probes") {
                SetValue      "InheritProxy"  $false
            }
            
            SetChild      "ProxyAddress"  "https://proxy.example.com"      "InheritProxy" $false
            SetChild      "ProxyPort"     "3128"                           "InheritProxy" $false
            SetChild      "ProxyUser"     "newUser"                        "InheritProxy" $false
            SetWriteChild "ProxyPassword" "newPassword" "HasProxyPassword" "InheritProxy" $false
        }

        It "Channel Unit Configuration" -TestCases $testCases {
            param($name, $obj)

            $object = (& $obj)
            
            if($name -eq "Sensors") {
                $object = Get-Sensor -Tags wmiband* | Select -First 1
            }

            SetValue "InheritChannelUnit" $false
            SetChild "BandwidthVolumeUnit" "TByte" "InheritChannelUnit" $false
            SetChild "BandwidthSpeedUnit"  "Tbit"  "InheritChannelUnit" $false
            SetChild "BandwidthTimeUnit"   "Day"   "InheritChannelUnit" $false

            if($name -eq "Sensors") {
                $object = Get-Sensor -Id (Settings DownSensor)
            }
            SetChild "MemoryUsageUnit"     "TByte" "InheritChannelUnit" $false

            if($name -eq "Sensors") {
                $object = Get-Sensor -Id (Settings PausedSensor)
            }
            SetChild "DiskSizeUnit"        "TByte" "InheritChannelUnit" $false

            if($name -ne "Sensors") {
                SetChild "FileSizeUnit"    "TByte" "InheritChannelUnit" $false
            }
        }
    }
    
    It "sets a notification action property" {

        $action = Get-NotificationAction -Id (Settings NotificationAction)

        $action.Active | Should Be $true

        $action | Set-ObjectProperty Active $false

        $newAction = Get-NotificationAction -Id (Settings NotificationAction)

        $newAction.Active | Should Be $false

        $newAction | Set-ObjectProperty Active $true

        $finalAction = Get-NotificationAction -Id (Settings NotificationAction)

        $finalAction.Active | Should Be $true
    }

    It "sets a schedule property" {
        $schedule = Get-PrtgSchedule -Id (Settings Schedule)
        $newName = "New Schedule"

        $schedule.Name | Should Not Be $newName        

        $schedule | Set-ObjectProperty Name $newName
        $newSchedule = Get-PrtgSchedule -Id (Settings Schedule)
        $newSchedule.Name | Should Be $newName

        $newSchedule | Set-ObjectProperty Name $schedule.Name

        $finalSchedule = Get-PrtgSchedule -Id (Settings Schedule)
        $finalSchedule.Name | Should Be $schedule.Name
    }

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

        $message = "Sensor Name: Required field, not defined. The object has not been changed"

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
        $upSensor.Interval | Should Not Be "00:10:00"

        $device = Get-Device -Id (Settings Device)
        $device.Interval | Should Not Be "00:10:00"

        $objects = $upSensor,$device

        $objects | Set-ObjectProperty Interval "00:10:00"

        LogTestDetail "Sleeping for 10 seconds while settings apply"
        Sleep 10

        $newUpSensor = Get-Sensor -Id (Settings UpSensor)
        $newUpSensor.Interval | Should Be "00:10:00"

        $newDevice = Get-Device -Id (Settings Device)
        $newDevice.Interval | Should Be "00:10:00"
    }

    It "sets multiple with dynamic parameters" {
        $device = Get-Device -Id (Settings Device)

        $props = $device | Get-ObjectProperty

        $props.VMwareUserName | Should Be $null
        $props.HasVMwarePassword | Should Be $false

        $device | Set-ObjectProperty -VMwareUserName root -VMwarePassword test

        $newProps = $device | Get-ObjectProperty

        $newProps.VMwareUserName | Should Be "root"
        $newProps.HasVMwarePassword | Should Be $true
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

    It "overrides a dependent property" {
        $object = Get-Sensor -Id (Settings UpSensor)

        $object | Set-ObjectProperty Interval 00:01:00

        $properties = $object | Get-ObjectProperty
        $properties.InheritInterval | Should Be $false
        $properties.Interval | Should Be "00:01:00"

        $object | Set-ObjectProperty -Interval 00:00:30 -InheritInterval $true

        $newProperties = $object | Get-ObjectProperty
        $newProperties.InheritInterval | Should Be $true
        $newProperties.Interval | Should Be "00:00:30"
    }
}