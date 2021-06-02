gci Function:/Describe -ErrorAction SilentlyContinue|where Version -EQ $null|Remove-Item

function Startup($type)
{
    InitializeUnitTestModules
    $global:tester = SetState $type $null
}

function InitializeUnitTestModules
{
    InitializeModules "PrtgAPI.Tests.UnitTests" $PSScriptRoot

    $accelerators = [PowerShell].Assembly.GetType("System.Management.Automation.TypeAccelerators")
    $accelerators::Add("Request", [PrtgAPI.Tests.UnitTests.Support.UnitRequest])
    $accelerators::Add("UrlFlag", [PrtgAPI.Tests.UnitTests.Support.UrlFlag])
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

function ImportModules
{
    param(
        $testProjectName, # e.g. PrtgAPI.Tests.PowerShell
        $scriptRoot       # e.g. C:\PrtgAPI\PrtgAPI.Tests.UnitTests\Support\PowerShell
    )

    $analysis = AnalyzeTestProject $testProjectName $scriptRoot

    $validCandidates = ReduceCandidates $analysis.Candidates

    $selectedCandidate = $validCandidates|Sort-Object LastWriteTime -Descending | select -First 1

    Import-Module $selectedCandidate.PrtgAPIPath -Global
    Import-Module $selectedCandidate.TestProjectDll
}

function AnalyzeTestProject($testProjectName, $scriptRoot)
{
    $solutionFolderEndIndex = $scriptRoot.ToLower().IndexOf($testProjectName.ToLower())

    if($solutionFolderEndIndex -eq -1)
    {
        throw "Could not identify solution folder"
    }

    $solutionFolder = $scriptRoot.Substring(0, $solutionFolderEndIndex)                                  # e.g. C:\PrtgAPI\
    $testProjectFolder = $scriptRoot.Substring(0, $solutionFolderEndIndex + $testProjectName.Length + 1) # e.g. C:\PrtgAPI\PrtgAPI.Tests.UnitTests

    $unitTestFolderCandidates = gci (Join-Path $testProjectFolder "bin") -Recurse "$testProjectName.dll" # e.g. get all folders containing PrtgAPI.Tests.UnitTests.dll

    $candidates = @()

    Write-Verbose "Enumerating build candidates"

    Write-Verbose "############################################################################"

    foreach($candidate in $unitTestFolderCandidates)
    {
        $obj = [PSCustomObject]@{
            Folder             = $candidate.Directory                                          # e.g. Debug (2015) or net461 (2017)
            FolderSuffix       = $candidate.DirectoryName.Substring($testProjectFolder.Length) # e.g. bin\Debug (2015) or bin\Debug\net461 (2017)
            TargetFramework    = $null                                                         # e.g. $null (2015) or net461 (2017)
            TestProjectDll     = Join-Path $candidate.DirectoryName "$testProjectName.dll"
            FolderPath         = $candidate.DirectoryName                                      # e.g. C:\PrtgAPI\PrtgAPI.Tests.UnitTests\bin\Debug\net461
            Configuration      = $null                                                         # e.g. Debug
            Edition            = $null                                                         # e.g. Desktop or Core
            LastWriteTime      = $candidate.LastWriteTime
            PrtgAPIPath        = $null
        }

        if($obj.Folder.Name.StartsWith("net"))
        {
            $obj.Configuration = $obj.Folder.Parent.Name

            # No point supporting .NET Standard as we're looking for unit test projects - project is either '
            if($obj.Folder.Name.StartsWith("netcore"))
            {
                $obj.Edition = "Core"
            }
            else
            {
                $obj.Edition = "Desktop"
            }
        }
        else
        {
            $obj.Configuration = $obj.Folder.Name
            $obj.Edition = "Desktop"
        }

        $suffix = $obj.FolderSuffix
        $targetFramework = Split-Path $suffix -Leaf

        if($targetFramework -notlike "net*")
        {
            $targetFramework = $null
        }

        $obj.PrtgAPIPath = Join-PathEx $solutionFolder "PrtgAPI.PowerShell" $suffix "PrtgAPI"
        $obj.TargetFramework = $targetFramework

        foreach($property in $obj.PSObject.Properties)
        {
            Write-Verbose "$($property.Name): $($obj.$($property.Name))"
        }

        Write-Verbose "############################################################################"

        #Write-Verbose $obj

        $candidates += $obj
    }

    $analysis = [PSCustomObject]@{
        SolutionDir = $solutionFolder
        TestProjectDir = $testProjectFolder
        Candidates = $candidates
    }

    Write-Verbose "SolutionDir: $solutionFolder"
    Write-Verbose "TestProjectDir: $testProjectFolder"

    Write-Verbose "############################################################################"

    return $analysis
}

function ReduceCandidates($candidates)
{
    $newCandidates = $candidates| where {
        if(!(Test-Path $_.PrtgAPIPath))
        {
            $alternatePrtgAPIPath = GetAlternatePrtgAPIPath $_

            if($alternatePrtgAPIPath -eq $null)
            {
                Write-Verbose "Eliminating candidate '$($_.TestProjectDll)' as folder '$($_.PrtgAPIPath)' does not exist"
                return $false
            }

            $_.PrtgAPIPath = $alternatePrtgAPIPath
        }

        $dll = Join-Path $_.PrtgAPIPath "PrtgAPI.PowerShell.dll"

        if(!(Test-Path $dll))
        {
            Write-Verbose "Eliminating candidate DLL '$dll' as file does not exist"
            return $false
        }

        if($PSEdition -ne $_.Edition)
        {
            Write-Verbose "Eliminating candidate '$($_.TestProjectDll)' as candidate edition '$($_.Edition)' does not match required edition '$PSEdition'"
            return $false
        }

        return $true
    }

    if(!$newCandidates)
    {
        $extendedError = $null

        if ($PSEdition -eq "Core")
        {
            $extendedError = " Note that PrtgAPI only builds test projects for .NET Core on Windows when building as Release."
        }

        throw "Could not find any valid build candidates for PowerShell $($PSEdition).$extendedError"
    }

    return $newCandidates
}

function GetAlternatePrtgAPIPath($candidate)
{
    if($candidate.TargetFramework -eq $null)
    {
        return $null
    }

    # e.g. C:\PrtgAPI\PrtgAPI.PowerShell\bin\Debug
    $outputDir = $candidate.PrtgAPIPath.Substring(0, $candidate.PrtgAPIPath.LastIndexOf($candidate.Configuration) + $candidate.Configuration.Length)

    $alternateCandidates = gci $outputDir -Filter "net*" | where PSIsContainer -eq $true | select -ExpandProperty Name

    $existsScriptBlock = {
        $path = Join-PathEx $outputDir $_ "PrtgAPI" "PrtgAPI.PowerShell.dll"

        return Test-Path $path
    }

    $selectedCandidate = $null

    if($candidate.TargetFramework -like "net4*")
    {
        # The unit test was built for .NET Framework. We'll accept another
        # .NET Framework version (preferred) or a .NET Standard version

        $fullCandidates = $alternateCandidates | where { $_ -like "net4*" } | where $existsScriptBlock

        if($fullCandidates)
        {
            $selectedCandidate = $fullCandidates | select -first 1
        }
        else
        {
            $standardCandidates = $alternateCandidates | where { $_ -like "netstandard*" } | where $existsScriptBlock

            if($standardCandidates)
            {
                $selectedCandidate = $standardCandidates | select -first 1
            }
        }
    }
    elseif($candidate.TargetFramework -like "netcoreapp*")
    {
        # The unit test was built for .NET Core. We'll accept another
        # .NET Core version (preferred) or a .NET Standard version

        $fullCandidates = $alternateCandidates | where { $_ -like "netcoreapp*" } | where $existsScriptBlock

        if($fullCandidates)
        {
            $selectedCandidate = $fullCandidates | select -first 1
        }
        else
        {
            $standardCandidates = $alternateCandidates | where { $_ -like "netstandard*" } | where $existsScriptBlock

            if($standardCandidates)
            {
                $selectedCandidate = $standardCandidates | select -first 1
            }
        }
    }
    elseif($candidate.TargetFramework -like "netstandard*")
    {
        throw "Unit test projects can't target .NET Standard?"
    }

    if($selectedCandidate)
    {
        return $candidate.PrtgAPIPath -replace $candidate.TargetFramework,$selectedCandidate
    }

    return $null
}

function global:Join-PathEx
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string[]]$Path,

        [Parameter(Mandatory = $true, Position = 1)]
        [string]$ChildPath,

        [Parameter(Mandatory = $false, Position = 2, ValueFromRemainingArguments = $true)]
        [string[]]$AdditionalChildPath
    )

    foreach($v in $Path)
    {
        $p = Join-Path $Path $ChildPath

        if($AdditionalChildPath)
        {
            foreach($cp in $AdditionalChildPath)
            {
                $p = Join-Path $p $cp
            }
        }

        $p
    }
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

function SetPrtgClient($client)
{
    $assembly = (gcm Disconnect-PrtgServer).ImplementingType.Assembly

    $sessionType = $assembly.GetType("PrtgAPI.PowerShell.PrtgSessionState")
    $editionType = $assembly.GetType("PrtgAPI.PowerShell.PSEdition")

    $edition = [enum]::GetValues($editionType) | where { $_ -eq $PSEdition }

    $flags = [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::NonPublic

    $clientProperty = $sessionType.GetProperty("Client", $flags)
    $editionProperty = $sessionType.GetProperty("PSEdition", $flags)

    $clientProperty.SetValue($null, $client)
    $editionProperty.SetValue($null, $edition)
}
