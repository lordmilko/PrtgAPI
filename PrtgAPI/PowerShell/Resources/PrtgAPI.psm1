New-Alias Add-Trigger Add-NotificationTrigger
New-Alias Edit-TriggerProperty Edit-NotificationTriggerProperty
New-Alias Get-Trigger Get-NotificationTrigger
New-Alias Get-TriggerTypes Get-NotificationTriggerTypes
New-Alias New-TriggerParameter New-NotificationTriggerParameter
New-Alias Remove-Trigger Remove-NotificationTrigger
New-Alias Set-Trigger Set-NotificationTrigger

New-Alias Set-ChannelSetting Set-ChannelProperty
New-Alias Set-ObjectSetting Set-ObjectProperty

New-Alias Acknowledge-Sensor Confirm-Sensor
New-Alias Pause-Object Suspend-Object
New-Alias Refresh-Object Update-Object
New-Alias Clone-Sensor Copy-Sensor
New-Alias Clone-Group Copy-Group
New-Alias Clone-Device Copy-Device
New-Alias Sort-PrtgObject Start-SortPrtgObject

New-Alias Connect-GoPrtg Connect-GoPrtgServer
New-Alias Install-GoPrtg Install-GoPrtgServer
New-Alias Uninstall-GoPrtg Uninstall-GoPrtgServer

New-Alias GoPrtg Connect-GoPrtgServer

$ErrorActionPreference = "Stop"


#bugs: 1. when you remove one prtg server, it removes the whole function from memory
#2. when we remove an entry we need to _update_ the function in memory
#3. when we update an alias we need to update the function in memory
#4. for _every single test_, we need to confirm that when we do something with an entry, the function in memory is updated

function New-Credential
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "", Scope="Function")]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingUserNameAndPassWordParams", "", Scope="Function")]
	[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "", Scope="Function")]
	[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseShouldProcessForStateChangingFunctions", "", Scope="Function")]
	[CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $UserName,

        [string]
        $Password
    )
    
    $secureString = ConvertTo-SecureString $Password -AsPlainText -Force
    New-Object System.Management.Automation.PSCredential -ArgumentList $UserName, $secureString
}

#what if we allowed multiple aliases to be installed
#and then, what if we could say ok if you dont specify a server you get the default one, otherwise we switch on
#a pattern of the server you specified
#we could also have a get-goprtgalias cmdlet that lists all the servers we've saved and the username we're using

Export-ModuleMember -Function * -Alias *

. $PSScriptRoot\PrtgAPI.GoPrtg.ps1