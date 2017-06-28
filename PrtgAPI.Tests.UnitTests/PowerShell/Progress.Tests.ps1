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
        { $result = Get-Progress; throw "`n`nProgress Queue contains more records than expected. Next record is:`n`n$result`n`n" } | Should Throw "Queue empty"
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
			"PRTG Sensor Search`n" +
			"    Detecting total number of items"

			"PRTG Sensor Search`n" +
			"    Processing sensor 0/1"

			"PRTG Sensor Search`n" +
			"    Processing sensor 1/1`n" +
			"    Percent Complete: 100"

			"Pausing PRTG Objects`n" +
			"    Pausing sensor 'Volume IO _Total' forever (1/1)`n" +
			"    Percent Complete: 100"

			"Pausing PRTG Objects (Completed)`n" +
			"    Pausing sensor 'Volume IO _Total' forever (1/1)`n" +
			"    Percent Complete: 100"
		))
	}
	
	It "It pipes from a variable" {

		$probes = Run "Probe" {
			$obj1 = GetItem
			$obj2 = GetItem

			WithItems ($obj1, $obj2) {
				Get-Probe
			}
		}

		$probes.Count | Should Be 2

        $probes | Get-Sensor

        Validate (@(
            "PRTG Sensor Search`n" +
            "    Processing all probes 1/2`n" +
            "    Percent Complete: 50`n" +
            "    Retrieving all sensors"

            "PRTG Sensor Search`n" +
            "    Processing all probes 2/2`n" +
            "    Percent Complete: 100`n" +
            "    Retrieving all sensors"

			"PRTG Sensor Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    Percent Complete: 100`n" +
            "    Retrieving all sensors"
        ))
	}

	It "Doesn't show progress when a variable contains only 1 object" {
		$probe = Get-Probe

		$sensors = $probe | Get-Sensor

		{ Get-Progress } | Should Throw "Queue empty"
	}
	
	It "Pipes from a variable into two cmdlets" {
		$probes = Run "Probe" {
			$obj1 = GetItem
			$obj2 = GetItem

			WithItems ($obj1, $obj2) {
				Get-Probe
			}
		}

		$probes.Count | Should Be 2

		$probes | Get-Device | Get-Sensor

		Validate(@(
			"PRTG Device Search`n" +
			"    Processing all probes 1/2`n" +
			"    Percent Complete: 50`n" +
			"    Retrieving all devices"

			"PRTG Device Search`n" +
			"    Processing all probes 1/2`n" +
			"    Percent Complete: 50`n" +
			"`n" +
			"    PRTG Sensor Search`n" +
			"        Processing all devices 1/2`n" +
			"        Percent Complete: 50`n" +
			"        Retrieving all sensors"
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

	<#It "Pipes from a data cmdlet to an action cmdlet to a data cmdlet" {
		throw
	}

	It "Pipes from a variable to an action cmdlet to a data cmdlet" {
		throw
	}#>
}