function Write-LogHeader($msg)
{
    if($env:APPVEYOR)
    {
        Write-LogInfo $msg
    }
    else
    {
        Write-Log $msg Cyan
    }
}

function Write-LogSubHeader($msg)
{
    Write-Log $msg Magenta
}

function Write-LogInfo($msg)
{
    Write-Log $msg
}

function Write-LogError($msg)
{
    Write-Log $msg Yellow
}

function Write-Log($msg, $color)
{
    $msg = "`t$msg"

    $msg = $msg -replace "`t","    "

    if(!$global:prtgProgressArgs)
    {
        if($color)
        {
            Write-Host -ForegroundColor $color $msg
        }
        else
        {
            Write-Host $msg
        }
    }
    else
    {
        $global:prtgProgressArgs.CurrentOperation = $msg.Trim()
        Write-Progress @global:prtgProgressArgs
    }

    $nl = [Environment]::NewLine

    [IO.File]::AppendAllText("$env:TEMP\PrtgAPI.Build.log", "$(Get-Date) $msg$nl")
}

function Write-LogVerbose($msg, $color)
{
    if($psISE)
    {
        Write-Verbose $msg

        $msg = "`t$msg"

        $msg = $msg -replace "`t","    "

        $nl = [Environment]::NewLine

        [IO.File]::AppendAllText("$env:TEMP\PrtgAPI.Build.log", "$(Get-Date) $msg$nl")
    }
    else
    {
        Write-Log $msg $color
    }
}

if(!$skipExport)
{
    Export-ModuleMember Write-LogHeader,Write-LogSubHeader,Write-LogInfo,Write-LogError,Write-LogVerbose
}