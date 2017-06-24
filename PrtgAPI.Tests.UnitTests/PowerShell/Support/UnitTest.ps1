#region Initialization

. $PSScriptRoot\Init.ps1

function Describe($name, $script) {

    Pester\Describe $name {
		BeforeAll { Startup $name.Substring($name.indexof("-") + 1) }
		AfterAll { Shutdown }

		& $script
	}
}

function GetItem
{
	return $global:tester.GetItem()
}

function WithItems($items, $assert)
{
	$oldClient = Get-PrtgClient

	$global:tester.SetPrtgSessionState($items)

	$result = & $assert

	$global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)

	if($result)
	{
		return $result
	}
}

function Run($objectType, $script)
{
	$oldClient = Get-PrtgClient
    $oldTester = $global:tester
	$global:tester = (New-Object PrtgAPI.Tests.UnitTests.ObjectTests.$($objectType)Tests)
	$global:tester.SetPrtgSessionState()

	$result = & $script

	$global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)
    $global:tester = $oldTester

	return $result
}

function SetPrtgClient($client)
{
	$type = [PrtgAPI.PrtgClient].Assembly.GetType("PrtgAPI.PowerShell.PrtgSessionState")
	$property = $type.GetProperty("Client", [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::NonPublic)

	$property.SetValue($null, $client)
}