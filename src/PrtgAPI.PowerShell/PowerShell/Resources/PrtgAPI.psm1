New-Alias Add-Trigger Add-NotificationTrigger
New-Alias Get-Trigger Get-NotificationTrigger
New-Alias Get-TriggerTypes Get-NotificationTriggerTypes
New-Alias New-TriggerParameters New-NotificationTriggerParameters
New-Alias New-Trigger Add-NotificationTrigger
New-Alias New-NotificationTrigger Add-NotificationTrigger
New-Alias Remove-Trigger Remove-NotificationTrigger
New-Alias Set-Trigger Set-NotificationTrigger
New-Alias Set-TriggerProperty Set-NotificationTriggerProperty

New-Alias Set-ChannelSetting Set-ChannelProperty
New-Alias Set-ObjectSetting Set-ObjectProperty
New-Alias Get-ObjectSetting Get-ObjectProperty

New-Alias Acknowledge-Sensor Confirm-Sensor
New-Alias Pause-Object Suspend-Object
New-Alias Refresh-Object Update-Object
New-Alias Refresh-SystemInfo Update-SystemInfo
New-Alias Clone-Object Copy-Object
New-Alias Sort-PrtgObject Invoke-SortPrtgObject
New-Alias Simulate-ErrorStatus Test-ErrorStatus
New-Alias Load-PrtgConfigFile Sync-PrtgConfigFile

New-Alias Connect-GoPrtg Connect-GoPrtgServer
New-Alias Install-GoPrtg Install-GoPrtgServer
New-Alias Uninstall-GoPrtg Uninstall-GoPrtgServer

New-Alias GoPrtg Connect-GoPrtgServer

New-Alias flt New-SearchFilter
New-Alias fdef New-SensorFactoryDefinition

New-Alias Restart-PrtgProbe Restart-Probe

New-Alias SensorNode New-SensorNode
New-Alias DeviceNode New-DeviceNode
New-Alias GroupNode New-GroupNode
New-Alias ProbeNode New-ProbeNode
New-Alias TriggerNode New-TriggerNode
New-Alias PropertyNode New-PropertyNode

$ErrorActionPreference = "Stop"

$functions = Get-ChildItem "$PSScriptRoot\Functions"

# Each function also needs to be manually exported in the psd1
foreach($function in $functions)
{
    . $function.FullName
}

# Export-ModuleMember with no arguments should export nothing, but this doesn't work
Export-ModuleMember New-Credential
Export-ModuleMember -Alias *
