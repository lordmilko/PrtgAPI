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

    $global:ErrorActionPreference = "Stop"
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

    $module = $rootFolder + "PrtgAPI.PowerShell\bin\" + $configuration + "\PrtgAPI"
    
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
        $tester = (New-Object PrtgAPI.Tests.UnitTests.ObjectData.$($objectType)Tests)
    }
    else
    {
        $tester = New-Object "PrtgAPI.Tests.UnitTests.ObjectData.$($objectType)Tests" -ArgumentList ($items)
    }
    
    $tester.SetPrtgSessionState()

    return $tester
}

function GetSensorTypeContexts($filePath, $allowEnhancedDescription)
{
    $contextNames = GetScriptContexts $filePath | foreach { $_.ToLower() }

    $sensorTypes = [enum]::GetNames([PrtgAPI.SensorType]) | foreach { $_.ToLower() }

    $excludedTypes = @("SqlServerDb") | foreach { $_.ToLower() }

    $sensorTypes = $sensorTypes|where { $_ -notin $excludedTypes }

    $missingTypes = $null

    if($allowEnhancedDescription)
    {
        $missingTypes = @()

        foreach($type in $sensorTypes)
        {
            $found = $false

            foreach($context in $contextNames)
            {
                if($context -eq $type -or $context -like "$($type):*")
                {
                    $found = $true
                    break
                }
            }

            if(!$found)
            {
                $missingTypes += $type
            }
        }
    }
    else
    {
        $missingTypes = $sensorTypes|where { $_ -notin $contextNames }
    }

    if($missingTypes)
    {
        $str = $missingTypes -join ", "

        throw "Missing contexts/tests for the following sensor types: $str"
    }
}

function GetScriptContexts($filePath)
{
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $filePath,
        [ref]$null,
        [ref]$null
    )

    $commands = $ast.FindAll({ $args[0] -is [System.Management.Automation.Language.CommandAst] }, $true)

    $contexts = $commands|where {
        $_.CommandElements.Count -ge 2 -and $_.CommandElements[0].Value -eq "Context"
    }

    $contextNames = $contexts | foreach {
        $_.FindAll({ $args[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -and $args[0].StringConstantType -ne "BareWord" }, $false) | select -ExpandProperty Value
    }

    return $contextNames
}
