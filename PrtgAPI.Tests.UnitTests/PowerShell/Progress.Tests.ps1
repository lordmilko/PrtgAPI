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

function RunCustomCount($hashtable, $action)
{
	$dictionary = GetCustomCountDictionary $hashtable

	$oldClient = Get-PrtgClient

	$newClient = [PrtgAPI.Tests.UnitTests.ObjectTests.BaseTest]::Initialize_Client((New-Object PrtgAPI.Tests.UnitTests.ObjectTests.Responses.MultiTypeResponse -ArgumentList $dictionary))

	try
	{
		SetPrtgClient $newClient

		& $action
	}
	catch
	{
		throw
	}
	finally
	{
		SetPrtgClient $oldClient
	}
}

function GetCustomCountDictionary($hashtable)
{
	$dictionary = New-Object "System.Collections.Generic.Dictionary[[PrtgAPI.Content],[int]]"

	foreach($entry in $hashtable.GetEnumerator())
	{
		$newKey = $entry.Key -as "PrtgAPI.Content"

		$dictionary.Add($newKey, $entry.Value)
	}

	return $dictionary
}

function ItWorks($a, $b)
{
	It $a $b
}

function ItsNotImplemented($a, $b)
{
	#It $a { throw }
	#todo: add some tests for having some other cmdlet at the start of the pipeline. need to modify getpipelineinput to handle this properly, e.g. what if Where-Object is at the start or middle?
}

