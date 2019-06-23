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

    if($color)
    {
        Write-Host -ForegroundColor $color $msg
    }
    else
    {
        if($psISE)
        {
            Write-Verbose $msg
        }
        else
        {
            Write-Host $msg
        }
    }

    [IO.File]::AppendAllText("$env:TEMP\PrtgAPI.Build.log", "$(Get-Date) $msg`r`n")
}

Export-ModuleMember Write-LogHeader,Write-LogSubHeader,Write-LogInfo,Write-LogError