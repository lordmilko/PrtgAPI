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

Describe "Get-SensorTarget_IT" -Tag @("PowerShell", "IntegrationTest") {

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

    It "specifies a list of values" {
        $services = $device | Get-SensorTarget WmiService *prtg*,netlogon

        $services[0].Name | Should Be "Netlogon"
        $services[1].Name | Should Be "PRTGCoreService"
        $services[2].Name | Should Be "PRTGProbeService"
    }

    It "throws retrieving targets for a sensor type unsupported by a device" {

        { $device | Get-SensorTarget -RawType exchangepsbackup } | Should Throw "An unspecified error occurred while trying to resolve sensor targets. Specified sensor type may not be valid on this device"
    }

    It "throws attempting to resolve sensor targets as a readonly user" {
        ReadOnlyClient {
            { $device | Get-SensorTarget ExeXml } | Should Throw "type was not valid or you do not have sufficient permissions on the specified object"
        }
    }

    It "specifies a timeout" {
        { $device | Get-SensorTarget WmiService -Timeout 0 } | Should Throw "Failed to retrieve sensor information within a reasonable period of time."
    }
}