Describe "Test-Progress" {
	
	InitializeClient

	#region 1: Something -> Action
	
	ItWorks "1a: Table -> Action" {
		Get-Sensor -Count 1 | Pause-Object -Forever

		Validate (@(
			"PRTG Sensor Search`n" +
			"    Retrieving all sensors"

			"PRTG Sensor Search`n" +
			"    Processing sensor 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			"Pausing PRTG Objects`n" +
			"    Pausing sensor 'Volume IO _Total' forever (1/1)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			"Pausing PRTG Objects (Completed)`n" +
			"    Pausing sensor 'Volume IO _Total' forever (1/1)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}
	
	ItWorks "1b: Variable -> Action" {
		$devices = Get-Device

		$devices.Count | Should Be 2

		$devices | Pause-Object -Forever

		Validate (@(
			"Pausing PRTG Objects`n" +
			"    Pausing device 'Probe Device' forever (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"Pausing PRTG Objects`n" +
			"    Pausing device 'Probe Device' forever (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"Pausing PRTG Objects (Completed)`n" +
			"    Pausing device 'Probe Device' forever (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	#endregion
	#region 2: Something -> Table

	ItWorks "2a: Table -> Table" {
		Get-Probe | Get-Group

		Validate(@(
			"PRTG Probe Search`n" +
			"    Retrieving all probes"

			###################################################################

			"PRTG Probe Search`n" +
			"    Processing probe 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"PRTG Probe Search`n" +
			"    Processing probe 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all groups"

			###################################################################

			"PRTG Probe Search`n" +
			"    Processing probe 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all groups"

			###################################################################

			"PRTG Probe Search (Completed)`n" +
			"    Processing probe 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all groups"
		))
	}

	ItWorks "2b: Variable -> Table" {

		$probes = Get-Probe

		$probes.Count | Should Be 2

        $probes | Get-Sensor

        Validate (@(
            "PRTG Sensor Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

			###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

			###################################################################

			"PRTG Sensor Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"
        ))
	}

	#endregion
	#region 3: Something -> Action -> Table
	
	ItWorks "3a: Table -> Action -> Table" {

		Get-Device | Clone-Device 5678 | Get-Sensor

		Validate (@(
			"PRTG Device Search`n" +
			"    Retrieving all devices"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Processing device 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"Cloning PRTG Devices (Completed)`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	ItWorks "3b: Variable -> Action -> Table" {

		$devices = Get-Device

		$devices | Clone-Device 5678 | Get-Sensor

		Validate(@(
			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"Cloning PRTG Devices (Completed)`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all sensors"
		))
	}

	#endregion
	#region 4: Something -> Table -> Table

	ItWorks "4a: Table -> Table -> Table" {

		Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

		Validate (@(
			"PRTG Group Search`n" +
	        "    Retrieving all groups"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Device Search`n" +
			"        Processing device 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Device Search`n" +
			"        Processing device 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			"        Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Device Search (Completed)`n" +
			"        Processing device 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			"        Retrieving all sensors"

			###################################################################

			"PRTG Group Search (Completed)`n" +
			"    Processing group 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}
	
	ItWorks "4b: Variable -> Table -> Table" {
		$probes = Get-Probe

		#we need to find a way to detect if the entire chain we're piping along
		#originated with a variable. maybe look at the first progressrecord to see if its dodgy
		#or can we potentially dig back along the pipeline to the entry

		$probes.Count | Should Be 2

		$probes | Get-Device | Get-Sensor

		Validate(@(
			"PRTG Device Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all devices"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +
			
			"    PRTG Sensor Search`n" +
			"        Processing all devices 1/2`n" +
			"        [oooooooooooooooooooo                    ] (50%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing all devices 2/2`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search (Completed)`n" +
			"        Processing all devices 2/2`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all devices"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing all devices 1/2`n" +
			"        [oooooooooooooooooooo                    ] (50%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing all devices 2/2`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search (Completed)`n" +
			"        Processing all devices 2/2`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Device Search (Completed)`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	#endregion
	#region 5: Something -> Table -> Action -> Table

	ItWorks "5a: Table -> Table -> Action -> Table" {
		Get-Group | Get-Device | Clone-Device 5678 | Get-Sensor

		Validate(@(
			"PRTG Group Search`n" +
			"    Retrieving all groups"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Device Search`n" +
			"        Processing device 1/2`n" +
			"        [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"        [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"        [oooooooooooooooooooo                    ] (50%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Processing device 2/2`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Devices (Completed)`n" +
			"        Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Device Search`n" +
			"        Processing device 1/2`n" +
			"        [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"        [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"        [oooooooooooooooooooo                    ] (50%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Processing device 2/2`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Devices`n" +
			"        Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Devices (Completed)`n" +
			"        Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search (Completed)`n" +
			"    Processing group 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	ItWorks "5b: Variable -> Table -> Action -> Table" {
		$probes = Get-Probe

		$probes | Get-Group -Count 1 | Clone-Group 5678 | Get-Device

		Validate(@(
			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all groups"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Groups`n" +
			"        Cloning group 'Windows Infrastructure' (ID: 2211) (1/1)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Groups`n" +
			"        Cloning group 'Windows Infrastructure' (ID: 2211) (1/1)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Cloning PRTG Groups (Completed)`n" +
			"        Cloning group 'Windows Infrastructure' (ID: 2211) (1/1)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all groups"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Groups`n" +
			"        Cloning group 'Windows Infrastructure' (ID: 2211) (1/1)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Groups`n" +
			"        Cloning group 'Windows Infrastructure' (ID: 2211) (1/1)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Cloning PRTG Groups (Completed)`n" +
			"        Cloning group 'Windows Infrastructure' (ID: 2211) (1/1)`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all devices"

			###################################################################

			"PRTG Group Search (Completed)`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	#endregion
	#region 6: Something -> Object

	ItWorks "6a: Table -> Object" {
		Get-Sensor -Count 1 | Get-Channel

		Validate(@(
			"PRTG Sensor Search`n" +
			"    Retrieving all sensors"

			###################################################################

			"PRTG Sensor Search`n" +
			"    Processing sensor 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Sensor Search`n" +
			"    Processing sensor 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all channels"

			###################################################################

			"PRTG Sensor Search (Completed)`n" +
			"    Processing sensor 1/1`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all channels"
		))
	}

	ItWorks "6b: Variable -> Object" {

		#1. why is pipes three data cmdlets together being infected by the crash here
		#2. why is injected_showchart failing to deserialize?

		$result = Run "Sensor" {

			$obj1 = GetItem
			$obj2 = GetItem

			WithItems ($obj1, $obj2) {
				Get-Sensor -Count 2
			}
		}

		$result.Count | Should Be 2
		$result | Get-Channel

		Validate(@(
			"PRTG Channel Search`n" +
			"    Processing all sensors 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +
			"    Retrieving all channels"

			"PRTG Channel Search`n" +
			"    Processing all sensors 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			"    Retrieving all channels"

			"PRTG Channel Search (Completed)`n" +
			"    Processing all sensors 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			"    Retrieving all channels"
		))
	}

	#endregion
	#region 7: Stream -> Something

	ItWorks "7a: Stream -> Object" {
		# Good enough for a test to Stream -> Table as well
		
		$counts = @{
			Sensors = 501
		}

		RunCustomCount $counts {
			Get-Sensor | Get-Channel
		}

		$records = @()
		$total = 501

		# Create progress records for processing each object

		for($i = 1; $i -le $total; $i++)
		{
			$maxChars = 40

			$percent = [Math]::Floor($i/$total*100)

			if($percent -ge 0)
			{
				$percentChars = $percent/100*$maxChars

				$spaceChars = $maxChars - $percentChars

				$percentBar = ""

				for($j = 0; $j -lt $percentChars; $j++)
				{
					$percentBar += "o"
				}

				for($j = 0; $j -lt $spaceChars; $j++)
				{
					$percentBar += " "
				}

				$percentBar = "[$percentBar] ($percent%)"
			}

			$records += "PRTG Sensor Search`n" +
						"    Processing sensor $i/$total`n" +
						"    $percentBar`n" +
						"    Retrieving all channels"
		}

		Validate(@(
			"PRTG Sensor Search`n" +
			"    Detecting total number of items"

			"PRTG Sensor Search`n" +
			"    Processing sensor 1/501`n" +
			"    [                                        ] (0%)"

			$records

			"PRTG Sensor Search (Completed)`n" +
			"    Processing sensor 501/501`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			"    Retrieving all channels"
		))
	}

	ItWorks "7b: Stream -> Action" {

		# Besides the initial "Detecting total number of items", there is nothing special about a streamed, non-streamed and streaming-unsupported (e.g. devices) run

		$counts = @{
			Sensors = 501
		}

		RunCustomCount $counts {
			Get-Sensor | Pause-Object -Forever
		}

		$records = @()
		$total = 501

		# Create progress records for processing each object

		for($i = 1; $i -le $total; $i++)
		{
			$maxChars = 40

			$percent = [Math]::Floor($i/$total*100)

			if($percent -ge 0)
			{
				$percentChars = $percent/100*$maxChars

				$spaceChars = $maxChars - $percentChars

				$percentBar = ""

				for($j = 0; $j -lt $percentChars; $j++)
				{
					$percentBar += "o"
				}

				for($j = 0; $j -lt $spaceChars; $j++)
				{
					$percentBar += " "
				}

				$percentBar = "[$percentBar] ($percent%)"
			}

			if($i -gt 1)
			{
				$records += "Pausing PRTG Objects`n" +
							"    Processing sensor $i/$total`n" +
							"    $percentBar"
			}

			$records += "Pausing PRTG Objects`n" +
						"    Pausing sensor 'Volume IO _Total' forever ($i/$total)`n" +
						"    $percentBar"
		}

		#todo: maybe try replace the original stream one with this

		Validate(@(
			"PRTG Sensor Search`n" +
			"    Detecting total number of items"

			"PRTG Sensor Search`n" +
			"    Processing sensor 1/501`n" +
			"    [                                        ] (0%)"

			$records

			"Pausing PRTG Objects (Completed)`n" +
			"    Pausing sensor 'Volume IO _Total' forever (501/501)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	#endregion
	#region 8: Something -> Table -> Object

	ItWorks "8a: Table -> Table -> Object" {

		$counts = @{
			Sensors = 1
		}

		RunCustomCount $counts {
			Get-Device | Get-Sensor | Get-Channel
		}

		Validate(@(
			"PRTG Device Search`n" +
			"    Retrieving all devices"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search (Completed)`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search (Completed)`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Device Search (Completed)`n" +
			"    Processing device 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	ItWorks "8b: Variable -> Table -> Object" {
		$probes = Get-Probe

		$counts = @{
			Sensors = 1
		}

		RunCustomCount $counts {
			$probes | Get-Sensor | Get-Channel
		}

		Validate (@(
			"PRTG Sensor Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"PRTG Sensor Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Channel Search`n" +
			"        Processing all sensors 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Sensor Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Channel Search (Completed)`n" +
			"        Processing all sensors 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Sensor Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"PRTG Sensor Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Channel Search`n" +
			"        Processing all sensors 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Sensor Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Channel Search (Completed)`n" +
			"        Processing all sensors 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"PRTG Sensor Search (Completed)`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	#endregion
	#region 9: Variable -> Action -> Table -> Table

	ItWorks "9: Variable -> Action -> Table -> Table" {
		# an extension of 3b. variable -> action -> table. Confirms that we can transform our setpreviousoperation into a
		# proper progress item when required

		$devices = Get-Device

		$devices | Clone-Device 5678 | Get-Sensor | Get-Channel		

		Validate(@(
			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (1/2)`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +

			"    PRTG Sensor Search (Completed)`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    Retrieving all sensors"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"Cloning PRTG Devices`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"    PRTG Sensor Search (Completed)`n" +
			"        Processing sensor 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all channels"

			###################################################################

			"Cloning PRTG Devices (Completed)`n" +
			"    Cloning device 'Probe Device' (ID: 40) (2/2)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	#endregion
	#region 10: Variable -> Table -> Table -> Table

	ItWorks "10: Variable -> Table -> Table -> Table" {
		# Validates we can get at least two progress bars out of a variable
		$probes = Get-Probe

		$probes | Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

		Validate(@(
			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +
			
			"    Retrieving all groups"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +
			
			"    PRTG Device Search`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +
			
			"    PRTG Device Search`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        PRTG Sensor Search`n" +
			"            Processing all devices 1/1`n" +
			"            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"            Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +
			
			"    PRTG Device Search`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        PRTG Sensor Search (Completed)`n" +
			"            Processing all devices 1/1`n" +
			"            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"            Retrieving all sensors"

			###################################################################


			"PRTG Group Search`n" +
			"    Processing all probes 1/2`n" +
			"    [oooooooooooooooooooo                    ] (50%)`n" +
			
			"    PRTG Device Search (Completed)`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			
			"    Retrieving all groups"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			
			"    PRTG Device Search`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        Retrieving all devices"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			
			"    PRTG Device Search`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        PRTG Sensor Search`n" +
			"            Processing all devices 1/1`n" +
			"            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"            Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			
			"    PRTG Device Search`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"        PRTG Sensor Search (Completed)`n" +
			"            Processing all devices 1/1`n" +
			"            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

			"            Retrieving all sensors"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
			
			"    PRTG Device Search (Completed)`n" +
			"        Processing all groups 1/1`n" +
			"        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			###################################################################

			"PRTG Group Search (Completed)`n" +
			"    Processing all probes 2/2`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	#endregion
	#region 11: Table -> Filter -> Something

	ItWorks "11a: Table -> Filter -> Table" {
		Get-Probe | Select-Object -First 2 | Get-Device

		{ Get-Progress } | Should Throw "Queue empty"
	}

	ItWorks "11b: Table -> Filter -> Action" {
		Get-Probe | Select-Object -First 2 | Pause-Object -Forever

		{ Get-Progress } | Should Throw "Queue empty"
	}

	#endregion
	#region 12: Variable -> Filter -> Something

	ItWorks "12a: Variable -> Filter -> Table" {
		$probes = Get-Probe

		$probes | Select-Object -First 2 | Get-Device

		{ Get-Progress } | Should Throw "Queue empty"
	}

	ItWorks "12b: Variable -> Filter -> Action" {
		$probes = Get-Probe

		$probes | Select-Object -First 2 | Pause-Object -Forever

		{ Get-Progress } | Should Throw "Queue empty"
	}

	#endregion
	#region 13: Table -> Filter -> Table -> Something

	ItWorks "13a: Table -> Filter -> Table -> Table" {
		Get-Probe | Select-Object -First 2 | Get-Device | Get-Sensor

		{ Get-Progress } | Should Throw "Queue empty"
	}

	ItWorks "13b: Table -> Filter -> Table -> Action" {
		Get-Probe | Select-Object -First 2 | Get-Device | Pause-Object -Forever

		{ Get-Progress } | Should Throw "Queue empty"
	}

	#endregion
	#region 14: Variable -> Filter -> Table -> Something

	ItWorks "14a: Variable -> Filter -> Table -> Table" {
		$probes = Get-Probe

		$probes | Select-Object -First 2 | Get-Device | Get-Sensor

		{ Get-Progress } | Should Throw "Queue empty"
	}

	ItWorks "14b: Variable -> Filter -> Table -> Action" {
		$probes = Get-Probe

		$probes | Select-Object -First 2 | Get-Device | Pause-Object -Forever

		{ Get-Progress } | Should Throw "Queue empty"
	}

	#endregion
	#region Sanity Checks	

	ItWorks "Streams when the number of returned objects is above the threshold" {
		Run "Sensor" {

			$objs = @()

			for($i = 0; $i -lt 501; $i++)
			{
				$objs += GetItem
			}

			WithItems ($objs) {
				$result = Get-Sensor
				$result.Count | Should Be 501
			}
		}

		$records = @()
		$total = 501

		# Create progress records for processing each object

		for($i = 1; $i -le $total; $i++)
		{
			$maxChars = 40

			$percent = [Math]::Floor($i/$total*100)

			if($percent -ge 0)
			{
				$percentChars = $percent/100*$maxChars

				$spaceChars = $maxChars - $percentChars

				$percentBar = ""

				for($j = 0; $j -lt $percentChars; $j++)
				{
					$percentBar += "o"
				}

				for($j = 0; $j -lt $spaceChars; $j++)
				{
					$percentBar += " "
				}

				$percentBar = "[$percentBar] ($percent%)"
			}

			$records += "PRTG Sensor Search`n" +
						"    Retrieving all sensors $i/$total`n" +
						"    $percentBar"
		}

		Validate(@(
			"PRTG Sensor Search`n" +
			"    Detecting total number of items"

			$records

			"PRTG Sensor Search (Completed)`n" +
			"    Retrieving all sensors 501/501`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	ItWorks "Doesn't stream when the number of returned objects is below the threshold" {
		Get-Sensor

		Validate(@(
			"PRTG Sensor Search`n" +
			"    Detecting total number of items"
		))
	}

	ItWorks "Doesn't show progress when a variable contains only 1 object" {
		$probe = Get-Probe -Count 1

		$probe.Count | Should Be 1

		$sensors = $probe | Get-Sensor

		{ Get-Progress } | Should Throw "Queue empty"
	}

	#endregion

    ItsNotImplemented "Variable(1) -> Table -> Table" {

		$probe = Get-Probe -Count 1

		$probe.Count | Should Be 1

		$probe | Get-Group | Get-Device

		Validate(@(

		))

        throw "todo: need to move this to a proper position"
    }

	ItsNotImplemented "blah2" {
		throw "unrelated: when you have a taskcancelledexception, it has a cancellation token which should be true if actually cancelled, false otherwise"
		#should we modify executerequest to check whether the token is true? if we've enabled a whole bunch of retries and try and ctrl+c will it keep retrying?
	}
}