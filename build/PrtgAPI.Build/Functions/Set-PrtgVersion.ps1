<#
.SYNOPSIS
Sets the version of all components used when building PrtgAPI

.DESCRIPTION
The Set-PrtgVersion cmdlet updates the version of PrtgAPI. The Set-PrtgVersion cmdlet allows the major, minor, build and revision components to be replaced with any arbitrary version. Typically the Set-PrtgVersion cmdlet is used to revert mistakes made when utilizing the Update-PrtgVersion cmdlet as part of a normal release, or to reset the version when updating the major or minor version components.

.PARAMETER Version
The version to set PrtgAPI to. Must at least include a major and minor version number.

.PARAMETER Legacy
Specifies whether to increase the version used when compiling PrtgAPI using the .NET Core SDK or the legacy .NET Framework tooling.

.EXAMPLE
C:\> Set-PrtgVersion 1.2.3
Set the version to version 1.2.3.0

.EXAMPLE
C:\> Set-PrtgVersion 1.2.3.4
Set the version to version 1.2.3.4. Systems that only utilize the first three version components will be versioned as 1.2.3

.LINK
Get-PrtgVersion
Update-PrtgVersion
#>
function Set-PrtgVersion
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [Version]$Version,

        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy
    )

    $old = Get-PrtgVersion -Legacy:$Legacy -ErrorAction SilentlyContinue
    Set-CIVersion $Version -IsCore:(-not $Legacy) -ErrorAction SilentlyContinue
    $new = Get-PrtgVersion -Legacy:$Legacy -ErrorAction SilentlyContinue

    $result = [PSCustomObject]@{
        Package = $null
        Assembly = $null
        File = $null
        Info = $null
        Module = $null
        ModuleTag = $null
    }

    if($old.PreviousTag)
    {
        $result | Add-Member PreviousTag $null
    }

    foreach($property in $old.PSObject.Properties)
    {
        $result.($property.Name) = ([PrtgVersionChange]::new($old.($property.Name), $new.($property.Name)))
    }

    return $result
}

class PrtgVersionChange
{
    [string]$Old
    [string]$New

    PrtgVersionChange($old, $new)
    {
        $this.Old = $old
        $this.New = $new
    }

    [string]ToString()
    {
        if($this.Old -eq $this.New)
        {
            return $this.Old
        }

        return "$($this.Old) -> $($this.New)"
    }
}