function Invoke-CITest
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        $BuildFolder,

        [Parameter(Position = 1)]
        $AdditionalArgs,

        [Parameter(Mandatory = $false)]
        $Configuration = $env:CONFIGURATION,

        [Parameter(Mandatory = $true)]
        [switch]$IsCore
    )

    Invoke-CIPowerShellTest $BuildFolder $AdditionalArgs -IsCore:$IsCore
    Invoke-CICSharpTest $BuildFolder $AdditionalArgs $Configuration -IsCore:$IsCore
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

        [Parameter(Mandatory = $true)]
        [switch]$IsCore,

        [Parameter(Mandatory = $false)]
        [switch]$Integration
    )

    Write-LogInfo "`tExecuting C# tests"

    $testProjectDetails = Get-TestProject $IsCore $Integration

    if($IsCore)
    {
        $csproj = Join-Path $BuildFolder $testProjectDetails.CSProj
        Write-Verbose "Using csproj '$csproj'"

        Invoke-CICSharpTestCore $csproj $Configuration $AdditionalArgs
    }
    else
    {
        $ch = [IO.Path]::DirectorySeparatorChar

        $dll = Join-Path $BuildFolder "$($testProjectDetails.Directory)\bin\$Configuration\$($testProjectDetails.Directory.Replace("src$ch",'')).dll"
        Write-Verbose "Using DLL '$dll'"

        Invoke-CICSharpTestFull $dll $BuildFolder $Configuration $AdditionalArgs
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

    Install-CIDependency dotnet

    Write-Verbose "Executing command 'dotnet $dotnetTestArgs'"

    Invoke-Process { & "dotnet" @dotnetTestArgs } -WriteHost
}

function Invoke-CICSharpTestFull($dll, $BuildFolder, $Configuration, $AdditionalArgs)
{
    $vsTestArgs = @(
        $dll
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

        [Parameter(Mandatory = $true)]
        [switch]$IsCore,

        [Parameter(Mandatory = $false)]
        [switch]$Integration
    )

    Write-LogInfo "`tExecuting PowerShell tests"

    $relativePath = (Get-TestProject $IsCore $Integration).PowerShell

    $directory = Join-Path $BuildFolder $relativePath

    Install-CIDependency Pester

    Invoke-Pester $directory -PassThru @AdditionalArgs
}