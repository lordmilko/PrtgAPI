function Invoke-CITest
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        $BuildFolder,

        [Parameter(Position = 1)]
        $AdditionalArgs,

        [Parameter(Mandatory = $false)]
        $Configuration = $env:CONFIGURATION
    )

    Invoke-CIPowerShellTest $BuildFolder $AdditionalArgs
    Invoke-CICSharpTest $BuildFolder $AdditionalArgs $Configuration
}

function Invoke-CICSharpTest
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
        [switch]$IsCore = $true
    )

    Write-LogInfo "`tExecuting C# tests"

    $relativePath = (Get-UnitTestProject $IsCore).CSProj

    $csproj = Join-Path $BuildFolder $relativePath

    Write-Verbose "Using csproj '$csproj'"

    if($IsCore)
    {
        Invoke-CICSharpTestCore $csproj $Configuration $AdditionalArgs
    }
    else
    {
        Invoke-CICSharpTestFull $BuildFolder $Configuration $AdditionalArgs
    }
}

function Invoke-CICSharpTestCore($csproj, $Configuration, $AdditionalArgs)
{
    $dotnetTestArgs = @(
        "test"
        $csproj
        "-nologo"
        "--no-restore"
        "--no-build"
        "--verbosity:n"
        "-c"
        $Configuration
    )

    if($AdditionalArgs)
    {
        $dotnetTestArgs += $AdditionalArgs
    }

    Write-Verbose "Executing command 'dotnet $dotnetTestArgs'"

    Invoke-Process { & "dotnet" @dotnetTestArgs } -WriteHost
}

function Invoke-CICSharpTestFull($BuildFolder, $Configuration, $AdditionalArgs)
{
    $vsTestArgs = @(
        Join-Path $BuildFolder "PrtgAPI.Tests.UnitTests\bin\$Configuration\PrtgAPI.Tests.UnitTests.dll"
    )

    if($AdditionalArgs)
    {
        $vsTestArgs += $AdditionalArgs
    }

    $vstest = Get-VSTest

    Write-Verbose "Executing command $vstest $vsTestArgs"

    Invoke-Process {
        & $vstest $vsTestArgs
    } -WriteHost
}

function Invoke-CIPowerShellTest
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        $BuildFolder,
        [Parameter(Position = 1)]
        $AdditionalArgs,

        [Parameter(Mandatory = $false)]
        [switch]$IsCore = $true
    )

    Write-LogInfo "`tExecuting PowerShell tests"

    $relativePath = (Get-UnitTestProject $IsCore).PowerShell

    $directory = Join-Path $BuildFolder $relativePath

    Invoke-Pester $directory -PassThru @AdditionalArgs -ExcludeTag Build
}