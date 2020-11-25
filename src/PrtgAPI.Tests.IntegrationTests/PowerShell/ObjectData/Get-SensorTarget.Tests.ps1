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

    Context "Query Target" {
        It "parses a sensor query target" {
            { Get-Device -Id (Settings Device) | Get-SensorTarget -RawType snmplibrary -Timeout 3 -qt "APC UPS.oidlib" } | Should Throw "Failed to retrieve sensor information within a reasonable period of time"
        }

        It "throws when a sensor query target is invalid" {
            $id = (Settings Device)

            { Get-Device -Id $id | Get-SensorTarget -RawType snmplibrary -Timeout 3 -qt test } | Should Throw "Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID $id. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "throws when a sensor query target is not required" {
            { Get-Device -Id (Settings Device) | Get-SensorTarget -RawType exexml -qt "test" } | Should Throw "Cannot specify query target 'test' on sensor type 'exexml': type does not support query targets."
        }

        It "throws when sensor query target is missing" {
            { Get-Device -Id (Settings Device) | Get-SensorTarget -RawType snmplibrary } | Should Throw "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "parses a set of sensor query target parameters" {
            { Get-Device -Id (Settings Device) | Get-SensorTarget -RawType oracletablespace -qp @{
                database = "XE"
                sid_type = 0
                prefix = 0
            } } | Should Throw "Specified sensor type may not be valid on this device"
        }

        It "throws when sensor query target parameters are not specified" {
            { Get-Device -Id (Settings Device) | Get-SensorTarget -RawType oracletablespace -qp @{} } | Should Throw "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_', 'sid_type_', 'prefix_'."
        }
    }

    It "specifies a list of values" {
        $services = $device | Get-SensorTarget WmiService *prtg*,netlogon

        $services[0].Name | Should Be "Netlogon"
        $services[1].Name | Should Be "PRTGCoreService"
        $services[2].Name | Should Be "PRTGProbeService"
    }

    It "throws retrieving targets for a sensor type unsupported by a device" {

        { $device | Get-SensorTarget -RawType exchangepsbackup } | Should Throw "Specified sensor type may not be valid on this device"
    }

    It "throws attempting to resolve sensor targets as a read-only user" {
        ReadOnlyClient {
            { $device | Get-SensorTarget ExeXml } | Should Throw "you may not have sufficient permissions on the specified object. The server responded"
        }
    }

    It "specifies a timeout" {
        { $device | Get-SensorTarget WmiService -Timeout 0 } | Should Throw "Failed to retrieve sensor information within a reasonable period of time."
    }
}