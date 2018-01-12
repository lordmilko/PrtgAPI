function Startup($type)
{
    InitializeUnitTestModules
    $global:tester = SetState $type $null
}

function InitializeUnitTestModules
{
    InitializeModules "PrtgAPI.Tests.UnitTests" $PSScriptRoot
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
        ImportModules $testProject $scriptRoot
    }
}

function ImportModules($testProject, $scriptRoot)
{
    # Get Root Folder

    $root = $testProject
    $rootIndex = $scriptRoot.ToLower().IndexOf($root.ToLower())

    if($rootIndex -eq -1)
    {
        throw "Could not identity root folder"
    }

    $rootFolder = $scriptRoot.Substring(0, $rootIndex)      # e.g. C:\PrtgAPI

    # Get Test Folder

    $testIndex = $scriptRoot.IndexOf("\", $rootIndex) + 1
    $testFolder = $scriptRoot.Substring(0, $testIndex)      # e.g. C:\PrtgAPI\PrtgAPI.Tests.UnitTests

    # Test Project Name

    $testProject = $scriptRoot.Substring($rootIndex, $testIndex - $rootIndex - 1)

    # Get Configuration Name

    $directory = gci ($testFolder + "bin\") -Recurse "$testProject.dll"|sort lastwritetime -Descending|select -first 1 -expand Directory

    $testModuleDir = $null

    if($directory.Name.StartsWith("PrtgAPI"))
    {
        $testModuleDir = $directory
        $directory = $directory.Parent
    }

    $configuration = $directory|select -expand Name

    # Get Module Path

    $module = $rootFolder + "PrtgAPI\bin\" + $configuration + "\PrtgAPI"
    
    if($testModuleDir -ne $null)
    {
        $directory = $testModuleDir
    }
    
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
