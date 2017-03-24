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

New-Alias Install-GoPrtg Install-GoPrtgAlias
New-Alias Uninstall-GoPrtg Uninstall-GoPrtgAlias

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


function Install-GoPrtgAlias
{
	[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "", Scope="Function")]
	[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidGlobalAliases", "", Scope="Function")]
	param ()

	if(!(Get-PrtgClient))
	{
		throw "You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer."
	}

	$new = $false

	if(!(Test-Path $Profile))
	{
		New-Item $Profile -Type File -Force
		$new = $true
	}

	if(!$new)
	{
		$contents = Get-Content $Profile -Raw

		$funcExists = $false
		$aliasExists = $false
		$prependNewline = $false

		if($contents -like "function __goPrtgConnectServer*")
		{
			$funcExists = $true
		}

		if($contents -like "New-Alias GoPrtg __goPrtgConnectServer")
		{
			$aliasExists = $true
		}

		if($funcExists -and $aliasExists)
		{
			throw "GoPrtg alias is already installed. To reinstall, uninstall first with Uninstall-GoPrtgAlias"
		}
		else
		{
			if($funcExists -or $aliasExists)
			{
				throw "GoPrtg is partially installed. Please uninstall with Uninstall-GoPrtgAlias."
			}
			else
			{
				if(($contents -ne $null) -and !$contents.EndsWith("`n"))
				{
					$prependNewline = $true
				}
			}
		}
	}

	$client = Get-PrtgClient

	$secureString = ConvertTo-SecureString $client.PassHash -AsPlainText -Force
	$encryptedString = ConvertFrom-SecureString $secureString

	$funcBody = "function __goPrtgConnectServer { Connect-PrtgServer $($client.Server) (New-Object System.Management.Automation.PSCredential -ArgumentList $($client.UserName), (ConvertTo-SecureString $encryptedString)) -PassHash }"

	if($prependNewline)
	{
		$funcBody = "`r`n$funcBody"
	}

	Add-Content $Profile $funcBody
	Add-Content $Profile "New-Alias GoPrtg __goPrtgConnectServer"

	.([ScriptBlock]::Create(($funcBody -replace "function ","function global:")))

	New-Alias GoPrtg __goPrtgConnectServer -Scope Global -Force
}

function Uninstall-GoPrtgAlias
{
	if(!(Test-Path $Profile))
	{
		return
	}

	$contents = Get-Content $Profile

	$funcStr = "function __goPrtgConnectServer*"
	$aliasStr = "New-Alias GoPrtg __goPrtgConnectServer"

	if($contents -like $funcStr)
	{
		$contents = $contents | Where-Object {$_ -notlike $funcStr}
	}

	if($contents -like $aliasStr)
	{
		$contents = $contents | Where-Object {$_ -notlike $aliasStr}
	}

	$str = ""

	if($contents -ne $null)
	{
		if($contents.GetType().BaseType.ToString() -eq "System.Array")
		{
			$str = [string]::Join("`r`n", $contents)
		}
		else
		{
			$str = $contents
		}
	}

	if($str -eq "")
	{
		Set-Content $Profile $str -NoNewline
	}
	else
	{
		Set-Content $Profile $str
	}	
}

Export-ModuleMember -Function * -Alias *