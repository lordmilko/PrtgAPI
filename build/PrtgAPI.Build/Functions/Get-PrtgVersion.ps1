<#
.SYNOPSIS
Retrieves version information used by various components of PrtgAPI

.DESCRIPTION
The Get-PrtgVersion cmdlet retrieves version details found in various locations in the PrtgAPI project. Version details can be updated using the Set-PrtgVersion and Update-PrtgVersion cmdlet. The following table details the version details that can be retrieved:

    | Property    | Source                                               | Description                                |
    | ------------| ---------------------------------------------------- | ------------------------------------------ |
    | Package     | build\Version.props                                  | Version used when creating nupkg files     |
    | Assembly    | build\Version.props                                  | Assembly Version used with assemblies      |
    | File        | build\Version.props                                  | Assembly File Version used with assemblies |
    | Module      | PrtgAPI.PowerShell\PowerShell\Resources\PrtgAPI.psd1 | PrtgAPI PowerShell Module version          |
    | ModuleTag   | PrtgAPI.PowerShell\PowerShell\Resources\PrtgAPI.psd1 | PrtgAPI PowerShell Module Release Tag      |
    | PreviousTag | Git                                                  | Version of previous GitHub Release         |

Note that if PrtgAPI detects that the .git folder is missing from the repo or that the "git" command is not installed on your system, the PreviousTag property will be emitted from results.

.PARAMETER Legacy
Specifies whether to retrieve the versions used when compiling PrtgAPI using the .NET Core SDK or the legacy .NET Framework tooling.

.EXAMPLE
C:\> Get-PrtgVersion
Retrieve version information about the PrtgAPI project.

.LINK
Set-PrtgVersion
Update-PrtgVersion
#>
function Get-PrtgVersion
{
    [CmdletBinding()]
    param(
        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy
    )

    $root = Get-SolutionRoot

    Get-CallerPreference $PSCmdlet $ExecutionContext.SessionState -DefaultErrorAction "Continue"

    Get-CIVersion $root -IsCore:(-not $Legacy) -ErrorAction $ErrorActionPreference
}