if(!$script:prtgAPIModule)
{
	. "$PSScriptRoot\..\Resources\PrtgAPI.GoPrtg.ps1"
}

function Get-GoPrtgServer
{
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $false, Position = 0)]
		[string]
		$Server
	)

    if(Get-Command -Name __goPrtgGetServers -ErrorAction SilentlyContinue)
    {
		$serversText = @(__goPrtgGetServers)|foreach {
			"`"[ ]`",$_"
		}
		
		$servers = $serversText | ConvertFrom-Csv -Header "[!]","Server","Alias","UserName","PassHash"

		$client = Get-PrtgClient

		foreach($s in $servers)
		{
			if($s.Alias -eq "")
			{
				$s.Alias = $null
			}

			if($s.Server -eq $client.Server -and $s.UserName -eq $client.UserName)
			{
				$s."[!]" = "[*]"
			}
		}

		if($Server)
		{
			$servers = @($servers | ForEach-Object {
				if($_.Server -like $Server)
				{
					$_
				}
				else
				{
					if($_.Alias -eq $Server)
					{
						$_
					}
				}
			})
		}

		$resp = $servers | Select-Object @{name="[!]";expression={$_."[!]"}},Server,Alias,UserName

		return $resp

        $response = @()

        foreach($server in $servers)
        {
            $username = $server.UserName
            $alias = $servers.Alias

            $obj = New-Object PSObject
            $obj | Add-Member NoteProperty Server $server.Server
            $obj | Add-Member NoteProperty Alias $alias
            $obj | Add-Member NoteProperty UserName $username

            $response += $obj
        }

        return $response
    }
    else
    {
        Write-ColorOutput "`nGoPrtg is not installed. Run Install-GoPrtgServer first to install a GoPrtg server.`n" -ForegroundColor Red
    }
}