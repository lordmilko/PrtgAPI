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

    if(![string]::IsNullOrEmpty($Password))
    {
        $secureString = ConvertTo-SecureString $Password -AsPlainText -Force
    }
    else
    {
        $secureString = New-Object SecureString
    }

    New-Object System.Management.Automation.PSCredential -ArgumentList $UserName, $secureString
}