$skipExport = $true
. $PSScriptRoot\..\Helpers\Write-Log.ps1

function It
{
    [CmdletBinding(DefaultParameterSetName = 'Normal')]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$name,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock] $script,

        [Parameter(Mandatory = $false)]
        [System.Collections.IDictionary[]] $TestCases
    )

    Write-LogInfo "Processing test '$name'"

    Pester\It $name {
        try
        {
            & $script
        }
        catch
        {
            Write-LogError "Error: $($_.Exception.Message)"

            throw
        }
    }
}

function WithoutTestDrive($script)
{
    $drive = Get-PSDrive TestDrive -Scope Global

    $drive | Remove-PSDrive -Force
    Remove-Variable $drive.Name -Scope Global -Force

    try
    {
        & $script
    }
    finally
    {
        New-PSDrive $drive.Name -PSProvider $drive.Provider -Root $drive.Root -Scope Global
        New-Variable $drive.Name -Scope Global -Value $drive.Root
    }
}