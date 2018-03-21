. $PSScriptRoot\Support\Standalone.ps1

function SetValue($params, $property, $value)
{
    $initial = $params.$property

    $params.$property = $value

    $new = $params.$property

    $new | Should Not Be $initial
    $new | Should Be $value
}

function ValidateParams($params, $address)
{
    $device = Run Device { Get-Device }

    WithResponseArgs "AddressValidatorResponse" $address {
        $device | Add-Sensor $params -Resolve:$false
    }
}

Describe "New-SensorParameters" {

    $device = Run Device { Get-Device }

    It "can create parameters with a name" {
        $params = New-SensorParameters ExeXml "custom name"

        $params.GetType().Name | Should Be "ExeXmlSensorParameters"
        $params.Name | Should Be "custom name"
    }

    It "can use a default name" {
        $params = New-SensorParameters ExeXml

        $params.GetType().Name | Should Be "ExeXmlSensorParameters"
        $params.Name | Should Be "XML Custom EXE/Script Sensor"
    }

    It "can specify a mandatory value" {
        $params = New-SensorParameters ExeXml "custom name" "blah.ps1"

        $params.GetType().Name | Should Be "ExeXmlSensorParameters"
        $params.Name | Should Be "custom name"
        $params.ExeFile | Should Be "blah.ps1"
    }

    Context "Raw Parameters" {
        It "can use raw parameters" {        
            $raw =@{
                "name_" = "custom name"
                "sensortype" = "custom type"
            }
        
            $params = New-SensorParameters $raw

            $params.GetType().Name | Should Be "RawSensorParameters"
            $params.Name | Should Be "custom name"
            $params.SensorType | Should Be "custom type"
        }

        It "throws when a raw name isn't specified" {
            { New-SensorParameters @{"sensortype" = "custom type"} } | Should Throw "'name_' is mandatory"
        }

        It "throws when a raw name is null" {
            { New-SensorParameters @{"name_" = $null; "sensortype" = "custom type"} } | Should Throw "objectName cannot be null or empty"
        }

        It "throws when a raw sensortype isn't specified" {
            { New-SensorParameters @{"name_" = "custom name"} } | Should Throw "'sensortype' is mandatory"
        }

        It "throws when a raw sensortype is null" {
            { New-SensorParameters @{"name_" = "custom name"; "sensortype" = $null} } | Should Throw "sensorType cannot be null or empty"
        }
    }

    Context "Empty Parameters" {

        It "can use empty parameters" {

            $params = New-SensorParameters -Empty

            $params["name_"] = "My Sensor"
            $params["sensortype"] = "customtype"
            $params["customfield"] = "somevalue"
            $params["anotherfield_"] = "3"

            ValidateParams $params "name_=My+Sensor&customfield=somevalue&anotherfield_=3&sensortype=customtype"
        }

        #region Get

        It "gets a CLR property via its raw name" {
            $params = New-SensorParameters -Empty

            $params["name_"] | Should BeNullOrEmpty
        }

        It "gets a raw property via its CLR name" {
            $params = New-SensorParameters -Empty

            $params.Name | Should BeNullOrEmpty
            $params.PSExtended.name_ | Should BeNullOrEmpty
        }

        It "gets a real parameter CLR property via its raw name" {
            $params = New-SensorParameters -Empty

            $params["sensortype"] | Should BeNullOrEmpty
            $params.PSExtended.sensortype | Should BeNullOrEmpty
        }

        It "gets a CLR property via its raw name and ignores case" {
            $params = New-SensorParameters -Empty

            $params["NAME_"] | Should BeNullOrEmpty
            $params.PSExtended.name_ | Should BeNullOrEmpty
        }

        it "gets a real parameter CLR property via its raw name and ignores case" {
            $params = New-SensorParameters -Empty

            $params["SENSORTYPE"] | Should BeNullOrEmpty
            $params.PSExtended.sensortype | Should BeNullOrEmpty
        }

        It "throws getting when a parameter doesn't exist" {
            $params = New-SensorParameters -Empty

            $val1 = $params["test_"]

            WithStrict {
                { $val2 = $params["test_"] } | Should Throw "Parameter with name 'test_' does not exist"
            }
        }

        #endregion
        #region Set

        It "sets a CLR property via its raw name" {
            $params = New-SensorParameters -Empty

            $params["name_"] = "My Sensor"
            $params.Name | Should Be "My Sensor"

            $params.PSExtended.name_ | Should BeNullOrEmpty
        }

        It "sets a raw property via its CLR name" {
            $params = New-SensorParameters -Empty

            $params.Name = "My Sensor"
            $params.Name | Should Be "My Sensor"
            $params["name_"] | Should Be "My Sensor"

            $params.PSExtended.name_ | Should BeNullOrEmpty
        }

        It "sets a real parameter CLR property via its raw name" {
            $params = New-SensorParameters -Empty

            $params["sensortype"] = "customtype"
            $params.SensorType | Should Be "customtype"

            $params.PSExtended.sensortype | Should BeNullOrEmpty
        }

        It "sets a CLR property via its raw name and ignores case" {
            $params = New-SensorParameters -Empty

            $params["NAME_"] = "My Sensor"
            $params.Name | Should Be "My Sensor"

            $params.PSExtended.name_ | Should BeNullOrEmpty
        }

        it "sets a real parameter CLR property via its raw name and ignores case" {
            $params = New-SensorParameters -Empty

            $params["SENSORTYPE"] = "customtype"
            $params.SensorType | Should Be "customtype"

            $params.PSExtended.sensortype | Should BeNullOrEmpty
        }

        It "sets an existing custom property via its raw name and ignores case" {
            $params = New-SensorParameters -Empty

            $params["test_"] = "hello"
            $params["TEST_"] = "goodbye"

            $params["test_"] | Should Be "goodbye"

            $params.Parameters.Count | Should Be 1

            $params.Name = "testName"
            $params.SensorType = "testType"
            ValidateParams $params "test_=goodbye&name_=testName&sensortype=testType&id=40"
        }

        #endregion

        It "throws adding when empty parameters dont have a name" {
            $params = New-SensorParameters -Empty

            $params["sensortype"] = "customtype"

            WithResponse "MultiTypeResponse" {
                { $device | Add-Sensor $params -Resolve:$false } | Should Throw "Property 'Name' requires a value"
            }
        }

        It "throws adding when empty parameters don't have a sensor type" {
            $params = New-SensorParameters -Empty
            $params["name_"] = "My Sensor"

            WithResponse "MultiTypeResponse" {
                { $device | Add-Sensor $params -Resolve:$false } | Should Throw "Property 'SensorType' requires a value"
            }
        }
    }

    }

    Context "ExeXmlSensorParameters" {

        It "can set a value on each property" {
            $params = New-SensorParameters ExeXml

            SetValue $params "Name" "New Sensor"
            SetValue $params "Tags" "newTag"
            SetValue $params "Tags" @("tag1", "tag2")
            SetValue $params "Priority" "Four"
            SetValue $params "ExeFile" "stuff.ps1"
            SetValue $params "ExeParameters" "arg1 arg2 arg3"
            SetValue $params "SetExeEnvironmentVariables" $true
            SetValue $params "UseWindowsAuthentication" $true
            SetValue $params "Mutex" "testMutex"
            SetValue $params "Timeout" "27"
            SetValue $params "DebugMode" "WriteToDiskWhenError"
            SetValue $params "InheritInterval" $false
            SetValue $params "Interval" "00:00:30"
            SetValue $params "IntervalErrorMode" "ThreeWarningsThenDown"
            SetValue $params "InheritTriggers" $false
        }
    }

    Context "WmiServiceParameters" {

        It "can set a value on each property" {
            SetResponseAndClient "WmiServiceTargetResponse"
            
            $services = $device | Get-SensorTarget WmiService
            $services.Count | Should BeGreaterThan 1

            $params = New-SensorParameters WmiService

            SetValue $params "Tags" "newTag"
            SetValue $params "Tags" @("tag1", "tag2")
            SetValue $params "Priority" "Four"
            SetValue $params "Services" $services
            SetValue $params "StartStopped" $true
            SetValue $params "NotifyStarted" $false
            SetValue $params "MonitorPerformance" $true
            SetValue $params "InheritTriggers" $false
        }

        It "sets services with a single service" {
            SetResponseAndClient "WmiServiceTargetResponse"
            
            $services = $device | Get-SensorTarget WmiService *prtgcore*
            $services.Count | Should Be 1

            $params = New-SensorParameters WmiService $services

            $params.Services.Count | Should Be 1
        }
    }
}