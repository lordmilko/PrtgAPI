. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

function RetrieveProperties([ScriptBlock]$getObjects)
{
    $properties = (& $getObjects) | Get-ObjectProperty

    $properties.Count | Should BeGreaterThan 1
}

Describe "Get-ObjectProperty_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "retrieves all sensor properties" {
        RetrieveProperties { Get-Sensor }
    }

    It "retrieves all device properties" {
        RetrieveProperties { Get-Device }
    }

    It "retrieves all group properties" {
        RetrieveProperties { Get-Group }
    }

    It "retrieves all probe properties" {
        RetrieveProperties { Get-Group }
    }

    It "retrieves an raw property" {
        $device = Get-Device -Id (Settings Device)

        $property = $device | Get-ObjectProperty -RawProperty "name"

        $property | Should Be (Settings DeviceName)
    }

    It "retrieves an raw property with a trailing underscore" {
        $device = Get-Device -Id (Settings Device)

        $property = $device | Get-ObjectProperty -RawProperty "name_"

        $property | Should Be (Settings DeviceName)
    }

    It "retrieves multiple raw properties" {
        $device = Get-Device -Id (Settings Device)

        $result = $device | Get-ObjectProperty -RawProperty "name_","host_","serviceurl_"

        $name = (Settings DeviceName)

        $result.name | Should Be $name
        $result.host | Should Be $name
        $result.serviceurl | Should Be "http://$name"
    }

    It "retrieves properties from a sub object" {

        $sensorId = (Settings ChannelSensor)
        $channelId = (Settings Channel)

        $channel = Get-Channel -SensorId $sensorId -Id $channelId

        $channel | Should Not BeNullOrEmpty
        $channel.UpperErrorLimit | Should Not BeNullOrEmpty

        $val = Get-ObjectProperty -Id $sensorId -SubId $channelId -RawSubType channel -RawProperty limitmaxerror

        $val | Should Be $channel.UpperErrorLimit
    }

    It "returns 'Not found' retrieving properties from an invalid sub object type" {
        
        $result = Get-ObjectProperty -Id (Settings ChannelSensor) -SubId 1 -RawSubType "blah" -RawProperty "blahblah"

        if(IsEnglish)
        {
            $result | Should Be "Not found"
        }
    }

    it "returns 'Not found' retrieving properties from an invalid sub id" {

        $result = Get-ObjectProperty -Id (Settings ChannelSensor) -SubId 1000 -RawSubType channel -RawProperty "limitmaxerror"

        if(IsEnglish)
        {
            $result | Should Be "Not found"
        }
    }

    It "throws retrieving a raw inheritance flag" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $scriptBlock = { $sensor | Get-ObjectProperty -RawProperty "accessgroup" }

        if(IsEnglish)
        {
            $scriptBlock | Should Throw "A value for property 'accessgroup' could not be found"
        }
        else
        {
            & $scriptBlock
        }
    }

    It "retrieves an invalid unsupported property" {
        $device = Get-Device -Id (Settings Device)

        $scriptBlock = { $device | Get-ObjectProperty -RawProperty "banana" }

        if(IsEnglish)
        {
            $scriptBlock | Should Throw "A value for property 'banana' could not be found"
        }
        else
        {
            & $scriptBlock
        }
    }

    It "throws an ErrorRecord when a property doesn't exist" {
        $device = Get-Device -Id (Settings Device)

        $device | Get-ObjectProperty -RawProperty "banana" -ErrorAction SilentlyContinue
    }

    $cases = @(
        @{name = "Sensor"}
        @{name = "Device"}
        @{name = "Group"}
        @{name = "Probe"}
    )

    It "throws retrieving typed properties as a readonly user" -TestCases $cases {

        param($name)

        ReadOnlyClient {

            $obj = Invoke-Expression "Get-$name -Count 1"

            $obj.Count | Should Be 1

            { $obj | Get-ObjectProperty } | Should Throw "Cannot retrieve properties for read-only"
        }
    }

    It "retrieves raw properties as a readonly user" -TestCases $cases {

        param($name)

        ReadOnlyClient {
            $obj = Invoke-Expression "Get-$name -Count 1"

            $obj.Count | Should Be 1

            $obj | Get-ObjectProperty -Raw
        }
    }

    It "retrieves individual typed properties as a readonly user" -TestCases $cases {

        param($name)

        ReadOnlyClient {
             $obj = Invoke-Expression "Get-$name -Count 1"

            $obj.Count | Should Be 1

            $name = $obj | Get-ObjectProperty Name

            $name | Should Be $obj.Name
        }
    }

    It "retrieves individual raw properties as a readonly user" -TestCases $cases {

        param($name)

        ReadOnlyClient {
            $obj = Invoke-Expression "Get-$name -Count 1"

            $obj.Count | Should Be 1

            $name = $obj | Get-ObjectProperty -RawProperty name_

            $name | Should Be $obj.Name
        }
    }
}