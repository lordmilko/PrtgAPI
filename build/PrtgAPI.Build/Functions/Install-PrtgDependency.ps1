<#
.SYNOPSIS
Installs dependencies required to use the PrtgAPI Build Environment

.DESCRIPTION
The Install-PrtgDependency installs dependencies required to utilize the PrtgAPI Build Environment. By default, Install-PrtgDependency will install all dependencies that are required. A specific dependency can be installed by specifying a value to the -Name parameter. If dependencies are not installed, the PrtgAPI Build Environment will automatically install a given dependency for you when attempting to execute a command that requires it.

.PARAMETER Name
The dependencies to install. If no value is specified, all dependencies will be installed.

.EXAMPLE
C:\> Install-PrtgDependency
Install all dependencies required to use the PrtgAPI Build Environment

.EXAMPLE
C:\> Install-PrtgDependency Pester
Install the version of Pester required by the PrtgAPI Build Environment
#>
function Install-PrtgDependency
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        [ValidateSet(
            "chocolatey", "dotnet", "Pester", "Codecov", "OpenCover", "ReportGenerator",
            "VSWhere", "NuGet", "NuGetProvider", "PowerShellGet", "PSScriptAnalyzer",
            "net452", "net461"
        )]
        [string[]]$Name
    )

    Install-CIDependency $Name -Log:$false
}