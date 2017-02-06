function Startup($type)
{
	InitializeModules "prtgapi.tests.unittests" $PSScriptRoot
	$global:tester = SetState $type $null
}

function Shutdown
{
	$global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$null)
	$global:tester = $null
}

function InitializeModules($testProject, $scriptRoot)
{
	$modules = Get-Module prtgapi,$testProject
	
	if($modules.Count -ne 2)
	{
		ImportModules $scriptRoot
	}
}

function ImportModules($scriptRoot)
{
    # Get Root Folder

    $root = "\PrtgAPI\"
    $rootIndex = $scriptRoot.ToLower().IndexOf($root.ToLower()) + $root.Length
    $rootFolder = $scriptRoot.Substring(0, $rootIndex)

    # Get Test Folder

    $testIndex = $scriptRoot.IndexOf("\", $rootIndex) + 1
    $testFolder = $scriptRoot.Substring(0, $testIndex)

    # Test Project Name

    $testProject = $scriptRoot.Substring($rootIndex, $testIndex - $rootIndex - 1)

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
