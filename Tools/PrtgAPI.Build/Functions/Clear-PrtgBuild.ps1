<#
.SYNOPSIS
Clears the output of one or more previous PrtgAPI builds.

.DESCRIPTION
The Clear-PrtgBuild clears the output of previous builds of PrtgAPI. By default, Clear-PrtgBuild will attempt to use the appropriate build tool (msbuild or dotnet.exe) to clear the previous build. If If -Full is specified, Clear-PrtgBuild will will instead force remove the bin and obj folders of each project in the solution.

.PARAMETER Configuration
Configuration to clean. If no value is specified PrtgAPI will clean the last Debug build.

.PARAMETER Legacy
Specifies whether to use legacy .NET tooling to clear the build.

.PARAMETER Full
Specifies whether to brute force remove all build and object files in the solution.

.EXAMPLE
C:\> Clear-PrtgBuild
Clear the last build of PrtgAPI

.EXAMPLE
C:\> Clear-PrtgBuild -Full
Remove all obj and bin folders under each project of PrtgAPI

.LINK
Invoke-PrtgBuild
#>
function Clear-PrtgBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet("Debug", "Release")]
        $Configuration = "Debug",

        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy,

        [Parameter(Mandatory = $false)]
        [switch]$Full
    )

    $devenv = Get-Process devenv -ErrorAction SilentlyContinue

    if($devenv)
    {
        Write-LogError "Warning: Visual Studio is currently running. Some items may not be able to be removed"
    }

    $root = Get-SolutionRoot

    $binLog = Join-Path $root "msbuild.binlog"

    if(Test-Path $binLog)
    {
        Remove-Item $binLog -Force
    }

    if($Full)
    {
        $projects = gci $root -Recurse -Filter *.csproj

        foreach($project in $projects)
        {
            Write-LogInfo "Processing $project"

            $folder = Split-Path $project.FullName -Parent

            $bin = Join-Path $folder "bin"
            $obj = Join-Path $folder "obj"

            if(Test-Path $bin)
            {
                Write-LogError "`tRemoving $bin"
                RemoveItems $bin
            }

            if(Test-Path $obj)
            {
                # obj will be automatically recreated and removed each time Clear-PrtgBuild is run,
                # due to dotnet/msbuild clean recreating it
                Write-LogError "`tRemoving $obj"
                RemoveItems $obj
            }
        }

        Write-LogInfo "Processing Redistributable Packages"

        $clearArgs = @{
            BuildFolder = $root
            Configuration = $Configuration
            IsCore = -not $Legacy
            NuGetOnly = $true
        }

        Clear-CIBuild @clearArgs
    }
    else
    {
        $clearArgs = @{
            BuildFolder = $root
            Configuration = $Configuration
            IsCore = -not $Legacy
        }

        Clear-CIBuild @clearArgs -Verbose
    }
}

function RemoveItems($folder)
{
    $items = gci $folder -Recurse

    $files = $items | where { !$_.PSIsContainer }

    foreach($file in $files)
    {
        Write-LogError "`t`tRemoving '$file'"

        $file | Remove-Item -Force
    }

    $folders = $items | where { $_.PSIsContainer }

    foreach($f in $folders)
    {
        if(Test-Path $f)
        {
            Write-LogError "`t`tRemoving '$f'"

            $f | Remove-Item -Force -Recurse
        }
    }

    $folder | Remove-Item -Force -Recurse
}