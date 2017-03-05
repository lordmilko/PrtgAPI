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

function New-Credential
{
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

<#function Install-PrtgAPI
{
	[CmdletBinding()]
	Param()

	$module = Get-Module PrtgAPI | where ModuleType -eq Binary
	$lastSlash = $module.Path.LastIndexOf("\")

	$modulePath = $module.Path.Substring(0, $lastSlash)

	# copy this whole folder to "whatever my program files is"\WindowsPowerShell\Modules\<insert the folder here>
	# maybe copy to both folders
}

function Update-PrtgAPI
{
	[CmdletBinding()]
	Param()

	# requires powershell 5

	#if $psISE, throw an exception
	#otherwise:

	$baseCI = "https://ci.appveyor.com/api/projects/lordmilko/prtgapi"

	$request = Invoke-WebRequest $baseCI
	$json = $request | ConvertFrom-Json

	if($json.build.status -eq "success")
	{
		$oldV = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$PSScriptRoot\PrtgAPI.dll").FileVersion
		$newV = $json.build.version

		Write-Host "Current version: $oldV"
		Write-Host "Latest version: $newV"

		$newVersion = New-Object Version -ArgumentList $newV
		$oldVersion = New-Object Version -ArgumentList $oldV

		if($newVersion -lt $oldVersion)
		{
			Write-Host "Downloading version $newV"

			$tmp = "$env:temp\PrtgAPI.zip"

			#Invoke-WebRequest "$baseCI/artifacts/PrtgAPI/bin/Release/PrtgAPI.zip" -OutFile $tmp

			#if we're installed in program files\windowspowershell\modules, we need to update both the 32-bit and 64-bit versions
			#and we need to move them both to that temp folder and throw an error indicating which one is locked
			#(full path) if we cant move them

			#Expand-Archive c:\a.zip -OutputPath c:\a

			#to update it, launch a new powershell, close this one, download the new version, create a Current folder, move everything to it
			#if we fail, move current back. if we suceed, clear a Previous version folder and move current to Previous

			#define a function, stringify it and pass it to another powershell:

			function UpdateInternal
			{
				param($a)

				#we should have a y/n prompt if we determine the versions differ confirming you want to upgrade

				Write-Host $a

				Write-Host "Checking if $a\PrtgAPI.dll is open"
				if($(try { [IO.File]::OpenWrite("$PSScriptRoot\PrtgAPI.dll").close();$true } catch {$false}))
				{
					Write-Host "not open!"
				}
				else
				{
					Write-Host "open!"
				}
				#Exit
			}

			#$updateFunction = "function update { $((Get-Command UpdateInternal).Definition) }; update"

			$updateFunction = 'function update { ' + (Get-Command UpdateInternal).Definition + ' }; update ' + "`"$PSScriptRoot`""

			$script = ([ScriptBlock]::Create($updateFunction)) 

			powershell.exe -noexit -command $script
		}
		else
		{
			Write-Host "You are already on the latest version"
		}
	}
	else
	{
		Write-Host "Could not update PrtgAPI; last build was unsuccessful. Please see https://github.com/lordmilko/PrtgAPI to update manually."
	}	
}#>

#todo: maybe have a list of exported commands, and have a version on this file in the output of get-module