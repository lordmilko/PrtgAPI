. $PSScriptRoot\Support\Standalone.ps1

function SetValue($params, $property, $value)
{
    $initial = $params.$property

    $params.$property = $value

    $new = $params.$property

    $new | Should Not Be $initial
    $new | Should Be $value
}

Describe "New-SensorParameters" {

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

    Context "ExeXmlSensorParameters" {

        It "can set a value on each property" {
            $params = New-SensorParameters ExeXml

            SetValue $params "Name" "New Sensor"
            setValue $params "Tags" "newTag"
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
}