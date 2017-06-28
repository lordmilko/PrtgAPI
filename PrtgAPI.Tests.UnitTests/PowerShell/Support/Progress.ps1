. $PSScriptRoot\UnitTest.ps1

function Clear-Progress {

	$val = [PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress.ProgressQueue]::ProgressSnapshots

	if($val -ne $null)
	{
		$val.Clear()
	}	
}

function Describe($name, $script)
{
	Pester\Describe $name {

		BeforeAll {	InitializeUnitTestModules }
		AfterEach {
			[PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress.ProgressQueue]::RecordQueue.Clear()
			Clear-Progress
		}

		& $script
	}
}