<#
.SYNOPSIS
Opens the PrtgAPI Wiki for getting help with the PrtgAPI Build Environment.

.DESCRIPTION
The Get-PrtgHelp cmdlet opens the PrtgAPI Wiki page containing detailed instructions on compiling PrtgAPI and using the PrtgAPI Build Environment.

Note: due to limitations of the Unix platform, when running on Linux or macOS the Get-PrtgHelp cmdlet will instead display the URL that you should navigate to instead of automatically opening the URL in your default web browser.

.EXAMPLE
C:\> Get-PrtgHelp
Open the PrtgAPI Wiki article detailing how to compile PrtgAPI.
#>
function Get-PrtgHelp
{
    $url = "https://github.com/lordmilko/PrtgAPI/wiki/Build-Environment"

    if(Test-IsWindows)
    {
        Start-Process $url
    }
    else
    {
        Write-Host "PrtgAPI Wiki: $url"
    }
}