<#
.SYNOPSIS
Retrieves commands that are available in the PrtgAPI Build Environment.

.DESCRIPTION
The Get-PrtgCommand retrieves all commands that are available for use within the PrtgAPI Build Environment. Each command contains a description outlining what exactly that command does. The results from Get-PrtgCommand can be filtered by specifying a wildcard expression matching part of the command's name you wish to find.

.PARAMETER Name
Wildcard used to filter results to a specific command.

.EXAMPLE
C:\> Get-PrtgCommand
List all commands supported by the PrtgAPI Build Module

.EXAMPLE
C:\> Get-PrtgCommand *build*
List all commands whose name contains "build"
#>
function Get-PrtgCommand
{
    [CmdletBinding()]
    param(
        [ValidateScript( { 
            if([String]::IsNullOrWhiteSpace($_))
            {
                throw "The argument is null, empty or whitespace. Provide an argument that is not null, empty or whitespace and then try the command again."
            }

            return $true
        } )]
        [Parameter(Mandatory = $false, Position = 0)]
        [string]$Name = "*"
    )

    $excluded = @(
        "Test-PrtgCI"
        "Write-PrtgProgress"
        "Complete-PrtgProgress"
    )

    if($script:getPrtgCommandCache)
    {
        $script:getPrtgCommandCache | where Name -Like $Name
    }
    else
    {
        $commands = gcm -Module PrtgAPI.Build -Name $Name | where { $_.Name -Like "*-Prtg*" -and $_.Name -notin $excluded }

        $sorted = $commands | foreach {
            [PSCustomObject]@{
                Name = $_.Name
                Category = GetCategory $_.Name
                Description = (Get-Help $_.Name).Synopsis.Trim()
            }
        } | Sort-Object Category,Name

        if($Name -eq "*")
        {
            $script:getPrtgCommandCache = $sorted
        }

        return $sorted
    }
}

function GetCategory($name)
{
    if($name -like "*-PrtgVersion")
    {
        return "Version"
    }
    elseif($name -like "*-PrtgBuild")
    {
        return "Build"
    }

    if($name -like "*-PrtgTest*")
    {
        return "Test"
    }

    $ci = @(
        "Get-PrtgCoverage"
        "New-PrtgPackage"
        "Simulate-PrtgCI"
    )

    if($name -in $ci)
    {
        return "CI"
    }

    $help = @(
        "Get-PrtgCommand"
        "Get-PrtgHelp"
    )

    if($name -in $help)
    {
        return "Help"
    }

    $utilities = @(
        "Get-PrtgLog"
        "Install-PrtgDependency"
        "Invoke-PrtgAnalyzer"
        "Start-PrtgAPI"
    )

    if($name -in $utilities)
    {
        return "Utility"
    }

    throw "Don't know how to categorize cmdlet '$name'"
}