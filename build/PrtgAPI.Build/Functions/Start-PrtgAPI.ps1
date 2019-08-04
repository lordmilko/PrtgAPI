<#
.SYNOPSIS
Starts a new PowerShell console containing the compiled version of PrtgAPI.

.DESCRIPTION
The Start-PrtgAPI starts starts a previously compiled version of PrtgAPI in a new PowerShell console. By default, Start-PrtgAPI will attempt to launch the last Debug build of PrtgAPI. Builds for .NET Core and .NET Standard will be launched in PowerShell Core, while builds for the .NET Framework will be launched in Windows PowerShell. If builds for multiple target frameworks are detected, Start-PrtgAPI will throw an execption specifying the builds that were found. A specific target framework can be specified to the -TargetFramework parameter.

If -Legacy is true, Start-PrtgAPI will skip enumerating target frameworks and instead attempt to open a build from the legacy .NET Framework version of PrtgAPI in a Windows PowerShell console.

.PARAMETER Configuration
Build configuration to launch. If no configuration is specified, Debug will be used.

.PARAMETER Legacy
Specifies whether to launch PrtgAPI compiled using the .NET Core project or the legacy .NET Framework project.

.PARAMETER TargetFramework
Specifies the target framework to launch when multiple frameworks have been compiled.

.EXAMPLE
C:\> Start-PrtgAPI
Open a PowerShell console containing the only target framework that has been compiled

.EXAMPLE
C:\> Start-PrtgAPI -TargetFramework net461
Open a PowerShell console containing the version of PrtgAPI compiled for the target framework 'net461'

.LINK
Invoke-PrtgBuild
#>
function Start-PrtgAPI
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [Configuration]$Configuration = "Debug",

        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy,

        [Parameter(Mandatory = $false)]
        [string]$TargetFramework
    )

    $path = GetModulePath $Configuration (-not $Legacy) $TargetFramework

    if(Test-Path $path)
    {
        $exe = GetPowerShellExe $path (-not $Legacy)

        $psd1 = Join-PathEx $path PrtgAPI PrtgAPI.psd1

        if(Test-IsWindows)
        {
            Write-Host -ForegroundColor Green "`nLaunching PrtgAPI from '$psd1'`n"

            Start-Process $exe -ArgumentList "-executionpolicy","bypass","-noexit","-command","ipmo $psd1; cd ~"
        }
        else
        {
            # Start-Process can't open new windows on Unix platforms and malfunctions when trying to run a nested PowerShell instance,
            # so just import the module into the current session

            if(gmo prtgapi)
            {
                throw "Cannot start PrtgAPI as PrtgAPI is already loaded in the current session. Please reopen the PrtgAPI Build Environment and try running PrtgAPI again."
            }

            Write-Host -ForegroundColor Green "`nImporting PrtgAPI from $psd1"

            Import-Module $psd1 -Global

            gmo prtgapi
        }
    }
    else
    {
        throw "Cannot start PrtgAPI: solution has not been compiled for '$Configuration' build. Path '$path' does not exist."
    }
}

function GetPowerShellExe($path, $isCore)
{
    if($isCore)
    {
        if($path -notlike "*net4*")
        {
            if(!(gcm pwsh -ErrorAction SilentlyContinue))
            {
                throw "Cannot find command 'pwsh' for launching module '$path'; is PowerShell Core installed?"
            }

            return "pwsh"
        }
    }

    return "powershell"    
}

function GetModulePath($configuration, $IsCore, $targetFramework)
{
    $root = Get-SolutionRoot

    $bin = Join-PathEx $root src PrtgAPI.PowerShell bin

    if($IsCore -and ![string]::IsNullOrWhiteSpace($targetFramework))
    {
        $targetFolder = Join-PathEx $bin $configuration $targetFramework

        if(!(Test-Path $targetFolder))
        {
            if(Test-Path (Join-Path $bin $configuration))
            {
                $candidates = gci (Join-Path $bin $configuration) -Filter "net*"

                if($candidates)
                {
                    $str = ($candidates | select -expand BaseName) -join ", "

                    throw "Cannot start PrtgAPI: target framework '$targetFramework' does not exist. Please ensure PrtgAPI has been compiled for the specified TargetFramework and Configuration. Known target frameworks: $str."
                }
            }

            throw "Cannot start PrtgAPI: target folder '$targetFolder' does not exist. Please ensure PrtgAPI has been compiled for the specified TargetFramework and Configuration."
        }
    }
    else
    {
        $targetFolder = CalculateTargetFolder
    }

    return $targetFolder
}

function CalculateTargetFolder
{
    if($IsCore)
    {
        $configFolder = Join-Path $bin $configuration

        $candidates = gci $configFolder -Filter "net*"

        if(!($candidates))
        {
            throw "Cannot find any build candidates under folder '$configFolder'. Please make sure PrtgAPI has been compiled."
        }

        if($candidates.Count -gt 1)
        {
            $str = ($candidates | select -expand BaseName) -join ", "

            throw "Unable to determine which TargetFramework to use. Please specify one of $str"
        }

        return $candidates.FullName
    }
    else
    {
        return Join-Path $bin $configuration
    }
}