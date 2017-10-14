# GoPrtg Shared Functions

function GetServers
{
	$servers = __goPrtgGetServers | ConvertFrom-Csv -Header "Server","Alias","UserName","PassHash"

	foreach($server in $servers)
	{
		if($server.Alias -eq "")
		{
			$server.Alias = $null
		}
	}

	return $servers
}

function GetGoPrtgStart($contents)
{
    for($i = 0; $i -lt $contents.Length; $i++)
	{
		if($contents[$i] -eq "########################### Start GoPrtg Servers ###########################")
		{
			return $i
		}
	}
    
    return $null
}

function GetGoPrtgEnd($contents)
{
    if($functionStart -ne -1)
	{
		for($i = $functionStart + 1; $i -lt $contents.Length; $i++)
		{
			if($contents[$i] -eq "############################ End GoPrtg Servers ############################")
			{
				return $i
			}
		}
	}

    return $null
}

function GetPre($contents, $functionStart)
{
    $index = 0

	$pre = $contents | ForEach-Object {
		if($index -lt $functionStart)
		{
			$_
		}

		$index++
    }

    return $pre
}

function GetPost($contents, $functionEnd)
{
    $index = 0

	$post = $contents | ForEach-Object {
		if($index -gt $functionEnd)
		{
			return $_
		}

		$index++
    }

    return $post
}

