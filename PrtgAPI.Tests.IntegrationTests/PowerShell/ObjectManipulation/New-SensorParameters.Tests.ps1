. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "New-SensorParameters_IT" -Tag @("PowerShell", "IntegrationTest") {

    Context "Query Target" {
        It "parses a sensor query target" {
            { Get-Device -Id (Settings Device) | New-SensorParameters -RawType snmplibrary -Timeout 3 -qt "APC UPS.oidlib" } | Should Throw "Failed to retrieve sensor information within a reasonable period of time"
        }

        It "throws when a sensor query target is invalid" {
            $id = (Settings Device)

            { Get-Device -Id $id | New-SensorParameters -RawType snmplibrary -Timeout 3 -qt test } | Should Throw "Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID $id. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "throws when a sensor query target is not required" {
            { Get-Device -Id (Settings Device) | New-SensorParameters -RawType exexml -qt "test" } | Should Throw "Cannot specify query target 'test' on sensor type 'exexml': type does not support query targets."
        }

        It "throws when sensor query target is missing" {
            { Get-Device -Id (Settings Device) | New-SensorParameters -RawType snmplibrary } | Should Throw "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
        }

        It "parses a set of sensor query target parameters" {
            { Get-Device -Id (Settings Device) | New-SensorParameters -RawType oracletablespace -qp @{
                database = "XE"
                sid_type = 0
                prefix = 0
            } } | Should Throw "Specified sensor type may not be valid on this device"
        }

        It "throws when sensor query target parameters are not specified" {
            { Get-Device -Id (Settings Device) | New-SensorParameters -RawType oracletablespace -qp @{} } | Should Throw "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_', 'sid_type_', 'prefix_'."
        }
    }
}