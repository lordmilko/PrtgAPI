New-Alias Add-Trigger Add-NotificationTrigger
New-Alias Edit-TriggerProperty Edit-NotificationTriggerProperty
New-Alias Get-Trigger Get-NotificationTrigger
New-Alias Get-TriggerTypes Get-NotificationTriggerTypes
New-Alias New-TriggerParameters New-NotificationTriggerParameters
New-Alias Remove-Trigger Remove-NotificationTrigger
New-Alias Set-Trigger Set-NotificationTrigger

New-Alias Set-ChannelSetting Set-ChannelProperty
New-Alias Set-ObjectSetting Set-ObjectProperty
New-Alias Get-ObjectSetting Get-ObjectProperty

New-Alias Acknowledge-Sensor Confirm-Sensor
New-Alias Pause-Object Suspend-Object
New-Alias Refresh-Object Update-Object
New-Alias Clone-Sensor Copy-Sensor
New-Alias Clone-Group Copy-Group
New-Alias Clone-Device Copy-Device
New-Alias Sort-PrtgObject Start-SortPrtgObject
New-Alias Simulate-ErrorStatus Test-ErrorStatus
New-Alias Load-PrtgConfigFile Sync-PrtgConfigFile

New-Alias Connect-GoPrtg Connect-GoPrtgServer
New-Alias Install-GoPrtg Install-GoPrtgServer
New-Alias Uninstall-GoPrtg Uninstall-GoPrtgServer

New-Alias GoPrtg Connect-GoPrtgServer

New-Alias flt New-SearchFilter

New-Alias Restart-PrtgProbe Restart-Probe

$ErrorActionPreference = "Stop"

$functions = Get-ChildItem "$PSScriptRoot\Functions"

$script:prtgAPIModule = $true



# Each function also needs to be manually exported in the psd1
foreach($function in $functions)
{
    . $function.FullName

    #$name = $function.Name -replace ".ps1",""

    #exporting the module member from a variable doesnt work. a string literal works fine
}

# Import PrtgAPI.GoPrtg.ps1 after dot sourcing each function so that the Export-ModuleMember refers to a function that actually exists
. $PSScriptRoot\PrtgAPI.GoPrtg.ps1

# GoPrtg cmdlets are exported in PrtgAPI.GoPrtg.ps1, as not exporting anything in there causes everything to be exported.
# Export-ModuleMember with no arguments should export nothing, but this doesn't work
Export-ModuleMember New-Credential

#bugs: 1. when you remove one prtg server, it removes the whole function from memory
#2. when we remove an entry we need to _update_ the function in memory
#3. when we update an alias we need to update the function in memory
#4. for _every single test_, we need to confirm that when we do something with an entry, the function in memory is updated



#what if we allowed multiple aliases to be installed
#and then, what if we could say ok if you dont specify a server you get the default one, otherwise we switch on
#a pattern of the server you specified
#we could also have a get-goprtgalias cmdlet that lists all the servers we've saved and the username we're using

Export-ModuleMember -Alias *
#Export-ModuleMember -Function * -Alias *
#Export-ModuleMember -Function Install-GoPrtgServer,Uninstall-GoPrtgServer,Get-GoPrtgServer,Connect-GoPrtgServer,Set-GoPrtgAlias

#. $PSScriptRoot\PrtgAPI.GoPrtg.ps1