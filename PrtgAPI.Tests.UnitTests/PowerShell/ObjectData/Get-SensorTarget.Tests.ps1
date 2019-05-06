. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

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

Describe "Get-SensorTarget" -Tag @("PowerShell", "UnitTest") {
    $device = Run Device { Get-Device } | Select -First 1

    Context "ExeXml" {

        SetResponseAndClient "ExeFileTargetResponse"

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

        SetResponseAndClient "WmiServiceTargetResponse"

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

    Context "SqlServerDB" {

        SetResponseAndClient "SqlServerQueryFileTargetResponse"

        $query = "test.sql"

        It "resolves a SQL Server Query File" {
            ResolvesAllItems "SqlServerDB" "SqlServerQueryTarget"
        }

        It "filters returned SQL Server Query Files" {
            FilterReturnedItems "SqlServerDB" $query
        }

        It "throws attempting to create a set of parameters from a SQL Server Query File" {

            $err = "Creating sensor parameters for sensor type 'SqlServerDB' is not supported"

            { CreateParameters "SqlServerDB" $query "SqlServerDBSensorParameters" "QueryFile" } | Should Throw $err
        }
    }

    Context "Raw" {
        SetResponseAndClient "ExeFileTargetResponse"

        It "resolves raw EXE files" {
            $items = $device | Get-SensorTarget -RawType exexml

            $items.Count | Should BeGreaterThan 1

            $item = $items | Select -First 1

            $item | Should Not BeNullOrEmpty

            $item.GetType().Name | Should Be "GenericSensorTarget"
        }

        It "specifies a table name" {
            $items = $device | Get-SensorTarget -RawType exexml -Table exefile

            $items.Count | Should BeGreaterThan 1

            $item = $items | Select -First 1

            $item | Should Not BeNullOrEmpty

            $item.GetType().Name | Should Be "GenericSensorTarget"
        }

        it "specifies an invalid table name" {
            { $device | Get-SensorTarget -RawType exexml -Table blah } | Should Throw "Cannot find any tables named 'blah'. Available tables: 'exefile'."
        }

        It "filters returned EXE files" {
            $item = @($device | Get-SensorTarget -RawType exexml *test*)

            $item.Count | Should Be 1

            $item.Name | Should Be "testScript.bat"

            $nothing = $device | Get-SensorTarget -RawType "exexml" "fake_prtgapi_item"

            $nothing | Should BeNullOrEmpty
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

            $device | Get-SensorTarget -RawType snmplibrary -qt $target
        }

        It "parses a sensor query target wildcard" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40), # Get initial target from wildcard

                [Request]::SensorTypes(40) # Verify specified target
                [Request]::BeginAddSensorQuery(40, "snmplibrary_nolist", "APC+UPS.oidlib")
                [Request]::AddSensorProgress(40, 2)
                [Request]::EndAddSensorQuery(40, 2)
            )

            $device | Get-SensorTarget -RawType snmplibrary -qt *ups*
        }

        It "throws when a sensor query target wildcard does not match" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target from wildcard
            )

            { $device | Get-SensorTarget -RawType snmplibrary -qt *potato* } | Should Throw "Could not find a query target matching the wildcard expression '*potato*'. Please specify one of the following parameters: 'APC UPS.oidlib',"
        }

        It "throws when a sensor query target wildcard is ambiguous" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target from wildcard
            )

            { $device | Get-SensorTarget -RawType snmplibrary -qt *apc* } | Should Throw "Query target wildcard '*apc*' is ambiguous between the following parameters: 'APC UPS.oidlib', 'APCSensorstationlib.oidlib'. Please specify a more specific identifier."
        }

        It "throws when a sensor query target is invalid" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40) # Get initial target from wildcard
            )

            { $device | Get-SensorTarget -RawType snmplibrary -qt potato } | Should Throw "Query target 'potato' is not a valid target for sensor type 'snmplibrary' on device ID 40. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "throws when target is not required" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40)
            )

            { $device | Get-SensorTarget -RawType ptfadsreplfailurexml -qt potato } | Should Throw "Cannot specify query target 'potato' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
        }

        It "throws when target is missing" {
            SetCustomAddressValidatorResponse "SensorQueryTargetValidatorResponse" @(
                [Request]::SensorTypes(40)
            )

            { $device | Get-SensorTarget -RawType snmplibrary } | Should Throw "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "parses a set of sensor query target parameters" {
            SetCustomAddressValidatorResponse "SensorQueryTargetParametersValidatorResponse" @(
                [Request]::SensorTypes(40)
                [Request]::BeginAddSensorQuery(40, "ptfadsreplfailurexml")
                [Request]::ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0")
                [Request]::AddSensorProgress(40, 7)
                [Request]::EndAddSensorQuery(40, 7)
            )

            $device | Get-SensorTarget -RawType ptfadsreplfailurexml -qp @{
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

            { $device | Get-SensorTarget -RawType ptfadsreplfailurexml -qt *ups* } | Should Throw "Cannot specify query target '*ups*' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
        }

        It "throws when a sensor query target parameter is missing" {
            SetCustomAddressValidatorResponse "SensorQueryTargetParametersValidatorResponse" @(
                [Request]::SensorTypes(40)
                [Request]::BeginAddSensorQuery(40, "ptfadsreplfailurexml")
            )

            { $device | Get-SensorTarget -RawType ptfadsreplfailurexml } | Should Throw "Failed to process request for sensor type 'oracletablespace': sensor query target parameters are required, however none were specified. Please retry the request specifying the parameters 'database_',"
        }
    }

    It "specifies a timeout" {

        SetResponseAndClient "ExeFileTargetResponse"

        $files = $device | Get-SensorTarget ExeXml -Timeout 3

        $files.Count | Should BeGreaterThan 1
    }

    It "retrieves targets from an unsupported sensor type" {
        { $device | Get-SensorTarget Http } | Should Throw "Sensor type 'Http' is not currently supported"
    }

    It "specifies a list of values" {
        SetResponseAndClient "WmiServiceTargetResponse"

        $targets = $device | Get-SensorTarget WmiService prtgcoreservice,prtgprobeservice
        $targets.Count | Should Be 2

        $targets[0].Name | Should Be "PRTGCoreService"
        $targets[1].Name | Should Be "PRTGProbeService"
    }
}