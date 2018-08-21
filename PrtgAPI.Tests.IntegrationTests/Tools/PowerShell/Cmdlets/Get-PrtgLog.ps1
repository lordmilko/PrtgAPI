function Get-PrtgLog
{
    [CmdletBinding(
        DefaultParameterSetName="Default"
    )]
    param (
        [Parameter(Mandatory = $false, Position = 0)]
        [string[]]$Pattern,

        [Parameter(Mandatory=$false, ParameterSetName = "Full")]
        [Switch]$Full = $false,

        [Parameter(Mandatory=$false, ParameterSetName = "Default")]
        [int]$Lines = 10,

        [Parameter(Mandatory=$false)]
        [Switch]$Clear = $false
    )
    
    $log = "$env:temp\PrtgAPI.IntegrationTests.log"

    if(Test-Path $log)
    {
        if($Clear)
        {
            Set-Content $log "" -NoNewline
        }

        if($Full)
        {
            notepad $log
        }
        else
        {
            cls
            $Host.UI.RawUI.WindowTitle = $log

            if($Pattern)
            {
                gc $log -Tail $Lines -Wait | sls $Pattern
            }
            else
            {
                gc $log -Tail $Lines -Wait
            }
        }
    }
    else
    {
        Write-Host -ForegroundColor Red "$log does not exist"
    }
}