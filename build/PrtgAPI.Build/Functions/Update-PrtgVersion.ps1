<#
.SYNOPSIS
Increments the version of all components used when building PrtgAPI

.DESCRIPTION
The Update-PrtgVersion cmdlet increments the version of PrtgAPI by a single build version. The Update-PrtgVersion should typically be run when preparing to release a new version. The changes to the PrtgAPI repo caused by running the Update-PrtgVersion cmdlet are typically commited as the "release" of the next PrtgAPI version. Once pushed to GitHub, the CI system will mark the build and all future builds as "release candidates" until the version is actually released.

If you wish to decrement the build version or change the major, minor or revision version components, you can do so by overwriting the entire version using the Set-PrtgVersion cmdlet.

For more information on the version components that may be processed, please see Get-Help Get-PrtgVersion

.PARAMETER Legacy
Specifies whether to increase the version used when compiling PrtgAPI using the .NET Core SDK or the legacy .NET Framework tooling.

.EXAMPLE
C:\> Update-PrtgVersion
Increment the PrtgAPI build version by 1.

.LINK
Get-PrtgVersion
Set-PrtgVersion
#>
function Update-PrtgVersion
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

    $version = Get-PrtgVersion -Legacy:$Legacy -ErrorAction SilentlyContinue

    $major = $version.File.Major
    $minor = $version.File.Minor
    $build = $version.File.Build + 1
    $revision = $version.File.Revision

    $newVersion = [Version]"$major.$minor.$build.$revision"

    Set-PrtgVersion $newVersion -Legacy:$Legacy
}