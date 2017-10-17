if(!$script:prtgAPIModule)
{
    . "$PSScriptRoot\..\Resources\PrtgAPI.GoPrtg.ps1"
}

$ErrorActionPreference = "Stop"

function Update-GoPrtgCredential
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "", Scope="Function")]
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        [PSCredential]
        $Credential
    )

    #todo - implement support for using the credential

    UpdateServerRunner {
        param($servers, $targetServer)

        $oldClient = Get-PrtgClient
        $newClient = GetNewClient $Credential

        if($oldClient.UserName -ne $newClient.UserName)
        {
            $duplicateServer = @($servers | Where-Object {$_.Server -eq $newClient.Server -and $_.UserName -eq $newClient.UserName })

            if($duplicateServer.Count -gt 0)
            {
                throw "Cannot update credential: a record with username '$($newClient.UserName)' for server '$($newClient.Server)' already exists"
            }
        }

        #encrypt the new passhash, store it in a variable, pass it in our call to create new line

        $secureString = ConvertTo-SecureString $newClient.PassHash -AsPlainText -Force
        $encryptedString = ConvertFrom-SecureString $secureString

        return {
            param($row, $createRow)

            return & $createRow $row.Server $row.Alias $newClient.UserName $encryptedString
        }.GetNewClosure()
    }

    $client = Get-PrtgClient

    Write-ColorOutput "`nSuccessfully updated credential" -ForegroundColor Green

    Write-ColorOutput "`nConnected to $($client.Server) as $($client.UserName)`n" -ForegroundColor Green

    #things to validate: if the user changes the credential username, need to verify that there isnt an existing record with the same username (in the same way we're already doing this with install-goprtg - check how it works)
}

function GetNewClient($credential)
{
    $oldClient = Get-PrtgClient

    if($null -eq $credential)
    {
        $credential = GetNewCredential $oldClient.UserName
    }

    Connect-PrtgServer $oldClient.Server $credential -Force

    return Get-PrtgClient
}

# Visual Studio Test Engine doesn't support Get-Credential, so we'll mock GetNewCredential instead
function GetNewCredential($username)
{
    return Get-Credential -UserName $username -Message "Enter your credentials."
}