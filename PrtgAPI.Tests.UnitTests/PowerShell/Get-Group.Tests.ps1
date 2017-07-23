. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-Group" {

	It "can deserialize" {
		$groups = Get-Group
		$groups.Count | Should Be 1
	}

	It "can filter valid wildcards" {
		$obj1 = GetItem
		$obj2 = GetItem

		$obj2.Tags = "testbananatest"

		WithItems ($obj1, $obj2) {
			$groups = Get-Group -Tags *banana*
			$groups.Count | Should Be 1
		}
	}

	It "can ignore invalid wildcards" {
		$obj1 = GetItem
		$obj2 = GetItem

		$obj2.Tags = "testbananatest"

		WithItems ($obj1, $obj2) {
			$groups = Get-Group -Tags *apple*
			$groups.Count | Should Be 0
		}
	}
}