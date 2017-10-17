if(!$script:prtgAPIModule)
{
    . "$PSScriptRoot\..\Resources\PrtgAPI.GoPrtg.ps1"
}

function Uninstall-GoPrtgServer([string]$Server, [switch]$Force)
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
        $servers = @(GetServers)

        $Server = SetServerWildcardIfMissing $Server $servers $Force
        $matches = @(GetMatches $Server $servers)

        $filtered = GetFiltered $contents $matches $functionStart $functionEnd

        if($filtered)
        {
            $filtered = AddGoPrtgFunctionHeaderAndFooter $contents $filtered $functionStart $functionEnd
        }

        UpdateGoPrtgFunctionBody $filtered $contents $functionStart $functionEnd

        if($null -ne $filtered)
        {
            .([ScriptBlock]::Create(($filtered -replace "function ","function global:")))
        }
        else
        {
            Remove-Item Function:\__goPrtgGetServers
        }
    }
    else
    {
        throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer."
    }
}

function SetServerWildcardIfMissing($server, $servers, $force)
{
    if($force)
    {
        $server = "*"
    }
    else
    {
        if(!$server)
        {
            if($servers.Count -gt 1)
            {
                throw "Cannot remove servers; server name or alias must be specified when multiple entries exist. To remove all servers, specify -Force"
            }
            else
            {
                $server = "*"
            }
        }
    }

    return $server
}

function GetMatches($server, $servers)
{
    $matches = @()

    foreach($s in $servers)
    {
        if($s.Server -like $server)
        {
            $matches += $s
        }
        else
        {
            if($null -ne $s.Alias)
            {
                if($s.Alias -like $server) # the alias
                {
                    $matches += $s
                }
            }
        }
    }

    if(!$matches)
    {
        throw "'$server' is not a valid server name or alias. To view all saved servers, run Get-GoPrtgServer"
    }

    return $matches
}

function GetFiltered($contents, $matches, $functionStart, $functionEnd)
{
    $filtered = @()

    for($i = $functionStart + 3; $i -le $functionEnd - 3; $i++)
    {
        $line = $contents[$i]

        $include = $true

        foreach($s in $matches)
        {
            $toRemove = $null

            if($null -ne $s.Alias)
            {
                $toRemove = "    `"```````"$($s.Server)```````",```````"$($s.Alias)```````",```````"$($s.UserName)```````",```````"$($s.PassHash)```````"`"*" #do we need twice as many backticks?
            }
            else
            {
                $toRemove = "    `"```````"$($s.Server)```````",,```````"$($s.UserName)```````",```````"$($s.PassHash)```````"`"*"
            }

            if($line -like $toRemove)
            {
                $include = $false
                break
            }
        }

        if($include)
        {
            $filtered += $line
        }
    }

    if($filtered)
    {
        $filtered = AdjustFilterRecordDelimiters $contents $filtered $functionStart $functionEnd
    }

    return $filtered
}

function AdjustFilterRecordDelimiters($contents, $filtered, $functionStart, $functionEnd)
{
    for($i = 0; $i -lt $filtered.Count; $i++)
    {
        if($i -lt $filtered.Count - 1)
        {
            if(!$filtered[$i].EndsWith(","))
            {
                $filtered[$i] = $filtered[$i] + ","
            }
        }
        else
        {
            if($filtered[$i].EndsWith(","))
            {
                $filtered[$i] = $filtered[$i].Substring(0, $filtered[$i].Length - 1)
            }
        }
    }

    return $filtered
}