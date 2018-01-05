function Get-PrtgLog
{
    [CmdletBinding()]
    param ()
    
    $log = "$env:temp\PrtgAPI.IntegrationTests.log"

    if(Test-Path $log)
    {
        cls
        $Host.UI.RawUI.WindowTitle = $log

        gc $log -Tail 10 -Wait
    }
    else
    {
        Write-Host -ForegroundColor Red "$log does not exist"
    }
}