$Host.UI.RawUI.WindowTitle = "PrtgAPI Build Environment"

function ShowBanner
{
    Write-Host "          Welcome to PrtgAPI Build Environment!"
    Write-Host ""
    Write-Host "  Build the latest version of PrtgAPI:                 " -NoNewLine
    Write-Host "Invoke-PrtgBuild" -ForegroundColor Yellow

    Write-Host "  To find out what commands are available, type:       " -NoNewLine
    Write-Host "Get-PrtgCommand" -ForegroundColor Yellow

    Write-Host "  Open a PrtgAPI prompt with:                          " -NoNewLine
    Write-Host "Start-PrtgAPI" -ForegroundColor Yellow

    Write-Host "  If you need more help, visit the PrtgAPI Wiki:       " -NoNewLine
    Write-Host "Get-PrtgHelp" -ForegroundColor Yellow

    Write-Host ""
    Write-Host "          Copyright (C) lordmilko, 2015"
    Write-Host ""
    Write-Host ""
}

if(!$psISE)
{
    # Modify the prompt function to change the console prompt.
    function global:prompt{

        # change prompt text
        Write-Host "PrtgAPI " -NoNewLine -ForegroundColor Green
        Write-Host ((Get-location).Path + ">") -NoNewLine
        return " "
    }
}

[Environment]::SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", 1)

ShowBanner