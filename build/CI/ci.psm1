$global:ErrorActionPreference = "Stop"
$ErrorActionPreference = "Stop"

. $PSScriptRoot\Helpers\Import-ModuleFunctions.ps1
. Import-ModuleFunctions "$PSScriptRoot\Helpers"
. Import-ModuleFunctions "$PSScriptRoot\CI"

function Get-MSBuild
{
    Install-CIDependency vswhere

    $vswhere = Get-ChocolateyCommand vswhere

    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1

    if([string]::IsNullOrEmpty($msbuild) -or !(Test-Path $msbuild))
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

    $vswhere = Get-ChocolateyCommand vswhere

    $path = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Workload.ManagedDesktop Microsoft.VisualStudio.Workload.Web -requiresAny -property installationPath

    if([string]::IsNullOrEmpty($path) -or !(Test-Path $path))
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
    Test-CIIsWindows
}

function Test-CIIsWindows
{
    # Pester 3 freaks out if you mock the same command twice for two different modules; as such
    # we mock a single command Test-CIIsWindows; all modules call Test-IsWindows, which forwards
    # the request to the internal implementation

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

function Get-TestProject($IsCore, $integration = $false)
{
    $name = "Unit"

    if($integration)
    {
        $name = "Integration"
    }

    $folder = Join-Path "src" "PrtgAPI.Tests.$($name)Tests"
    $csproj = "PrtgAPI.Tests.$($name)Tests.csproj"
    $powerShell = Join-Path $folder "PowerShell"

    if($IsCore)
    {
        $csproj = "PrtgAPIv17.Tests.$($name)Tests.csproj"
    }

    return [PSCustomObject]@{
        CSProj = Join-Path $folder $csproj
        Directory = $folder
        PowerShell = $powerShell
    }
}

function Get-BuildProject($IsCore)
{
    $root = Get-SolutionRoot
    $projects = gci $root -Recurse -Filter "*.csproj"

    if($IsCore)
    {
        $projects = $projects | where { $_.Name -notlike "PrtgAPI.*"}
    }
    else
    {
        $projects = $projects | where { $_.Name -notlike "PrtgAPIv17.*"}
    }

    return $projects
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

    $base = Join-Path $BuildFolder "src\PrtgAPI.PowerShell\bin\$Configuration\"

    if($IsCore)
    {
        # Get the lowest .NET Framework folder
        $candidates = gci (Join-Path $base "net4*")

        if(!$candidates)
        {
            # Could not find a build for .NET Framework. Maybe we're building .NET Core instead

            $candidates = gci (Join-Path $base "netcore*")
        }

        if(!$candidates)
        {
            $candidates = gci (Join-Path $base "netstandard*")
        }

        $fullName = $candidates | select -First 1 -Expand FullName

        if(!$fullName)
        {
            throw "Couldn't find any Core $Configuration build candidates for PrtgAPI.PowerShell"
        }

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

    $pkgs = (Get-ChildItem (PackageManager -RepoLocation) -Filter *.*nupkg)
        
    foreach($pkg in $pkgs)
    {
        $newName = "$($pkg.BaseName)$suffix$($pkg.Extension)"
        $newPath = Join-Path $DestinationFolder $newName

        Write-LogInfo "`t`t`t`tMoving package '$($pkg.Name)' to '$newPath'"
        Move-Item $pkg.Fullname $newPath -Force

        gi $newPath
    }

    $zips = Get-ChildItem (PackageManager -RepoLocation) -Filter *.zip

    foreach($zip in $zips)
    {
        $newPath = Join-Path $DestinationFolder $zip.Name

        Write-LogInfo "`t`t`t`tMoving package '$($zip.Name)' to '$newPath'"
        Move-Item $zip.FullName $newPath -Force

        gi $newPath
    }
}

function HasType
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, ValueFromPipeline = $true)]
        [string[]]$Actual,

        [Parameter(Mandatory = $true, Position = 0)]
        [string[]]$Desired
    )

    begin {
        $actuals = @()
    }

    process {
        $actuals += $Actual
    }

    end {
        return [string]::IsNullOrEmpty($actuals) -or $actuals.Count -eq 0 -or [bool]($Desired | where { $_ -in $actuals })
    }
}

# Based on https://powershell.org/2014/01/revisited-script-modules-and-variable-scopes/
function Get-CallerPreference
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateScript({ $_.GetType().FullName -eq 'System.Management.Automation.PSScriptCmdlet' })]
        $Cmdlet,

        [Parameter(Mandatory = $true, Position = 1)]
        [System.Management.Automation.SessionState]
        $SessionState,

        [Parameter(Mandatory = $false)]
        [string]$DefaultErrorAction
    )

    $preferences = @{
        'ErrorActionPreference' = 'ErrorAction'
        'DebugPreference' = 'Debug'
        'ConfirmPreference' = 'Confirm'
        'WhatIfPreference' = 'WhatIf'
        'VerbosePreference' = 'Verbose'
        'WarningPreference' = 'WarningAction'
    }

    foreach($preference in $preferences.GetEnumerator())
    {
        # If this preference wasn't specified to our inner cmdlet
        if(!$Cmdlet.MyInvocation.BoundParameters.ContainsKey($preference.Value))
        {
            if($PSCmdlet.MyInvocation.BoundParameters.ContainsKey("Default$($preference.Value)"))
            {
                $variable = [PSCustomObject]@{
                    Name = $preference.Name
                    Value = $PSCmdlet.MyInvocation.BoundParameters["Default$($preference.Value)"]
                }
            }
            else
            {
                # Get the value of this preference from the outer scope
                $variable = $Cmdlet.SessionState.PSVariable.Get($preference.Key)
            }

            # And apply it to our inner scope
            if($null -ne $variable -and $null -ne $variable.Value)
            {
                if ($SessionState -eq $ExecutionContext.SessionState)
                {
                    #todo: what is "scope 1"?
                    Set-Variable -Scope 1 -Name $variable.Name -Value $variable.Value -Force -Confirm:$false -WhatIf:$false
                }
                else
                {
                    $SessionState.PSVariable.Set($variable.Name, $variable.Value)
                }
            }
        }
    }
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

# PowerShell Gallery requires TLS 1.2
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

$exports = @(
    "HasType"
    "New-PackageManager"
    "PackageManager"
    "Get-CallerPreference"
    "Get-ChocolateyCommand"
    "Test-IsWindows"
    "Get-TestProject"
    "Import-ModuleFunctions"
    "Invoke-CIRestoreFull"
    "Invoke-CICSharpTest"
    "Invoke-CIPowerShellTest"
    "Join-PathEx"
    "Get-BuildProject"
    "New-CSharpPackage"
    "New-PowerShellPackage"
    "New-PackageManager"
    "Get-PowerShellOutputDir"
    "Move-Packages"
    "Write-LogHeader"
    "Write-LogSubHeader"
    "Write-LogInfo"
    "Write-LogError"
    "Write-LogVerbose"

    # Exports to prevent Pester 3 from blowing up due to mocks disappearing after being used

    "GetGitTag"
    "Write-Log"
    "Invoke-CIProcess"
    "Test-CIIsWindows"
    "Get-MSBuild"
    "Get-VSTest"
    "Get-CIVersionInternal"
    "Get-PackageSourceEx"
    "Register-PackageSourceEx"
    "Unregister-PackageSourceEx"
    "Get-PSRepositoryEx"
    "Register-PSRepositoryEx"
    "Unregister-PSRepositoryEx"
    "Install-PackageEx"
    "Publish-ModuleEx"
    "Update-RootModule"
)

Export-ModuleMember $exports