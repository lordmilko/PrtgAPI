function Install-CIDependency
{
    [CmdletBinding()]
    param(
        [string[]]$Name,
        [switch]$Log = $true,
        [switch]$SilentSkip
    )

    Get-CallerPreference $PSCmdlet $ExecutionContext.SessionState

    $nameInternal = $Name | foreach {
        if($_ -eq "OpenCover" -or $_ -eq "ReportGenerator")
        {
            return $_ += ".portable"
        }

        if($_ -eq "NuGet")
        {
            return $_ += ".CommandLine"
        }

        return $_
    }

    $dependencies = Get-CIDependency

    if($Name)
    {
        $dependencies = $dependencies | where { $_["Name"] -in $nameInternal }
    }

    if(!$dependencies)
    {
        throw "Could not find dependency '$Name'"
    }

    $silentSkip = $false

    if($Name)
    {
        if($PSBoundParameters.ContainsKey("SilentSkip"))
        {
            $silentSkipInternal = $SilentSkip
        }
        else
        {
            $silentSkipInternal = $true
        }
    }

    foreach($dependency in $dependencies)
    {
        Install-Dependency @dependency -Log:$Log -SilentSkip:$silentSkipInternal
    }
}