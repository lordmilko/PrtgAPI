function Get-CIVersion
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [switch]$IsCore = $true
    )

    Get-CallerPreference $PSCmdlet $ExecutionContext.SessionState

    $root = Get-SolutionRoot

    $versionTable = Get-CIVersionInternal $root $IsCore

    Validate-VersionTable $versionTable

    return $versionTable
}

function Get-CIVersionInternal($buildFolder, $IsCore)
{
    $package = $null
    $assembly = $null
    $file = $null

    if($IsCore)
    {
        $versionPath = Join-Path $buildFolder "build\Version.props"

        if(!(Test-Path $versionPath))
        {
            throw "Cannot find file '$versionPath' required for .NET Core versioning."
        }

        $versionProps = ([xml](gc $versionPath)).Project.PropertyGroup

        $package = $versionProps.Version
        $assembly = $versionProps.AssemblyVersion
        $file = $versionProps.FileVersion
        $info = $versionProps.InformationalVersion
    }
    else
    {
        $versionPath = Join-Path $buildFolder "src\PrtgAPI\Properties\Version.cs"

        if(!(Test-Path $versionPath))
        {
            throw "Cannot find file '$versionPath' required for .NET Framework versioning."
        }

        $versionContents = gc $versionPath

        $assembly = GetAssemblyAttribute $versionContents AssemblyVersion
        $file = GetAssemblyAttribute $versionContents AssemblyFileVersion
        $info = GetAssemblyAttribute $versionContents AssemblyInformationalVersion
        $package = ([Version]$file).ToString(3)
    }

    $psd1Path = Join-Path $BuildFolder "src\PrtgAPI.PowerShell\PowerShell\Resources\PrtgAPI.psd1"

    if(!(Test-Path $psd1Path))
    {
        throw "Cannot find file '$psd1Path' required for PowerShell Module versioning."
    }

    $psd1Contents = gc $psd1Path

    $moduleVersion = ($psd1Contents|sls "ModuleVersion = ") -replace "`n" -replace "ModuleVersion = '(.+?)'",'$1'
    $releaseTag = ($psd1Contents|sls "ReleaseNotes = ") -replace "`n" -replace ".+ReleaseNotes = '.+/tag/(.+?)",'$1'

    $versionTable = [PSCustomObject]@{
        Package = $package
        Assembly = $assembly
        File = $file
        Info = $info
        Module = $moduleVersion
        ModuleTag = $releaseTag
    }

    if((Test-Path (Join-Path $BuildFolder ".git")) -and (gcm "git" -ErrorAction SilentlyContinue))
    {
        $gitTag = GetGitTag $BuildFolder

        $versionTable | Add-Member PreviousTag $gitTag
    }

    return $versionTable
}

function GetAssemblyAttribute($contents, $name)
{
    $matches = @($contents|sls $name)

    if ($matches.Count -eq 0)
    {
        throw "Could not find any matches for attribute '$name' in contents '$contents'"
    }

    if ($matches.Count -gt 1)
    {
        throw "Found $($matches.Count) matches for attribute '$name' in contents '$contents'"
    }

    $result = $matches -replace "`n" -replace ".+$name\(`"(.+?)`"\).+",'$1'

    return $result
}

function GetGitTag($buildFolder)
{
    # Defined in a separate function so we can mock calling git
    return (git -C $buildFolder describe --tags) -replace "(.+?)(-.+)",'$1'
}

function Validate-VersionTable($versionTable)
{
    foreach($property in $versionTable.PSObject.Properties)
    {
        if([string]::IsNullOrWhiteSpace($property.Value))
        {
            throw "Version Property '$($property.Name)' did not have a value."
        }

        $version = $null

        if([Version]::TryParse($property.Value, [ref] $version))
        {
            $property.Value = $version
        }
    }

    $three = $versionTable.File.ToString(3)
    $two = $versionTable.File.ToString(2) + ".0.0"
    $vThree = "v$three"

    $package = $versionTable.Package.ToString()
    $assembly = $versionTable.Assembly.ToString()
    $module = $versionTable.Module.ToString()

    if($package -ne $three)
    {
        Write-Error "Expected property 'Package' to be '$three' but was '$package' instead.`nPackage version should match the first three digits of File version." -Category InvalidData
    }

    if($assembly -ne $two)
    {
        Write-Error "Expected property 'Assembly' to be '$Two' but was '$assembly' instead.`nAssembly version should match the first two digits of File version and consist of four components." -Category InvalidData
    }

    if($module -ne $three)
    {
        Write-Error "Expected property 'Module' to be '$three' but was '$module' instead.`nModule version should match the first three digits of File version." -Category InvalidData
    }

    if($versionTable.ModuleTag -ne $vThree)
    {
        Write-Error "Expected property 'ModuleTag' to be '$vThree' but was '$($versionTable.ModuleTag)' instead.`nModuleTag should start with 'v' followed by the first three digits of File version." -Category InvalidData
    }

    if($versionTable.PreviousTag)
    {
        [Version]$previousTag = $versionTable.PreviousTag.TrimStart("v")
        [Version]$moduleTag = $versionTable.ModuleTag.TrimStart("v")

        if($moduleTag -lt $previousTag)
        {
            Write-Error "Module tag '$($versionTable.ModuleTag)' should be greater than or equal to previous release tag '$($versionTable.PreviousTag)'."
        }
    }
}