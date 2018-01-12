function Get-PrtgLog
{
    [CmdletBinding(
        DefaultParameterSetName="Default"
    )]
    param (
        [Parameter(Mandatory=$false, ParameterSetName = "Full")]
        [Switch]$Full = $false,

        [Parameter(Mandatory=$false, ParameterSetName = "Default")]
        [int]$Lines = 10
    )
    
    $log = "$env:temp\PrtgAPI.IntegrationTests.log"

    if(Test-Path $log)
    {
        if($Full)
        {
            notepad $log
        }
        else
        {
            cls
            $Host.UI.RawUI.WindowTitle = $log

            gc $log -Tail $Lines -Wait
        }
    }
    else
    {
        Write-Host -ForegroundColor Red "$log does not exist"
    }
}