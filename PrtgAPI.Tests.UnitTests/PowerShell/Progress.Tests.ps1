. $PSScriptRoot\Support\Progress.ps1

function Get-Progress {
	return [PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress.ProgressQueue]::Dequeue()
}

function Validate($list)	{

	foreach($progress in $list)
	{
		Get-Progress | Should Be $progress
	}

	try
	{
		{ Get-Progress } | Should Throw "Queue empty"
	}
	catch [exception]
	{
		Clear-Progress
		throw
	}
}

function InitializeClient {
	[PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.MockProgressWriter]::Bind()

	$client = [PrtgAPI.Tests.UnitTests.ObjectTests.BaseTest]::Initialize_Client((New-Object PrtgAPI.Tests.UnitTests.ObjectTests.Responses.MultiTypeResponse))

	SetPrtgClient $client

	Enable-PrtgProgress
}

Describe "Test-Progress" {
	
	InitializeClient

	It "Pauses an object" {
		Get-Sensor | Pause-Object -Forever

		Validate (@(
			""
		))
	}

	It "Chains three data cmdlets together" {

		Get-Group | Get-Device | Get-Sensor

		Validate (@(
			"PRTG Group Search`n" +
	        "    Retrieving all groups"

			"PRTG Group Search`n" +
			"    Processing group 0/1"

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    Percent Complete: 100"

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    Percent Complete: 100`n" +
			"    Retrieving all devices"

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    Percent Complete: 100`n" +
			"`n" +
			"    PRTG Device Search`n" +
			"        Processing device 0/1"

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    Percent Complete: 100`n" +
			"`n" +
			"    PRTG Device Search`n" +
			"        Processing device 1/1`n" +
			"        Percent Complete: 100"

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    Percent Complete: 100`n" +
			"`n" +
			"    PRTG Device Search`n" +
			"        Processing device 1/1`n" +
			"        Percent Complete: 100`n" +
			"        Retrieving all sensors"

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    Percent Complete: 100`n" +
			"`n" +
			"    PRTG Device Search (Completed)`n" +
			"        Processing device 1/1`n" +
			"        Percent Complete: 100`n" +
			"        Retrieving all sensors"

			"PRTG Group Search (Completed)`n" +
			"    Processing group 1/1`n" +
			"    Percent Complete: 100"
		))
	}
}