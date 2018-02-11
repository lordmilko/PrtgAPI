. $PSScriptRoot\Support\Standalone.ps1

Describe "Add-Sensor" -Tag @("PowerShell", "UnitTest") {

    SetActionResponse

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

        $sensor.Id | Should Be 1000,1001
    }

    function GetItemSubset($skip, $select)
    {
        $format = [PrtgAPI.WmiServiceTarget].GetMethod("PrtgAPI.IFormattable.GetSerializedFormat", @("NonPublic", "Instance"))

        $serviceStrs = $services | select -First $select -Skip $skip | foreach {
            $str = ("service__check=" + $format.Invoke($_, $null))
            
            $str -replace "&","%26" `
                 -replace "\+","%2b" `
                 -replace " ","+" -replace "/","%2f" -replace "`"","%22"-replace "\|","%7c" -replace ",","%2c" -replace "'","%27" -replace ":","%3a" -replace ";","%3b" -replace "–","%e2%80%93" -replace "’","%e2%80%99"
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

        $base = "addsensor5.htm?name_=Service&priority_=3&inherittriggers_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&"
        $end = "&id=40&"

        $first =   GetItemSubset 0   30
        $second =  GetItemSubset 30  30
        $third =   GetItemSubset 60  30
        $fourth =  GetItemSubset 90  30
        $fifth =   GetItemSubset 120 30
        $sixth =   GetItemSubset 150 30
        $seventh = GetItemSubset 180 30

        SetAddressValidatorResponse @(
            "$base$first$end"
            "$base$second$end"
            "$base$third$end"
            "$base$fourth$end"
            "$base$fifth$end"
            "$base$sixth$end"
            "$base$seventh$end"
        )

        $device | Add-Sensor $params -Resolve:$false
    }

    It "adds a non-excessive number of objects on a sensor type with an excessive limit" {
        SetResponseAndClient "WmiServiceTargetResponse"

        $device = Run Device { Get-Device }

        $services = @($device | Get-SensorTarget WmiService | Select -First 1)

        $services.Count | Should Be 1

        $params = New-SensorParameters WmiService $services

        SetAddressValidatorResponse "addsensor5.htm?name_=Service"

        $device | Add-Sensor $params -Resolve:$false
    }

    It "adds a sensor from sensor parameters piped from Get-SensorTarget" {
        SetMultiTypeResponse

        Get-Device -Count 1 | Get-SensorTarget ExeXml *test* -Parameters | Add-Sensor -Resolve:$false
    }

    It "throws piping sensors not created by Get-SensorTarget" {
        { New-SensorParameters ExeXml | Add-Sensor } | Should Throw "Only sensor parameters created by Get-SensorTarget can be piped"
    }
}