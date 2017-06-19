if(!$prtgAPIModule)
{
	. "$PSScriptRoot\..\Resources\PrtgAPI.GoPrtg.ps1"
}

function Install-GoPrtgServer
{
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $false, Position = 0)]
		[string]
		$Alias
	)

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

	#todo: what is the force parameter meant to do? overwrite? if so we need to update some of the existing exception messages

    $newContents = GetNewContents $Alias
	$profileContent = GetProfileContents $newContents

    if($new)
    {
        Add-Content $Profile $profileContent
    }
    else
    {
        Set-Content $Profile $profileContent
    }

    .([ScriptBlock]::Create(($newContents.Func -replace "function ","function global:")))
}

function GetNewContents($alias)
{
    $contents = Get-Content $Profile

	$functionStart = GetGoPrtgStart $contents
	$functionEnd = GetGoPrtgEnd $contents $functionStart

	$functionExists = Get-Command __goPrtgGetServers -ErrorAction SilentlyContinue

	ValidateGoPrtgBlock $functionStart $functionEnd $functionExists

	# If the function didn't exist but the header and footer did, an exception would have been thrown above.
	# We can infer then that if we've reached this stage and the function doesn't exist, the header and footer don't exist either

	if($functionExists)
	{
		$servers = @(GetServers)

		CheckServerAgainstExistingRecords $servers $alias

        $pre = GetPre $contents $functionStart

        $post = GetPost $contents $functionEnd

        $new = GetNew $servers $alias
		
        $obj = ContentsObj
        $obj.Pre = $pre
        $obj.Func = $new
        $obj.Post = $post

        return $obj
	}
	else
	{
		$new = GetNew $null $alias

        $obj = ContentsObj
        $obj.Pre = $contents
        $obj.Func = $new
        $obj.Post = $null

        return $obj
	}
}

function GetProfileContents($newContents)
{
	$funcBody = ""

    if($null -ne $newContents.Pre)
    {
        $funcBody += [string]::Join("`r`n", $newContents.Pre) + "`r`n"
    }

	# Due to the way Powershell's Command Discovery works, PowerShell will detect match the Get-GoPrtg alias to the command GoPrtg
	# before detecting GoPrtg is in fact a valid alias all by itself. By forcing the module to import, PowerShell will correctly
	# identify that GoPrtg is the intended command.

    $funcBody += "########################### Start GoPrtg Servers ###########################`r`n`r`n" + 
				 [string]::Join("`r`n", $newContents.Func) +
				 "`r`n`r`n############################ End GoPrtg Servers ############################"				

    if($null -ne $newContents.Post)
    {
        $funcBody += "`r`n" + [string]::Join("`r`n", $newContents.Post)
    }

	return $funcBody
}

function CheckServerAgainstExistingRecords($servers, $alias)
{
	$client = Get-PrtgClient

	if($servers | Where-Object {$_.Server -eq $client.Server })
	{
		$record = $servers | Where-Object {$_.Server -eq $client.Server } | Select-Object -first 1

		if($null -ne $record.Alias)
		{
			if($record.UserName -eq $client.UserName)
			{
				throw "Cannot add server '$($client.Server)': a record for user '$($client.UserName)' already exists. To update the alias of this record use Set-GoPrtgAlias. To reinstall this record, first uninstall with Uninstall-GoPrtgServer and then re-run Install-GoPrtgServer."
			}

			if(!$alias)
			{
				throw "Cannot add server '$($client.Server)': an alias must be specified to differentiate this connection from an existing connection with the same server address."
			}
		}
		else
		{
			if($record.UserName -eq $client.UserName)
			{
				throw "Cannot add server '$($client.Server)': a record for user '$($client.UserName)' already exists."
			}

			throw "Cannot add server '$($client.Server)': a record for server already exists without an alias. Please update the alias of this record with Set-GoPrtgAlias and try again."
		}
	}
	elseif($servers | Where-Object {$_.Alias -eq $alias})
	{
		throw "Cannot add server '$($client.Server)' with alias '$alias': a record for this alias already exists. For more information see 'Get-GoPrtgServer $alias'"
	}
}

function GetNew
{
	[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "", Scope="Function")]
	param($servers, $alias)

    $client = Get-PrtgClient

    $secureString = ConvertTo-SecureString $client.PassHash -AsPlainText -Force
	$encryptedString = ConvertFrom-SecureString $secureString
    #$encryptedString = $client.PassHash

    if(!$servers)
    {
        $servers = @()
    }

	$newRecord = SerializeRecord $client.Server $alias $client.UserName $encryptedString

	$newRecord = $newRecord -replace "`"```"","`"" -replace "```"`"","`"" -replace "```"","`""

	$r = $newRecord | ConvertFrom-Csv -Header "Server","Alias","UserName","PassHash"
	
	if($r.Alias -eq "")
	{
		$r.Alias = $null
	}

	<#$obj = New-Object PSObject
	$obj | Add-Member NoteProperty Server $client.Server
	$obj | Add-Member NoteProperty Alias $alias
	$obj | Add-Member NoteProperty UserName $client.UserName
	$obj | Add-Member NoteProperty PassHash $encryptedString#>

	$servers += $r

    $newContents = @()

    $newContents += "function __goPrtgGetServers {@("

    $index = 0
    $count = $servers.Count

    foreach($s in $servers)
    {
        $toAdd = SerializeRecord $s.Server $s.Alias $s.UserName $s.PassHash

        if($count -gt 1 -and ($index -lt $count - 1))
        {
            $toAdd += ","
        }

        $newContents += $toAdd

        $index++
    }

	$newContents += ")}"

    return $newContents
}

function SerializeRecord($server, $alias, $username, $passhash)
{
	$record = $null

	if($alias)
	{
		$record = "    `"```"$server```",```"$alias```",```"$username```",```"$passhash```"`""
	}
	else
	{
		$record = "    `"```"$server```",,```"$username```",```"$passhash```"`""
	}

	return $record
}

function ContentsObj
{
    $obj = @{
        "Pre" = @();
        "Func" = @();
        "Post" = @()
    }

    return $obj
}