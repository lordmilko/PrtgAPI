. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

function ResolvesAllItems($sensorType, $targetType)
{
    $items = $device | Get-SensorTarget $sensorType

    $items.Count | Should BeGreaterThan 1

    $item = $items | Select -First 1

    $item | Should Not BeNullOrEmpty

    $item.GetType().Name | Should Be $targetType
}

function FilterReturnedItems($sensorType, $goodFilter)
{
    $item = @($device | Get-SensorTarget $sensorType $goodFilter)

    $item.Count | Should Be 1

    $item.Name | Should Be $goodFilter

    $nothing = $device | Get-SensorTarget $sensorType "fake_prtgapi_item"

    $nothing | Should BeNullOrEmpty
}

function CreateParameters($sensorType, $goodFilter, $paramsType, $criticalValue)
{
    $params = $device | Get-SensorTarget $sensorType $goodFilter -Params

    $params.GetType().Name | Should Be $paramsType
    $params.$criticalValue.Name | Should Be $goodFilter
}

Describe "Get-SensorTarget_IT" {

    $device = Get-Device -Id (Settings Device)

    Context "ExeXml" {

        $fileName = "testScript.bat"

        It "resolves all EXE Files" {

            ResolvesAllItems "ExeXml" "ExeFileTarget"
        }

        It "filters returned EXE Files" {

            FilterReturnedItems "ExeXml" $fileName
        }

        It "creates a set of parameters from an EXE File" {

            CreateParameters "ExeXml" $fileName "ExeXmlSensorParameters" "ExeFile"
        }

        It "throws attempting to create parameters when more than one EXE File is returned" {

            $files = $device | Get-SensorTarget ExeXml

            $files.Count | Should BeGreaterThan 1

            { $device | Get-SensorTarget ExeXml -Params } | Should Throw "cannot be used against multiple targets in a single request"
        }
    }

    Context "WmiService" {

        $service = "PRTGCoreService"

        It "resolves a WMI Service" {
            ResolvesAllItems "WmiService" "WmiServiceTarget"
        }

        It "filters returned WMI Services" {
            FilterReturnedItems "WmiService" $service
        }

        It "creates a set of parameters from a set of WMI Services" {
            CreateParameters "WmiService" $service "WmiServiceSensorParameters" "Services"
        }
    }
}