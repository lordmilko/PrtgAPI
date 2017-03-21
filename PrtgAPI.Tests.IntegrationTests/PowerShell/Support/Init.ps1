. "$PSScriptRoot\..\..\..\PrtgAPI.Tests.UnitTests\PowerShell\Support\Init.ps1"

function Startup
{
	StartupSafe

	if($global:PreviousTest)
	{
		Sleep 30

		Write-Host "Refreshing objects"

		Get-Device | Refresh-Object

		Sleep 30
	}

	[PrtgAPI.Tests.IntegrationTests.BasePrtgClientTest]::AssemblyInitialize($null)

}

function StartupSafe
{
	Write-Host "Performing startup tasks"
	InitializeModules "PrtgAPI.Tests.IntegrationTests" $PSScriptRoot

	if(!(Get-PrtgClient))
	{
		Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin)
	}
}

function Shutdown
{
	Write-Host "Performing cleanup tasks"
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



