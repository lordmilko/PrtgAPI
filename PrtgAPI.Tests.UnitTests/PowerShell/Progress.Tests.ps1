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

function It1($a, $b)
{

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
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			"Pausing PRTG Objects`n" +
			"    Pausing sensor 'Volume IO _Total' forever (1/1)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

			"Pausing PRTG Objects (Completed)`n" +
			"    Pausing sensor 'Volume IO _Total' forever (1/1)`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}
	
	It "It pipes from a variable" {

		$probes = Get-Probe

		$probes.Count | Should Be 2

        $probes | Get-Sensor

        Validate (@(
            "PRTG Sensor Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            "    Retrieving all sensors"

            "PRTG Sensor Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            "    Retrieving all sensors"

			"PRTG Sensor Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            "    Retrieving all sensors"
        ))
	}

	It "Doesn't show progress when a variable contains only 1 object" {
		$probe = Get-Probe -Count 1

		$probe.Count | Should Be 1

		$sensors = $probe | Get-Sensor

		{ Get-Progress } | Should Throw "Queue empty"
	}
	
	It "Pipes from a variable into two table cmdlets" {
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

	It1 "Pipes from a variable into three cmdlets" {
		$probes = Get-Probe

		$probes | Get-Group | Get-Device | Get-Sensor

		throw
	}

	It "Pipes from a variable into an object cmdlet" {

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

	It "Pipes from a variable into a table cmdlet into an object cmdlet" {
		$probes = Get-Probe

		$probes | Get-Sensor | Get-Channel

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

	It "Pipes three data cmdlets together" {

		Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

		Validate (@(
			"PRTG Group Search`n" +
	        "    Retrieving all groups"

			###################################################################

			"PRTG Group Search`n" +
			"    Processing group 0/1"

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
			"        Processing device 0/1"

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

	It "Pipes from a data cmdlet to an action cmdlet to a data cmdlet" {

		Get-Device | Clone-Device 5678 -Resolve | Get-Sensor

		Validate (@(
			"PRTG Device Search`n" +
			"    Retrieving all devices"

			###################################################################

			"PRTG Device Search`n" +
			"    Processing device 0/2"

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

	It1 "Pipes from a variable to an action cmdlet to a data cmdlet" {

		$devices = Get-Device

		$devices | Clone-Device 5678 -Resolve | Get-Sensor

		Validate(@(
			#DONT DO ANY MORE BEFORE A. CHANGING ALL IT1 BACK TO IT AND MAKING SURE THEY RUN, AND B. MAKING SOME COMMITS
		))
	}

	It1 "Pipes from a variable to a data cmdlet to an action cmdlet to a data cmdlet" {
		throw
	}

	It "Shows progress on streamable cmdlets" {
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

			"PRTG Sensor Search`n" +
			"    Retrieving all sensors 0/501"

			$records

			"PRTG Sensor Search (Completed)`n" +
			"    Retrieving all sensors 501/501`n" +
			"    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
		))
	}

	It1 "Doesn't show stream-like progress when a streamable cmdlet is piped into another cmdlet" {
		throw
	}
}