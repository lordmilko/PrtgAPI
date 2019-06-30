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
        [switch]$IsCore
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
        $Target
    )

    $dotnetBuildArgs = @(
        "build"
        $Target
        "-p:EnableSourceLink=true"
        "-nologo"
        "-c"
        "$Configuration"
    )

    if($AdditionalArgs)
    {
        $dotnetBuildArgs += $AdditionalArgs
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