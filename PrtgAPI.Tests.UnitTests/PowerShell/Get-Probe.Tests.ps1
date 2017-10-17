. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-Probe" {

    It "can deserialize" {
        $probes = Get-Probe
        $probes.Count | Should Be 1
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
}
