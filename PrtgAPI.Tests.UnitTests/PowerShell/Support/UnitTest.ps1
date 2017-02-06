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

	& $assert

	$global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)
}

function Run($objectType, $script)
{
	$oldClient = Get-PrtgClient
	$global:tester = (New-Object PrtgAPI.Tests.UnitTests.ObjectTests.$($objectType)Tests)
	$global:tester.SetPrtgSessionState()

	$result = & $script

	$global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)

	return $result
}