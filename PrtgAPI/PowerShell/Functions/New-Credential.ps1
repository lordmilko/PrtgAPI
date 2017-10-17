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