. "$PSScriptRoot\..\..\..\PrtgAPI.Tests.UnitTests\PowerShell\Support\Init.ps1"

function Startup
{
	StartupSafe

	[PrtgAPI.Tests.IntegrationTests.BasePrtgClientTest]::AssemblyInitialize($null)

}

function Log($message, $error = $false)
{
	[PrtgAPI.Tests.IntegrationTests.Logger]::Log($message, $error, "PS")
	Write-Host $message
}

function LogTest($message, $error)
{
	if($error -ne $true)
	{
		$error = $false
	}

	[PrtgAPI.Tests.IntegrationTests.Logger]::LogTest($message, $error, "PS")
	Write-Host $message
}

function LogTestName($message, $error = $false)
{
	[PrtgAPI.Tests.IntegrationTests.Logger]::LogTestDetail($message, $error, "PS")
}

function LogTestDetail($message, $error = $false)
{
	LogTestName "    $message" $error
}

function StartupSafe
{
	Write-Host "Performing startup tasks"
	InitializeModules "PrtgAPI.Tests.IntegrationTests" $PSScriptRoot

	#do, get the dlltestsrunning value, continue as long as its true keep sleeping for 10 seconds, then sleep for another 30 seconds
	#after the end of the loop

	#i think we should change this to dlltestsran, and then if they did ever run we wait 30 seconds or something


	#bug: we're doing assemblycleanup when we havent initialized; this will cause us to delete a preexisting config
	#file that hasnt been deleted

	#need to make dll tests wait for the service to start if powershell tests are run before dll tests
	#todo: need to test we can sort groups in probes/groups, devices in groups with sort-prtgobject
	#we might want to consider hacking the nuspec file thats generated to fix up the cmdlet names? acknowledge-sensor etc
	#can be fixed up too

	if(!(Get-PrtgClient))
	{
		Log "Starting PowerShell Tests"

		try
		{
			Log "Connecting to PRTG Server"
			Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin)
		}
		catch [exception]
		{
			if(!($Global:FirstRun))
			{
				$Global:FirstRun = $true
				Log "Sleeping for 30 seconds as its our first test and we couldn't connect..."
				Sleep 30
				Log "Attempting second connection"

				try
				{
					Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin)

					Log "Connection successful"
				}
				catch [exception]
				{
					Log $_.Exception.Message $true
					throw
				}

				Log "Refreshing all sensors"

				Get-Sensor | Refresh-Object

				Log "Sleeping for 30 seconds"

				Sleep 30
			}
			else
			{
				throw
			}
		}		
	}

	if($global:PreviousTest)
	{
		Log "Sleeping for 30 seconds as tests have run previously"
		Sleep 30

		Log "Refreshing objects"

		Get-Device | Refresh-Object

		Log "Waiting for refresh"
		Sleep 30
	}
	else
	{
		try
		{
			Get-SensorTotals
		}
		catch [exception]
		{
			Log "PRTG service may still be starting up; pausing for 60 seconds"
			Sleep 30
			Get-Sensor | Refresh-Object
			Sleep 30
		}
	}
}

function Shutdown
{
	Log "Performing cleanup tasks"
	[PrtgAPI.Tests.IntegrationTests.BasePrtgClientTest]::AssemblyCleanup()

	$global:PreviousTest = $true
}

function Settings($property)
{
	$val = [PrtgAPI.Tests.IntegrationTests.Settings]::$property

	if($val -eq $null)
	{
		throw "Property '$property' could not be found."
	}

	return $val
}

function It($name, $script) {
	LogTestName "Running test '$name'"

	Pester\It $name {

		try
		{
			& $script
		}
		catch [exception]
		{
			LogTestDetail ($_.Exception.Message -replace "`n"," ") $true
			throw
		}

		
	}
}