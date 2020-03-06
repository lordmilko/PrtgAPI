. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

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

    $urls = @(
        @(
            [Request]::Status()
            [Request]::BeginAddSensorQuery($device.Id, $params["sensortype"])
            [Request]::AddSensor($address + "&id=$($device.Id)")
        ),
        $true
    )

    WithResponseArgs "AddressValidatorResponse" $urls {
        $device | Add-Sensor $params -Resolve:$false
    }
}

Describe "New-SensorParameters" -Tag @("PowerShell", "UnitTest") {

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

    It "has contexts for all sensor parameter types" {

        $contextNames = GetScriptContexts $PSCommandPath

        $baseType = [PrtgAPI.Parameters.SensorParametersInternal]

        $allTypes = $baseType.Assembly.GetTypes()

        $sensorParametersTypes = $allTypes | where { $baseType.IsAssignableFrom($_) -and $_ -ne $baseType } | select -ExpandProperty Name

        $missingContexts = $sensorParametersTypes | where { $_ -notin $contextNames }

        if($missingContexts)
        {
            $str = $missingContexts -join "`n"

            throw "Contexts/tests are missing for the following parameter types: $str. Make sure you also add the sensor type to `$parameterTypes under the Types context!"
        }
    }

    Context "Types" {

        $parameterTypes = @(
            @{name = "ExeXml"}
            @{name = "WmiService"}
            @{name = "Http"}
            @{name = "Factory"}
        )

        It "creates a set of <name> parameters" -TestCases $parameterTypes {
            param($name)

            $params = New-SensorParameters $name

            $params.SensorType | Should Be $name
        }
    }

    Context "Raw Parameters" {
        It "can use raw parameters" {        
            $raw =@{
                "name_" = "custom name"
                "sensortype" = "custom type"
            }
        
            $params = New-SensorParameters $raw

            $params.GetType().Name | Should Be "PSRawSensorParameters"
            $params.Name | Should Be "custom name"
            $params.SensorType | Should Be "custom type"
            $params.DynamicType | Should Be $false
        }

        It "creates parameters with a -DynamicType" {
            $raw =@{
                "name_" = "custom name"
                "sensortype" = "custom type"
            }
        
            $params = New-SensorParameters $raw -DynamicType

            $params.GetType().Name | Should Be "PSRawSensorParameters"
            $params.Name | Should Be "custom name"
            $params.SensorType | Should Be "custom type"
            $params.DynamicType | Should Be $true
        }

        It "throws when a raw name isn't specified" {
            { New-SensorParameters @{"sensortype" = "custom type"} } | Should Throw "'name_' is mandatory"
        }

        It "throws when a raw name is null" {
            { New-SensorParameters @{"name_" = $null; "sensortype" = "custom type"} } | Should Throw "An object name cannot be null."
        }

        It "throws when a raw sensortype isn't specified" {
            { New-SensorParameters @{"name_" = "custom name"} } | Should Throw "'sensortype' is mandatory"
        }

        It "throws when a raw sensortype is null" {
            { New-SensorParameters @{"name_" = "custom name"; "sensortype" = $null} } | Should Throw "SensorType cannot be null or empty."
        }

        It "throws when multiple CustomParameter objects exist for a non sensor target property" {
            $params = New-SensorParameters -Empty
            $params["name"] = "first"
            $params["name"] | Should Be "first"
            $params.Parameters.Add((New-Object PrtgAPI.Parameters.CustomParameter "name","second"))

            $params["name"] | Should BeNullOrEmpty

            WithStrict {
                { $params["name"] } | Should Throw "Property 'name' contains an invalid collection of elements"
            }
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
            ValidateParams $params "test_=goodbye&name_=testName&sensortype=testType"
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

    Context "Dynamic Parameters" {
        
        function GetDynamicParams
        {
            $params = WithResponse "MultiTypeResponse" {
                $device | New-SensorParameters -RawType exexml
            }

            return $params
        }

        $nameCases = @(
            @{type = "raw"; name="name_"}
            @{type = "normal"; name="name"}
        )
        
        #region Get

        It "gets a CLR property via its <type> name" -TestCases $nameCases {
            param($name)

            $params = GetDynamicParams

            $expected = "XML Custom EXE/Script Sensor"

            $params[$name] | Should Be $expected
            $params.Name | Should Be $expected
            $params.DynamicType | Should Be $false
        }

        It "gets a real parameter CLR property" {
            $params = GetDynamicParams

            $expected = "exexml"

            $params["sensortype"] | Should Be $expected
            $params.SensorType | Should Be $expected
        }

        It "gets properties and ignores case" {
            $params = GetDynamicParams

            $params["NAME"] | Should Be "XML Custom EXE/Script Sensor"
            $params["SENSORTYPE"] | Should Be "exexml"
        }

        It "throws getting when a parameter doesn't exist" {
            $params = GetDynamicParams

            $val1 = $params["test_"]

            WithStrict {
                { $val2 = $params["test_"] } | Should Throw "Parameter with name 'test_' does not exist"
            }
        }

        It "gets a non-existant paramter via a dynamic property" {

            $params = GetDynamicParams

            { $params.newprop } | Should Throw "Parameter with name 'newprop' does not exist."
        }

        It "gets a parameter via a dynamic property ending in an underscore" {
            $params = GetDynamicParams

            $params.environment_ | Should Be "0"
        }

        It "gets a parameter via a dynamic property and ignores case" {
            $params = GetDynamicParams

            $params.ENVIRONMENT | Should Be "0"
        }

        It "gets a sensor target" {
            $params = GetDynamicParams

            $params.exefile.GetType().Name | Should Be "GenericSensorTarget"
        }

        It "specifies -rt" {
            $params = WithResponse "MultiTypeResponse" {
                $device | New-SensorParameters -rt exexml
            }

            $params.SensorType | Should Be "exexml"
        }

        #endregion
        #region Set

        It "sets a CLR property via its <type> name" -TestCases $nameCases {
            param($name)

            $params = GetDynamicParams

            $expected = "New Name"

            $params[$name] = $expected
            $params.Name | Should Be $expected
        }

        It "sets a real parameter CLR property" {
            $params = GetDynamicParams

            $expected = "wmiservice"

            $params["sensortype"] = $expected
            $params.SensorType | Should Be $expected
        }

        It "sets properties and ignores case" {
            $params = GetDynamicParams

            $params["NAME"] = "New Name"
            $params["SENSORTYPE"] = "wmiservice"

            $params["name"] | Should Be "New Name"
            $params["sensortype"] | Should Be "wmiservice"
        }

        It "sets an existing custom property and ignores case" {

            $params = GetDynamicParams

            $params["environment"] | Should Be "0"
            $params["ENVIRONMENT"] = "1"

            $params["environment"] | Should Be "1"
        }

        It "throws setting when a parameter doesn't exist" {
            $params = GetDynamicParams

            { $params["test_"] = 3 } | Should Throw "Parameter with name 'test_' does not exist. To add new parameters object must first be unlocked"
        }

        It "adds a new parameter when parameters are unlocked" {
            $params = GetDynamicParams

            $params.Unlock()

            $params["test_"] = "hello"

            $params["test_"] | Should Be "hello"

            $paramsArr = @(
                "name_=XML+Custom+EXE%2FScript+Sensor"
                "priority_=3"
                "inherittriggers=1"
                "intervalgroup=1"
                "interval_=60%7C60+seconds"
                "errorintervalsdown_=1"
                "tags_=xmlexesensor"
                "exefilelabel="
                "exeparams_="
                "environment_=0"
                "usewindowsauthentication_=0"
                "mutexname_="
                "timeout_=60"
                "writeresult_=0"
                "exefile_=Demo+Batchfile+-+Returns+static+values+in+four+channels.bat%7CDemo+Batchfile+-+Returns+static+values+in+four+channels.bat%7C%7C"
                "test_=hello"
                "sensortype=exexml"
            )

            $address = [string]::Join("&", $paramsArr)

            ValidateParams $params $address
        }

        It "sets a new parameter when parameters are unlocked and ignores case" {
            $params = GetDynamicParams

            $params.Unlock()

            $params["test_"] = "hello"
            $params["TEST_"] = "goodbye"

            $params["test_"] | Should Be "goodbye"
        }

        It "throws setting a non-existant parameter via a dynamic property when parameters are locked" {

            $params = GetDynamicParams

            { $params.test = "blah" } | Should Throw "Parameter with name 'test' does not exist. To add new parameters object must first be unlocked."
        }

        It "sets a non-existant parameter via a dynamic property when parameters are unlocked" {

            $params = GetDynamicParams

            $params.Unlock()

            $params.test = "blah"
            $params.test | Should Be "blah"
            $params["test"] | Should Be "blah"
        }

        It "sets a parameter via a dynamic property and ignores case" {
            $params = GetDynamicParams

            $params.ENVIRONMENT = "1"

            $params["environment"] | Should Be "1"
            $params.Environment | Should Be "1"
        }

        It "sets a non-existant parameter via a dynamic property with an underscore but doesn't have an underscore in the final name" {

            $params = GetDynamicParams
            $params.Unlock()

            $expected = "newval"

            $params.newprop_ = $expected
            $params.newprop | Should Be $expected
            $params["newprop"] | Should Be $expected
            $params["newprop_"] | Should Be $expected
        }

        It "sets a typed parameter without a trailing underscore" {
            $params = GetDynamicParams

            $params["interval"] = "60|60 seconds"

            $paramsArr = @(
                "name_=XML+Custom+EXE%2FScript+Sensor"
                "priority_=3"
                "inherittriggers=1"
                "intervalgroup=0"
                "interval_=60%7C60+seconds"
                "errorintervalsdown_=1"
                "tags_=xmlexesensor"
                "exefilelabel="
                "exeparams_="
                "environment_=0"
                "usewindowsauthentication_=0"
                "mutexname_="
                "timeout_=60"
                "writeresult_=0"
                "exefile_=Demo+Batchfile+-+Returns+static+values+in+four+channels.bat%7CDemo+Batchfile+-+Returns+static+values+in+four+channels.bat%7C%7C"
                "sensortype=exexml"
            )

            $address = [string]::Join("&", $paramsArr)

            ValidateParams $params $address
        }

        It "sets a typed parameter with a trailing underscore" {
            $params = GetDynamicParams
            
            $params["interval_"] = "300|5 minutes"

            $paramsArr = @(
                "name_=XML+Custom+EXE%2FScript+Sensor"
                "priority_=3"
                "inherittriggers=1"
                "intervalgroup=0"
                "interval_=300%7C5+minutes"
                "errorintervalsdown_=1"
                "tags_=xmlexesensor"
                "exefilelabel="
                "exeparams_="
                "environment_=0"
                "usewindowsauthentication_=0"
                "mutexname_="
                "timeout_=60"
                "writeresult_=0"
                "exefile_=Demo+Batchfile+-+Returns+static+values+in+four+channels.bat%7CDemo+Batchfile+-+Returns+static+values+in+four+channels.bat%7C%7C"
                "sensortype=exexml"
            )

            $address = [string]::Join("&", $paramsArr)

            ValidateParams $params $address
        }

        It "sets a sensor target" {
            $params = GetDynamicParams

            $params.exefile.GetType().Name | Should Be "GenericSensorTarget"
            $params.exefile.ToString() | Should Be "Demo Batchfile - Returns static values in four channels.bat"
            
            $params.exefile = $params.Targets["exefile"][1]
            $params.exefile.GetType().Name | Should Be "GenericSensorTarget"
            $params.exefile | Should Be "testScript.bat"
        }

        It "sets multiple sensor targets" {
            $params = GetDynamicParams

            $params.exefile = $params.Targets["exefile"]
            
            $params.Targets["exefile"].Count | Should Be 2

            $params.exefile = $params.Targets["exefile"]
            $params.exefile.GetType().Name | Should Be "GenericSensorTarget[]"
            $params.exefile[0].ToString() | Should Be "Demo Batchfile - Returns static values in four channels.bat"
            $params.exefile[1].ToString() | Should Be "testScript.bat"
        }

        It "sets multiple objects that are not sensor targets" {
            $params = GetDynamicParams

            $params.environment = 3,4
            $params.environment[0] | Should Be 3
            $params.environment[1] | Should Be 4
        }

        It "sets multiple sensor targets retrieved from Where-Object" {

            $params = GetDynamicParams

            $params.Targets["exefile"].Count | Should Be 2
            $params.exefile = $params.Targets["exefile"]
            $params.exefile.GetType().Name | Should Be "GenericSensorTarget[]"
            $params.exefile = $params.Targets["exefile"] | where name -Like *
            $params.exefile.GetType().Name | Should Be "object[]"

            $paramsArr = @(
                "name_=XML+Custom+EXE%2FScript+Sensor"
                "priority_=3"
                "inherittriggers=1"
                "intervalgroup=1"
                "interval_=60%7C60+seconds"
                "errorintervalsdown_=1"
                "tags_=xmlexesensor"
                "exefilelabel="
                "exeparams_="
                "environment_=0"
                "usewindowsauthentication_=0"
                "mutexname_="
                "timeout_=60"
                "writeresult_=0"
                "exefile_=Demo+Batchfile+-+Returns+static+values+in+four+channels.bat%7CDemo+Batchfile+-+Returns+static+values+in+four+channels.bat%7C%7C"
                "exefile_=testScript.bat%7CtestScript.bat%7C%7C"
                "sensortype=exexml"
            )

            $address = [string]::Join("&", $paramsArr)

            ValidateParams $params $address
        }

        It "locks and unlocks" {

            $params = GetDynamicParams

            { $params.newprop = "newval" } | Should Throw "Parameter with name 'newprop' does not exist. To add new parameters object must first be unlocked."
            $params.IsLocked() | Should Be $true

            $params.Unlock()

            $params.newprop = "newval2"
            $params.newprop | Should Be "newval2"
            $params.IsLocked() | Should Be $false

            $params.Lock()

            { $params.NEWPROP2 = "newval3" } | Should Throw "Parameter with name 'NEWPROP2' does not exist. To add new parameters object must first be unlocked."
            $params.IsLocked() | Should Be $true
        }

        #endregion

        It "creates parameters with a -DynamicType" {
            $params = WithResponse "MultiTypeResponse" {
                $device | New-SensorParameters -RawType exexml -DynamicType
            }

            $params.DynamicType | Should Be $true
        }
        
        It "pipes parameters to Add-Sensor" {

            $address = "blah"

            $device = Run Device { Get-Device }

            $addAddressParts = @(
                "name_=HTTP"
                "priority_=3"
                "inherittriggers=1"
                "intervalgroup=1"
                "interval_=60%7C60+seconds"
                "errorintervalsdown_=1"
                "tags_=httpsensor"
                "timeout_=60"
                "httpurl_=http%3A%2F%2F"
                "httpmethod_=GET"
                "postcontentoptions_=0"
                "postcontenttype_="
                "snihost="
                "sni_inheritance_=0"
                "httpproxy=1"
                "proxy_="
                "proxyport_=8080"
                "proxyuser_="
                "proxypassword_="
                "postdata_="
                "sensortype=http"
                "id=40"
                "username=username"
                "passhash=12345678"
            )

            $combined = [string]::Join("&", $addAddressParts)

            $final = "https://prtg.example.com/addsensor5.htm?$combined"

            $urls = @(
                @(
                    [Request]::SensorTypes(40)
                    [Request]::BeginAddSensorQuery(40, "http")
                    [Request]::AddSensorProgress(40, 2)
                    [Request]::AddSensorProgress(40, 2)
                    [Request]::EndAddSensorQuery(40, 2)
                    [Request]::Status()
                    [Request]::BeginAddSensorQuery(40, "http")
                    $final
                ),
                $true
            )

            WithResponseArgs "AddressValidatorResponse" $urls {
                $device | New-SensorParameters -RawType http | Add-Sensor -Resolve:$false
            }
        }

        It "filters by targets" {

            $device = Run Device { Get-Device }

            $normalParams = WithResponse "MultiTypeResponse" {
                $device | New-SensorParameters -RawType exexml
            }

            $normalParams.exefile.ToString() | Should Be "Demo Batchfile - Returns static values in four channels.bat"

            $filterParams = WithResponse "MultiTypeResponse" {
                $device | New-SensorParameters -RawType exexml -Target *test*
            }

            $filterParams.exefile.ToString() | Should Be "testScript.bat"
        }

        It "specifies a timeout" {
            $device = Run Device { Get-Device }

            $normalParams = WithResponse "MultiTypeResponse" {
                $device | New-SensorParameters -RawType exexml -Timeout 3
            }

            $normalParams.exefile.ToString() | Should Be "Demo Batchfile - Returns static values in four channels.bat"
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

    Context "WmiServiceSensorParameters" {

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
            SetValue $params "NotifyChanged" $false
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

    Context "HttpSensorParameters" {

        It "can set a value on each property" {
            SetResponseAndClient "HttpTargetResponse"

            $params = New-SensorParameters Http

            SetValue $params "Timeout" 27
            SetValue $params "Url" "http://localhost"
            SetValue $params "HttpRequestMethod" "HEAD"
            SetValue $params "PostData" "test"
            SetValue $params "UseCustomPostContent" $true
            SetValue $params "PostContentType" "customType"
            SetValue $params "UseSNIFromUrl" $true
        }
    }

    Context "FactorySensorParameters" {

        It "can set a value on each property" {
            SetMultiTypeResponse

            $params = New-SensorParameters Factory

            SetValue $params "ChannelDefinition" "a","b"
            SetValue $params "FactoryErrorMode" "WarnOnError"
            SetValue $params "FactoryErrorFormula" "test"
            SetValue $params "FactoryMissingDataMode" "CalculateWithZero"
        }
    }

    Context "Query Target" {
        It "parses a sensor query target" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target

                [Request]::SensorTypes(40) # Verify specified target
                [Request]::BeginAddSensorQuery(40, "snmplibrary_nolist", "APC+UPS.oidlib")
                [Request]::AddSensorProgress(40, 2)
                [Request]::EndAddSensorQuery(40, 2)
            )

            $target = $device | Get-SensorType snmplibrary | select -ExpandProperty QueryTargets|select -First 1

            $target | Should Be "APC UPS.oidlib"

            $device | New-SensorParameters -RawType snmplibrary -qt $target
        }

        It "parses a sensor query target wildcard" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40), # Get initial target from wildcard

                [Request]::SensorTypes(40) # Verify specified target
                [Request]::BeginAddSensorQuery(40, "snmplibrary_nolist", "APC+UPS.oidlib")
                [Request]::AddSensorProgress(40, 2)
                [Request]::EndAddSensorQuery(40, 2)
            )

            $device | New-SensorParameters -RawType snmplibrary -qt *ups*
        }

        It "throws when a sensor query target wildcard does not match" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target from wildcard
            )

            { $device | New-SensorParameters -RawType snmplibrary -qt *potato* } | Should Throw "Could not find a query target matching the wildcard expression '*potato*'. Please specify one of the following parameters: 'APC UPS.oidlib',"
        }

        It "throws when a sensor query target wildcard is ambiguous" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target from wildcard
            )

            { $device | New-SensorParameters -RawType snmplibrary -qt *apc* } | Should Throw "Query target wildcard '*apc*' is ambiguous between the following parameters: 'APC UPS.oidlib', 'APCSensorstationlib.oidlib'. Please specify a more specific identifier."
        }

        It "throws when a sensor query target is invalid" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target from wildcard
            )

            { $device | New-SensorParameters -RawType snmplibrary -qt potato } | Should Throw "Query target 'potato' is not a valid target for sensor type 'snmplibrary' on device ID 40. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "throws when target is not required" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40)
            )

            { $device | New-SensorParameters -RawType ptfadsreplfailurexml -qt potato } | Should Throw "Cannot specify query target 'potato' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
        }

        It "throws when target is missing" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40)
            )

            { $device | New-SensorParameters -RawType snmplibrary } | Should Throw "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "parses a set of sensor query target parameters" {
            SetCustomAddressValidatorResponse "SensorQueryTargetParametersValidatorResponse" @(
                [Request]::SensorTypes(40)
                [Request]::BeginAddSensorQuery(40, "ptfadsreplfailurexml")
                [Request]::ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0")
                [Request]::AddSensorProgress(40, 7)
                [Request]::EndAddSensorQuery(40, 7)
            )

            $device | New-SensorParameters -RawType ptfadsreplfailurexml -qp @{
                database = "XE"
                sid_type = 0
                prefix = 0
            }
        }

        It "throws when sensor query target parameters are not specified" {

            SetCustomAddressValidatorResponse "SensorQueryTargetParametersValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target from wildcard

                [Request]::SensorTypes(40)
            )

            { $device | New-SensorParameters -RawType ptfadsreplfailurexml -qt *ups* } | Should Throw "Cannot specify query target '*ups*' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
        }

        It "throws when a sensor query target parameter is missing" {
            SetCustomAddressValidatorResponse "SensorQueryTargetParametersValidatorResponse" @(
                [Request]::SensorTypes(40)
                [Request]::BeginAddSensorQuery(40, "ptfadsreplfailurexml")
            )

            { $device | New-SensorParameters -RawType ptfadsreplfailurexml } | Should Throw "Failed to process request for sensor type 'oracletablespace': sensor query target parameters are required, however none were specified. Please retry the request specifying the parameters 'database_',"
        }
    }
}