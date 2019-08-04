. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-SearchFilter" -Tag @("PowerShell", "UnitTest") {
    Context "Enum Transformation" {

        It "transforms a string to an enum" {

            SetAddressValidatorResponse "filter_condition=0"

            flt Condition eq Disconnected | Get-Probe
        }

        It "transforms a string containing a space to an enum" {

            SetAddressValidatorResponse "filter_type=aggregation"

            flt type eq "sensor factory" | Get-Sensor
        }

        It "transforms a string to an enum via its raw property" {

            SetAddressValidatorResponse "filter_type=aggregation"

            flt type eq "sensor factory" | Get-Sensor
        }

        It "leaves an unapplicable property" {

            SetAddressValidatorResponse "filter_dstart=ping"

            flt StartDate eq ping | Get-Sensor
        }

        It "leaves a non enum value" {

            SetAddressValidatorResponse "filter_name=ping"

            flt name eq ping | Get-Sensor
        }
    }
}