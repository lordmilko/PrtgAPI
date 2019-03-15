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