<#
.SYNOPSIS
Creates NuGet packages from PrtgAPI for distribution

.DESCRIPTION
The New-PrtgPackage generates NuGet packages from PrtgAPI for distribution within a NuGet package management system. By default, packages will be built using the last Debug build for both the C# and PowerShell versions of PrtgAPI using .NET Core SDK tooling (where applicable). Packages can be built for a specific project type by specifying a value to the -Type parameter. Upon generating a package, a FileInfo object will be emitted to the pipeline indicating the name and path to the generated package.

Unlike packaging done in CI builds, New-PrtgPackage does not verify that the contents of the generated package are correct.

.PARAMETER Type
Type of packages to create. By default C# and PowerShell packages as well as a redistributable zip file are created.

.PARAMETER Configuration
Configuration to pack. If no value is specified, the last Debug build will be packed.

.PARAMETER Legacy
Specifies whether to pack the .NET Core version of PrtgAPI or the legacy .NET Framework version.

.EXAMPLE
C:\> New-PrtgPackage
Create NuGet packages for both C# and PowerShell

.EXAMPLE
C:\> New-PrtgPackage -Type PowerShell
Create NuGet packages for PowerShell only

.EXAMPLE
C:\> New-PrtgPackage -Configuration Release
Create Release NuGet packages for both C# and PowerShell

.LINK
Invoke-PrtgBuild
#>
function New-PrtgPackage
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet('C#', 'PowerShell', 'Redist')]
        [string[]]$Type,

        [Parameter(Mandatory=$false)]
        [Configuration]$Configuration = "Debug",

        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy
    )

    if(!(Test-IsWindows) -and $Configuration -eq "Release")
    {
        throw "Release packages can only be created on Windows."
    }

    $root = Get-SolutionRoot

    $manager = New-PackageManager

    if($Type | HasType "C#")
    {
        $created = $true

        if(!(Test-Path (PackageManager -RepoLocation)))
        {
            New-Item (PackageManager -RepoLocation) -ItemType Directory -Force | Out-Null
            $created = $true
        }
        else
        {
            gci (PackageManager -RepoLocation) -Recurse | Remove-Item -Force
        }

        $csharpArgs = @{
            BuildFolder = $root
            OutputFolder = (PackageManager -RepoLocation)
            Version = (Get-PrtgVersion -Legacy:$Legacy -ErrorAction Stop).Package
            Configuration = $Configuration
            IsCore = -not $Legacy
        }

        Write-PrtgProgress "New-PrtgPackage" "Creating C# Package"

        New-CSharpPackage @csharpArgs -Verbose

        Move-Packages "" $root

        if($created)
        {
            Remove-Item (PackageManager -RepoLocation) -Force
        }
    }

    if($Type | HasType "PowerShell","Redist")
    {
        $text = "PowerShell"

        if(!($Type | HasType "PowerShell"))
        {
            $text = "Redistributable"
        }

        Write-PrtgProgress "New-PrtgPackage" "Creating $text Package" -PercentComplete 50
        
        $manager.InstallPowerShellRepository()

        $binDir = Get-PowerShellOutputDir $root $Configuration (-not $Legacy)

        $powershellArgs = @{
            OutputDir = $binDir
            RepoManager = $manager
            Configuration = $Configuration
            IsCore = -not $Legacy
            Redist = $Type | HasType "Redist"
            PowerShell = $Type | HasType "PowerShell"
        }

        New-PowerShellPackage @powershellArgs

        Move-Packages "_PowerShell" $root

        # Don't uninstall the repository unless we succeeded, so we can troubleshoot any issues
        # inside the repository incase the pack fails
        $manager.UninstallPowerShellRepository()
    }

    Complete-PrtgProgress
}