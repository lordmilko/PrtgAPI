. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Add-Sensor" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    It "adds a sensor" {
        $params = New-SensorParameters ExeXml -Second "test.ps1"

        $device = Run Device { Get-Device }

        $device | Add-Sensor $params -Resolve:$false
    }

    It "adds a sensor missing a required value" {
        $params = New-SensorParameters ExeXml

        $device = Run Device { Get-Device }

        { $device | Add-Sensor $params -Resolve:$false } | Should Throw "'ExeFile' requires a value"
    }

    It "executes with -WhatIf" {
        $params = New-SensorParameters ExeXml

        $device = Run Device { Get-Device }

        $device | Add-Sensor $params -Resolve:$false -WhatIf
    }

    It "resolves a created sensor" {
        SetResponseAndClient "DiffBasedResolveResponse"

        $params = New-SensorParameters ExeXml -Second "test.ps1"

        $device = Run Device { Get-Device }

        $sensor = $device | Add-Sensor $params -Resolve

        $sensor.Id | Should Be 1002,1003
    }

    function GetItemSubset($skip, $select)
    {
        $format = [PrtgAPI.Targets.WmiServiceTarget].GetMethod("PrtgAPI.Request.ISerializable.GetSerializedFormat", @("NonPublic", "Instance"))

        $serviceStrs = $services | select -First $select -Skip $skip | foreach {
            $str = ("service__check=" + $format.Invoke($_, $null))
            
            $str -replace "&","%26" `
                 -replace "\+","%2B" `
                 -replace " ","+" -replace "/","%2F" -replace "`"","%22"-replace "\|","%7C" -replace ",","%2C" -replace "'","%27" -replace ":","%3A" -replace ";","%3B" -replace "–","%E2%80%93" -replace "’","%E2%80%99"
        }

        $str = [string]::Join("&", $serviceStrs)

        return $str
    }

    It "adds an excessive number of items" {
        SetResponseAndClient "WmiServiceTargetResponse"

        $device = Run Device { Get-Device }

        $services = $device | Get-SensorTarget WmiService

        $services.Count | Should BeGreaterThan 30

        $params = New-SensorParameters WmiService $services

        $base = "name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&"
        $end = "&id=40"

        $first =   GetItemSubset 0   30
        $second =  GetItemSubset 30  30
        $third =   GetItemSubset 60  30
        $fourth =  GetItemSubset 90  30
        $fifth =   GetItemSubset 120 30
        $sixth =   GetItemSubset 150 30
        $seventh = GetItemSubset 180 30

        SetAddressValidatorResponse @(
            [Request]::Status()
            [Request]::BeginAddSensorQuery(40, "wmiservice")
            [Request]::AddSensor("$base$first$end")
            [Request]::AddSensor("$base$second$end")
            [Request]::AddSensor("$base$third$end")
            [Request]::AddSensor("$base$fourth$end")
            [Request]::AddSensor("$base$fifth$end")
            [Request]::AddSensor("$base$sixth$end")
            [Request]::AddSensor("$base$seventh$end")
        )

        $device | Add-Sensor $params -Resolve:$false
    }

    It "adds a non-excessive number of objects on a sensor type with an excessive limit" {
        SetResponseAndClient "WmiServiceTargetResponse"

        $device = Run Device { Get-Device }

        $services = @($device | Get-SensorTarget WmiService | Select -First 1)

        $services.Count | Should Be 1

        $params = New-SensorParameters WmiService $services

        SetAddressValidatorResponse @(
            [Request]::Status()
            [Request]::BeginAddSensorQuery(40, "wmiservice")
            [Request]::AddSensor("name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&service__check=AxInstSV%7CActiveX+Installer+(AxInstSV)%7CProvides+User+Account+Control+validation+for+the+installation+of+ActiveX+controls+from+the+Internet+and+enables+management+of+ActiveX+control+installation+based+on+Group+Policy+settings.+This+service+is+started+on+demand+and+if+disabled+the+installation+of+ActiveX+controls+will+behave+according+to+default+browser+settings.%7CStopped%7C%7C&id=40")
        )

        $device | Add-Sensor $params -Resolve:$false
    }

    It "adds a sensor from sensor parameters piped from Get-SensorTarget" {
        SetMultiTypeResponse

        Get-Device -Count 1 | Get-SensorTarget ExeXml *test* -Parameters | Add-Sensor -Resolve:$false
    }

    It "throws piping sensors not created by Get-SensorTarget" {
        { New-SensorParameters ExeXml | Add-Sensor } | Should Throw "Only sensor parameters created by Get-SensorTarget can be piped"
    }

    It "ignores sensor query targets" {

        SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
            [Request]::Status()
            [Request]::BeginAddSensorQuery(40, "snmplibrary"),
            [Request]::AddSensor("name_=test&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&sensortype=snmplibrary&id=40")
        )

        $params = New-SensorParameters -RawParameters @{
            name_ = "test"
            sensortype = "snmplibrary"
        }

        $device = Run Device { Get-Device }

        $device | Add-Sensor $params -Resolve:$false
    }

    It "synthesizes sensor query parameters" {
        $response = SetCustomAddressValidatorResponse "SensorQueryTargetParametersValidatorResponse" @(
            [Request]::Status(),
            [Request]::BeginAddSensorQuery(40, "oracletablespace")
            [Request]::ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0"), # Response hardcodes 2055, however normally this will in fact match
            [Request]::AddSensor("name_=test&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&sid_type=0&database=XE&prefix=0&sensortype=oracletablespace&id=40")
        )

        if($PSEdition -eq "Core")
        {
            $response.AllowReorder = $true
        }

        $params = New-SensorParameters -RawParameters @{
            name_ = "test"
            sensortype = "oracletablespace"
            database = "XE"
            sid_type = 0
            prefix = 0
        }

        $device = Run Device { Get-Device }

        $device | Add-Sensor $params -Resolve:$false
    }

    It "throws when synthesized sensor query parameters are missing" {
        SetCustomAddressValidatorResponse "SensorQueryTargetParametersValidatorResponse" @(
            [Request]::Status(),
            [Request]::BeginAddSensorQuery(40, "oracletablespace")
        )

        $params = New-SensorParameters -RawParameters @{
            name_ = "test"
            sensortype = "oracletablespace"
            sid_type = 0
            prefix = 0
        }

        $device = Run Device { Get-Device }

        { $device | Add-Sensor $params -Resolve:$false } | Should Throw "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameter 'database_'."
    }
}