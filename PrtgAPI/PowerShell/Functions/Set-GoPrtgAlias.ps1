if(!$prtgAPIModule)
{
	. "$PSScriptRoot\..\Resources\PrtgAPI.GoPrtg.ps1"
}

function Set-GoPrtgAlias
{
	[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseShouldProcessForStateChangingFunctions", "", Scope="Function")]
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $false, Position = 0)]
		[string]
		$Alias
	)

	UpdateServerRunner {
        param($servers, $targetServer)

		$client = Get-PrtgClient

        if($servers | Where-Object Alias -EQ $Alias)
        {
	        throw "Cannot set alias for server '$($client.Server)': a record with alias '$Alias' already exists. For more information see Get-GoPrtgServer."
        }

        if(!$Alias -and $targetServer.Alias -ne $null)
        {
	        if(($servers | Where-Object {$_.Server -eq $client.Server}).Count -gt 1)
	        {
		        throw "Cannot remove alias of server: multiple entries for server '$($client.Server)' are stored within GoPrtg. To remove this alias uninstall all other entries for this server. For more information see Get-GoPrtgServer."
	        }
        }

        # We can't access the outer closure from the second level closure below
        $alias = $Alias

        return {
            param($row, $createRow)

	        return & $createRow $row.Server $alias $row.UserName $row.PassHash
        }.GetNewClosure()
    }.GetNewClosure()
}