#region Initialization

function Startup($type)
{
	InitializeModules
	$global:tester = SetState $type $null
}

function Shutdown
{
	$global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$null)
	$global:tester = $null
}

function InitializeModules
{
	$modules = Get-Module prtgapi,prtgapi.tests.unittests

	if($modules.Count -ne 2)
	{
		ImportModules
	}
}

function ImportModules
{
    # Get Root Folder

    $root = "\PrtgAPI\"
    $rootIndex = $PSScriptRoot.ToLower().IndexOf($root.ToLower()) + $root.Length
    $rootFolder = $PSScriptRoot.Substring(0, $rootIndex)

    # Get Test Folder

    $testIndex = $PSScriptRoot.IndexOf("\", $rootIndex) + 1
    $testFolder = $PSScriptRoot.Substring(0, $testIndex)

    # Test Project Name

    $testProject = $PSScriptRoot.Substring($rootIndex, $testIndex - $rootIndex - 1)

    # Get Configuration Name

    $directory = gci ($testFolder + "bin\") -Recurse "$testProject.dll"|sort lastwritetime -Descending|select -first 1 -expand Directory
    $configuration = $directory|select -expand Name

    # Get Module Path

    $module = $rootFolder + "PrtgAPI\bin\" + $configuration + "\PrtgAPI"
    $testModule = $directory.FullName + "\" + $testProject + ".dll"
    
    import-module $module
    import-module $testModule
}

function SetState($objectType, $items)
{
	$tester = $null

	if(!$items)
	{
		$tester = (New-Object PrtgAPI.Tests.UnitTests.ObjectTests.$($objectType)Tests)
	}
	else
	{
		$tester = New-Object "PrtgAPI.Tests.UnitTests.ObjectTests.$($objectType)Tests" -ArgumentList ($items)
	}
	
	$tester.SetPrtgSessionState()

	return $tester
}

#endregion

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