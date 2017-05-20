. $PSScriptRoot\Support\UnitTest.ps1

function Describe($name, $script)
{
	Pester\Describe $name {

		BeforeAll {	InitializeUnitTestModules }

		& $script
	}
}

Describe "Test-Reflection" {

	It "can detect next cmdlet on pipeline" {
		Test-Reflection1 -Downstream | Test-Reflection2 -Downstream | Should Be "Test-Reflection2"
	}

	Context "Progress" {
		It "can retrieve progress source ID" {

			Test-Reflection1 -SourceId | Should BeGreaterThan 0
		}

		It "can detect parent source ID in chain of 3" {
			Test-Reflection1 -ChainSourceId | Test-Reflection2 -ChainSourceId | Test-Reflection3 -ChainSourceId | Should Be 4
		}
	}

	Context "Pipeline Input" {
		It "can detect pipeline input from cmdlet" {

			Test-Reflection1 -CmdletInput | Test-Reflection2 -CmdletInput | Should Be 1,2,3
		}

		It "can detect pipeline input from variable array" {
			$value = 1,2,3

			$value | Test-Reflection1 -VariableInputArray | Should Be 1,2,3
		}

		It "can detect pipeline input from variable object" {
			$value = 1

			$value | Test-Reflection1 -VariableInputObject | Should Be 1
		}
	}
}