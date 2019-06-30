$global:ErrorActionPreference = "Stop"
$ErrorActionPreference = "Stop"

. $PSScriptRoot\Helpers\Import-ModuleFunctions.ps1
. Import-ModuleFunctions "$PSScriptRoot\Helpers"
. Import-ModuleFunctions "$PSScriptRoot\CI"

function Get-MSBuild
{
    Install-CIDependency vswhere

    $msbuild = vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1

    if(!(Test-Path $msbuild))
    {
        $msbuild = "C:\Program Files (x86)\MSBuild\14.0\bin\amd64\msbuild.exe"

        if(!(Test-Path $msbuild))
        {
            throw "Could not find a standalone version of MSBuild or a version included with Visual Studio"
        }
    }

    return $msbuild
}

function Get-VSTest
{
    Install-CIDependency vswhere

    $path = vswhere -latest -products * -requires Microsoft.VisualStudio.Workload.ManagedDesktop Microsoft.VisualStudio.Workload.Web -requiresAny -property installationPath

    if(!(Test-Path $path))
    {
        $path = "C:\Program Files (x86)\Microsoft Visual Studio 14.0"

        if(!(Test-Path $path))
        {
            throw "Could not find vstest.console.exe"
        }
    }

    $vstest = Join-Path $path "Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

    return $vstest
}

function Test-IsWindows
{
    if($PSEdition -ne $null -and $PSEdition -ne "Desktop")
    {
        if($IsWindows)
        {
           return $true 
        }
    }
    else
    {
        return $true
    }

    return $false
}

function Invoke-MSBuild($msbuildArgs)
{
    $msbuild = Get-MSBuild

    Write-Verbose "Executing $msbuild $msbuildArgs"

    Invoke-Process {
        & $msbuild $msbuildArgs
    } -WriteHost
}

function Invoke-VSTest($vstestArgs)
{
    $vstest = Get-VSTest

    Write-Verbose "Executing $vstest $vstestArgs"

    Invoke-Process {
        & $vstest $vstestArgs
    } -WriteHost
}

function Get-UnitTestProject($IsCore)
{
    $folder = "PrtgAPI.Tests.UnitTests"
    $csproj = "PrtgAPI.Tests.UnitTests.csproj"
    $powerShell = Join-Path $folder "PowerShell"

    if($IsCore)
    {
        $csproj = "PrtgAPIv17.Tests.UnitTests.csproj"
    }

    return [PSCustomObject]@{
        CSProj = Join-Path $folder $csproj
        Directory = $folder
        PowerShell = $powerShell
    }
}

function Get-BuildProject($IsCore)
{
    $prefix = "PrtgAPI"

    if($IsCore)
    {
        $prefix = "PrtgAPIv17."
    }
    else
    {
        $prefix = "PrtgAPI."
    }

    $root = Get-SolutionRoot

    gci $root -Recurse -Filter "$prefix*csproj"
}

function Get-PowerShellOutputDir
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        $BuildFolder,

        [Parameter(Mandatory = $true)]
        $Configuration,

        [Parameter(Mandatory = $true)]
        $IsCore
    )

    $base = "$BuildFolder\PrtgAPI.PowerShell\bin\$Configuration\"

    if($IsCore)
    {
        # Get the lowest .NET Framework folder
        $candidates = gci (Join-Path $base "net4*")

        if(!$candidates)
        {
            # Could not find a build for .NET Framework. Maybe we're building .NET Core instead

            $candidates = gci "$base\netcore*"
        }

        $fullName = $candidates | select -First 1 -Expand FullName

        return "$fullName\PrtgAPI"
    }
    else
    {
        return "$base\PrtgAPI"
    }
}

function Move-Packages
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        $Suffix,

        [Parameter(Mandatory = $true, Position = 1)]
        $DestinationFolder
    )

    $pkgs = Get-ChildItem ([PackageManager]::RepoLocation) -Filter *.*nupkg
        
    foreach($pkg in $pkgs)
    {
        $newName = "$($pkg.BaseName)$suffix$($pkg.Extension)"
        $newPath = Join-Path $DestinationFolder $newName

        Write-LogInfo "`t`t`t`tMoving package '$($pkg.Name)' to '$newPath'"
        Move-Item $pkg.Fullname $newPath -Force

        gi $newPath
    }
}

$exports = @(
    "Get-MSBuild"
    "Test-IsWindows"
    "Get-UnitTestProject"
    "Import-ModuleFunctions"
    "Invoke-CICSharpTest"
    "Invoke-CIPowerShellTest"
    "Get-BuildProject"
    "New-CSharpPackage"
    "New-PowerShellPackage"
    "New-PackageManager"
    "Get-PowerShellOutputDir"
    "Move-Packages"
)

Export-ModuleMember $exports