. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Get-ObjectProperty" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    #region Object

    Context "Default" {
        It "can deserialize sensor settings" {
            $sensor = Get-Sensor -Count 1

            $properties = $sensor | Get-ObjectProperty

            $properties.GetType().Name | Should Be SensorSettings
        }

        It "can deserialize device settings" {
            $device = Get-Device -Count 1

            $properties = $device | Get-ObjectProperty

            $properties.GetType().Name | Should Be DeviceSettings
        }

        It "warns when an object is read-only" {
            WithReadOnly {
                $sensor = Get-Sensor -Count 1

                { $sensor | Get-ObjectProperty } | Should Throw "Cannot retrieve properties for read-only sensor with ID 4000."
            }
        }
    }

    Context "Property" {
        It "retrieves a single property" {
            $device = Get-Device -Count 1

            $property = $device | Get-ObjectProperty -Property Tags

            $property[0] | Should Be "tag1"
            $property[1] | Should Be "tag2"
        }

        It "retrieves multiple properties" {
            $sensor = Get-Sensor -Count 1

            $properties = $sensor | Get-ObjectProperty Name,Tags

            $properties.Name | Should Be "testName"
            $properties.Tags.Count | Should Be 2
            $properties.Tags[0] | Should Be "tag1"
            $properties.Tags[1] | Should Be "tag2"
        }

        It "throws trying to retrieve a mergeable property" {

            $device = Get-Device -Count 1

            { $Device | Get-ObjectProperty LocationName } | Should Throw "'LocationName' is a virtual property and cannot be retrieved directly. To access this value, property 'Location' should be retrieved instead."
        }
    }

    Context "RawProperty" {
        It "retrieves a raw property" {
            $sensor = Get-Sensor -Count 1

            $property = $sensor | Get-ObjectProperty -RawProperty name_

            $property | Should Be "testName"
        }

        It "retrieves a raw property with -Text" {

            $sensor = Get-Sensor -Count 1

            WithResponseArgs "AddressValidatorResponse" "id=4000&name=name&show=text&username=username" {
                $sensor | Get-ObjectProperty -RawProperty name_ -Text
            }
        }

        It "retrieves multiple raw properties" {
            $sensor = Get-Sensor -Count 1

            $properties = $sensor | Get-ObjectProperty -RawProperty name_,tags_

            $properties.name | Should Be "testName"
            $properties.tags | Should Be "tag1 tag2"
        }
    }

    Context "Raw" {
        It "retrieves all raw properties" {
            $sensor = Get-Sensor -Count 1

            $properties = $sensor | Get-ObjectProperty -Raw

            $properties.name | Should Be "Server CPU Usage"
            $properties.interval | Should Be "60|60 seconds"
            $properties.schedule | Should Be "627|Weekdays Nights (17:00 - 9:00) [GMT+0800]|"
        }
    }

    #endregion
    #region Id

    Context "PropertyManual" {
        It "retrieves a single property" {

            $property = Get-ObjectProperty -Id 1001 -Property Tags

            $property[0] | Should Be "tag1"
            $property[1] | Should Be "tag2"
        }

        It "retrieves multiple properties" {

            $properties = Get-ObjectProperty -Id 1001 -Property Name,Tags

            $properties.Name | Should Be "testName"
            $properties.Tags.Count | Should Be 2
            $properties.Tags[0] | Should Be "tag1"
            $properties.Tags[1] | Should Be "tag2"
        }
    }

    Context "RawPropertyManual" {
        It "retrieves a raw property" {

            $property = Get-ObjectProperty -Id 1001 -RawProperty name_

            $property | Should Be "testName"
        }

        It "retrieves a raw property with -Text" {

            WithResponseArgs "AddressValidatorResponse" "id=4000&name=name&show=text&username=username" {
                Get-ObjectProperty -Id 4000 -RawProperty name_ -Text
            }
        }

        It "retrieves multiple raw properties" {

            $properties = Get-ObjectProperty -Id 1001 -RawProperty name_,tags_

            $properties.name | Should Be "testName"
            $properties.tags | Should Be "tag1 tag2"
        }
    }

    Context "RawSubPropertyManual" {
        It "retrieves a sub object property" {

            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "limitmaxerror&subid=5&subtype=channel&show=nohtmlencode")
                [Request]::GetObjectProperty(1001, "limitminerror&subid=5&subtype=channel&show=nohtmlencode")
            )

            $response = Get-ObjectProperty -Id 1001 -SubId 5 -RawSubType channel -RawProperty limitmaxerror,limitminerror
            $response.limitmaxerror | Should Be 90
            $response.limitminerror | Should Be 30
        }

        It "retrieves a sub object property with -Text" {
            WithResponseArgs "AddressValidatorResponse" "api/getobjectproperty.htm?id=1001&name=limitmaxerror&subid=5&subtype=channel&username" {
                $response = Get-ObjectProperty -Id 1001 -SubId 5 -RawSubType channel -RawProperty limitmaxerror -Text

                $response | Should Be 90
            }
        }
    }

    Context "RawManual" {
        SetMultiTypeResponse

        It "retrieves all raw properties" {

            $properties = Get-ObjectProperty -Id 1001 -Raw

            $properties.name | Should Be "Server CPU Usage"
            $properties.interval | Should Be "60|60 seconds"
            $properties.schedule | Should Be "627|Weekdays Nights (17:00 - 9:00) [GMT+0800]|"
        }

        It "throws retrieving all raw properties when no parameter specified" {

            { Get-ObjectProperty -Id 1001 } | Should Throw "Parameter set cannot be resolved"
        }
    }

    It "throws an ErrorRecord when a property doesn't exist" {
        Get-Device -Count 1 | Get-ObjectProperty -RawProperty banana -ErrorAction SilentlyContinue

        $? | Should Be $false
    }

    It "throws when a property doesn't exist and ErrorActionPreference is stop" {
        { Get-Device -Count 1 | Get-ObjectProperty -RawProperty banana -ErrorAction Stop } | Should Throw "PRTG was unable to complete the request. A value for property 'banana' could not be found."
    }

    #endregion
}