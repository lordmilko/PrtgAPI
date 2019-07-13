function Invoke-CIBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        $BuildFolder,

        [Parameter(Position = 1)]
        $AdditionalArgs,

        [Parameter(Mandatory = $false, Position = 2)]
        $Configuration = $env:CONFIGURATION,

        [Parameter(Mandatory = $false)]
        $Target,

        [Parameter(Mandatory = $false)]
        [switch]$IsCore,

        [Parameter(Mandatory = $false)]
        [switch]$SourceLink
    )

    if([string]::IsNullOrEmpty($Target))
    {
        if($IsCore)
        {
            $Target = Join-Path $BuildFolder "PrtgAPIv17.sln"
        }
        else
        {
            $Target = Join-Path $BuildFolder "PrtgAPI.sln"
        }
    }

    $innerArgs = @{
        BuildFolder = $BuildFolder
        AdditionalArgs = $AdditionalArgs
        Configuration = $Configuration
        Target = $Target
        SourceLink = $SourceLink
    }

    if($IsCore)
    {
        Invoke-CIBuildCore @innerArgs
    }
    else
    {
        Invoke-CIBuildFull @innerArgs
    }
}

function Invoke-CIBuildCore
{
    param(
        $BuildFolder,
        $AdditionalArgs,
        $Configuration,
        $Target,
        $SourceLink
    )

    $dotnetBuildArgs = @(
        "build"
        $Target
        "-nologo"
        "-c"
        "$Configuration"
    )

    if($SourceLink)
    {
        $dotnetBuildArgs += "-p:EnableSourceLink=true"
    }

    if($AdditionalArgs)
    {
        $dotnetBuildArgs += $AdditionalArgs
    }

    Install-CIDependency dotnet

    if(Test-IsWindows)
    {
        Install-CIDependency net452,net461
    }

    Write-Verbose "Executing command 'dotnet $dotnetBuildArgs'"

    Invoke-Process {
        dotnet @dotnetBuildArgs
    } -WriteHost
}

function Invoke-CIBuildFull
{
    param(
        $BuildFolder,
        $AdditionalArgs,
        $Configuration,
        $Target
    )

    $msbuild = Get-MSBuild

    $msbuildArgs = @(
        $Target
        "/verbosity:minimal"
        "/p:Configuration=$Configuration"
    )

    if($AdditionalArgs)
    {
        $msbuildArgs += $AdditionalArgs
    }

    Write-Verbose "Executing command '$msbuild $msbuildArgs'"

    Invoke-Process {
        & $msbuild @msbuildArgs
    } -WriteHost
}

function Invoke-CIRestoreFull
{
    [CmdletBinding()]
    param($root)

    Install-CIDependency nuget

    $nuget = Get-ChocolateyCommand nuget
    $sln = Join-Path $root "PrtgAPI.sln"

    $nugetArgs = @(
        "restore"
        $sln
    )

    Write-Verbose "Executing command '$nuget $nugetArgs'"

    Invoke-Process { & $nuget $nugetArgs } -WriteHost
}