function ValidateGoPrtgBlock
{
	[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSPossibleIncorrectComparisonWithNull", "", Scope="Function")]
	param($functionStart, $functionEnd, $functionExists)

	if(!$functionExists -and $null -eq $functionStart -and $null -eq $functionEnd)
    {
        return
    }

	if($null -eq $functionStart -and $null -ne $functionEnd)
	{
		throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
	}

	if($null -ne $functionStart -and $null -eq $functionEnd)
	{
		throw "GoPrtg Servers end line '############################ End GoPrtg Servers ############################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
	}

	if($null -ne $functionStart -and $null -ne $functionEnd)
	{
		if(!$functionExists)
		{
			throw "GoPrtg header and footer are present in PowerShell profile, however __goPrtgGetServers function was not loaded into the current session. Please verify the function has not been corrupted or remove the GoPrtg header and footer and re-run Install-GoPrtgServer."
		}
	}
}

function AddGoPrtgFunctionHeaderAndFooter($contents, $funcBody, $functionStart, $functionEnd)
{
	$formatted = @()
	$formatted += $contents[$functionStart + 2]
    $formatted += $funcBody
	$formatted += $contents[$functionEnd - 2]

	return $formatted
}

function UpdateGoPrtgFunctionBody($funcBody, $contents, $functionStart, $functionEnd)
{
	$pre = GetPre $contents $functionStart
    $post = GetPost $contents $functionEnd

    $newContent = @()
    $newContent += $pre

    if($null -ne $funcBody)
    {
		$funcBody = AddGoPrtgHeaderAndFooter $contents $funcBody $functionStart $functionEnd
        $newContent += $funcBody
    }

    $newContent += $post

    $str = [string]::Join("`r`n", $newContent)

    if($str -eq "")
	{
		Set-Content $Profile $str -NoNewline
	}
	else
	{
		Set-Content $Profile $str
	}
}

function AddGoPrtgHeaderAndFooter($contents, $funcBody, $functionStart, $functionEnd)
{
	$formatted = @()
    $formatted += $contents[$functionStart]
	$formatted += $contents[$functionStart + 1]
    $formatted += $funcBody
	$formatted += $contents[$functionEnd - 1]
    $formatted += $contents[$functionEnd]

	return $formatted
}

function UpdateServerRunner([ScriptBlock]$script)
{
    if(!(Test-Path $Profile))
	{
		throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer."
	}

	$contents = Get-Content $Profile

	$functionStart = GetGoPrtgStart $contents
	$functionEnd = GetGoPrtgEnd $contents $functionStart

	$functionExists = Get-Command __goPrtgGetServers -ErrorAction SilentlyContinue

	ValidateGoPrtgBlock $functionStart $functionEnd $functionExists

	if($functionExists)
	{
		if(!(Get-PrtgClient))
		{
			throw "You are not connected to a PRTG Server. Please connect first using GoPrtg [<server>]."
		}

		$client = Get-PrtgClient

		$servers = @(GetServers)

		#todo: when we update the aliases in the file, we need to also update them in memory!! is that even happening?

		$targetServer = @($servers | Where-Object {$_.Server -eq $client.Server -and $_.UserName -eq $client.UserName })

		if($targetServer.Count -eq 0)
		{
            $serverIgnoringUserName = @($servers | Where-Object {$_.Server -eq $client.Server})

            if($serverIgnoringUserName.Count -eq 0)
            {
                throw "Server '$($client.Server)' is not a valid GoPrtg server. To install this server, run Install-GoPrtgServer [<alias>]"
            }
            else
            {
                throw "Server '$($client.Server)' is a valid GoPrtg server, however you are not authenticated as a valid user for this server. To modify this server, first run GoPrtg [<alias>], then re-run the original command"
            }
		}
		elseif($targetServer.Count -eq 1)
		{
            $rowUpdater = & $script $servers $targetServer

            UpdateServerRow $contents $targetServer $functionStart $functionEnd $rowUpdater
		}
		elseif($targetServer.Count -gt 1)
		{
			throw "A critical error has occurred: more than one server exists with the same server and username. Please remove the duplicate from your $Profile manually."
		}
	}
	else
	{
		throw "GoPrtg is not installed. Run Install-GoPrtgServer <alias> to install a server with the specified alias."
	}
}

function UpdateServerRow($contents, $server, $functionStart, $functionEnd, [ScriptBlock]$updater)
{
	$new = @()

	for($i = $functionStart + 3; $i -le $functionEnd -3; $i++)
	{
		$line = $contents[$i]

		$compare = $null

		# Get the text of the server based on whether or not it has an alias

		if($null -ne $server.Alias -and $server.Alias -ne "")
		{
			$compare = "    `"```````"$($server.Server)```````",```````"$($server.Alias)```````",```````"$($server.UserName)```````",```````"$($server.PassHash)```````"`"*"
		}
		else
		{
			$compare = "    `"```````"$($server.Server)```````",,```````"$($server.UserName)```````",```````"$($server.PassHash)```````"`"*"
		}

		if($line -like $compare)
		{
			$newLine = $null

			# Update the value of this line based on whether or not our alias is empty

			$newLine = & $updater $server {
				param($server, $alias, $username, $passhash)

				CreateNewLine $server $alias $username $passhash
			}

			if($line.EndsWith(","))
			{
				$newLine += ","
			}

			$new += $newLine
		}
		else
		{
			$new += $line
		}		
	}

	$final = AddGoPrtgFunctionHeaderAndFooter $contents $new $functionStart $functionEnd

	UpdateGoPrtgFunctionBody $final $contents $functionStart $functionEnd

	.([ScriptBlock]::Create(($final -replace "function ","function global:")))
}

function CreateNewLine($server, $alias, $username, $passhash)
{
    $newLine = ""

    if($null -ne $alias -and $alias -ne "")
	{
		$newLine = "    `"```"$($server)```",```"$alias```",```"$($userName)```",```"$($passHash)```"`""
	}
	else
	{
		$newLine = "    `"```"$($server)```",,```"$($userName)```",```"$($passHash)```"`""
	}

    return $newLine
}

function Write-ColorOutput($Object, $ForegroundColor)
{
	$initialColor = $Host.UI.RawUI.ForegroundColor

	if($initialColor -ne -1)
	{
		$Host.UI.RawUI.ForegroundColor = $ForegroundColor
	}

	$Object | Write-Output

	if($initialColor -ne -1)
	{
		$Host.UI.RawUI.ForegroundColor = $initialColor
	}

	#need to test piping something with a header
}


#todo: all of these tests should test extracting the passhash and confirming it decrypts to the original value


#todo: run invoke-scriptanalyzer when finished

#todo: make the passhash encrypted again, change the should be's to should belike's

#todo: if you remove the last server, the second last server should remove its semicolon

#push notes

#maybe we should consider using the password vault?
#https://www.powershellgallery.com/packages/BetterCredentials/4.3/Content/BetterCredentials.psm1

#note: if we use the windows credential store, our module wont be cross platform compatible

# Export nothing
try
{
	Export-ModuleMember -Function Connect-GoPrtgServer, Get-GoPrtgServer, Install-GoPrtgServer, Set-GoPrtgAlias, Uninstall-GoPrtgServer, Update-GoPrtgCredential
}
catch
{
}