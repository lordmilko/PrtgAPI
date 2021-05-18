. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-Probe" -Tag @("PowerShell", "UnitTest") {

    It "can deserialize" {
        $probes = Get-Probe
        $probes.Count | Should Be 1
    }

    It "can filter by status" {
        $items = (GetItem),(GetItem),(GetItem),(GetItem)
        $items.Count | Should Be 4

        $items[0].StatusRaw = "5" # Down
        $items[1].StatusRaw = "3" # Up
        $items[2].StatusRaw = "3" # Up
        $items[3].StatusRaw = "8" # PausedByDependency

        WithItems $items {
            $probes = Get-Probe -Status Up,Paused

            $probes.Count | Should Be 3
        }
    }

    It "can filter valid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $probes = Get-Probe -Tags *banana*
            $probes.Count | Should Be 1
        }
    }

    It "can ignore invalid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $probes = Get-Probe -Tags *apple*
            $probes.Count | Should Be 0
        }
    }

    It "filters by probe status" {
        WithResponseArgs "AddressValidatorResponse" "filter_condition=2" {
            Get-Probe -ProbeStatus Connected
        }
    }

    It "allows filtering by ParentId 0" {
        flt parentid eq 0 | Get-Probe
    }

    Context "Dynamic" {
        It "uses dynamic parameters" {
            SetAddressValidatorResponse "filter_position=0000000030"

            Get-Probe -Position 3
        }

        It "throws using a dynamic parameter not supported by this type" {
            { Get-Probe -Host dc-1 } | Should Throw "A parameter cannot be found that matches parameter name 'Host'"
        }

        It "uses dynamic parameters in conjunction with regular parameters" {

            SetAddressValidatorResponse "filter_name=@sub(probe)&filter_objid=3&filter_parentid=0"

            Get-Probe *probe* -Id 3 -ParentId 0
        }

        It "uses wildcards with a dynamic parameter" {
            
            SetAddressValidatorResponse "filter_message=@sub(1)"

            $probe = @(Get-Probe -Count 3 -Message "*1")

            $probe.Count | Should Be 1

            $probe.Name | Should Be "127.0.0.11"
        }

        It "uses a bool with a dynamic parameter" {

            SetAddressValidatorResponse "filter_active=-1"

            Get-Probe -Active $true
        }
    }

    It "removes non probes from API responses" {
        $obj1 = GetItem
        $obj2 = GetItem
        $obj3 = GetItem

        $obj1.Name = "First"
        $obj2.Name = "Second"
        $obj3.TypeRaw = "AutonomousDevice"

        WithItems ($obj1, $obj2, $obj3) {
            $probes = Get-Probe

            $probes.Count | Should Be 2
        }
    }

    It "throws specifying a ParentId other than 0" {
        { flt parentid eq -1 | Get-Probe } | Should Throw "Cannot filter for probes based on a ParentId other than 0"
    }
}
