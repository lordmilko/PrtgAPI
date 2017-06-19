if(!$prtgAPIModule)
{
	. "$PSScriptRoot\..\Resources\PrtgAPI.GoPrtg.ps1"
}

function Connect-GoPrtgServer
{
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $false, Position = 0)]
		[string]
		$Server
	)

	if(Get-Command __goPrtgGetServers -ErrorAction SilentlyContinue)
	{
		$servers = @(GetServers)

		if(!$Server)
		{
			$connectTo = $servers | Select-Object -First 1

			ConnectToGoPrtgServer $connectTo
		}
		else
		{
			$matches = @($servers | Where-Object {$_.Server -like $Server -or $_.Alias -eq $Server})

			if($matches.Count -eq 1)
			{
				ConnectToGoPrtgServer $matches
			}
			elseif($matches.Count -gt 1)
			{
				Write-ColorOutput "`nAmbiguous server specified. The following servers matched the specified server name or alias" -ForegroundColor Red

				Get-GoPrtgServer | Where-Object {$_.Server -like $Server -or $_.Alias -eq $Server}
			}
			else
			{
				Write-ColorOutput "`nCould not find server that matches name or alias '$Server'`n" -ForegroundColor Red
			}
		}	
	}
	else
	{
		Write-ColorOutput "`nNo GoPrtg servers are installed. Please install a server first using Install-GoPrtgServer`n" -ForegroundColor Red
	}
}

function ConnectToGoPrtgServer($server)
{
	$client = Get-PrtgClient

	if($server.Server -eq $client.Server -and $server.UserName -eq $client.UserName)
	{
		Write-ColorOutput "`nAlready connected to $($server.Server) as $($server.UserName)`n" -ForegroundColor Yellow
	}
	else
	{
		$credential = New-Object System.Management.Automation.PSCredential -ArgumentList $server.UserName, (ConvertTo-SecureString $server.PassHash)
		Connect-PrtgServer $server.Server $credential -PassHash -Force

		Write-ColorOutput "`nConnected to $($server.Server) as $($server.UserName)`n" -ForegroundColor Green
	}
}