function New-Credential
{
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