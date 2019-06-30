function New-PowerShellPackage
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputDir,

        [Parameter(Mandatory = $true)]
        $RepoManager,

        [Parameter(Mandatory = $true)]
        [string]$Configuration,

        [Parameter(Mandatory = $true)]
        [switch]$IsCore
    )

    $RepoManager.WithTempCopy(
        $OutputDir,
        {
            param($tempPath)

            gci "$($tempPath)\*" -Include *.cmd,*.pdb,*.sh | Remove-Item -Force

            Write-LogInfo "`t`tPublishing module to $([PackageManager]::RepoName)"

            $expr = "Publish-Module -Path $tempPath -Repository $([PackageManager]::RepoName) -WarningAction SilentlyContinue"

            # PowerShell Core currently has a bug wherein attempting to execute Start-Process -Wait doesn't work on Windows 7.
            # Work around this by diverting to Windows PowerShell
            if($PSEdition -eq "Core")
            {
                Write-Verbose "Executing powershell -command '$expr'"
                # Clear the PSModulePath to prevent PowerShell Core specific directories contaminating Publish-Module's inner cmdlet lookups
                powershell -command "`$env:PSModulePath = '$env:ProgramFiles\WindowsPowerShell\Modules;$env:SystemRoot\WindowsPowerShell\v1.0\Modules'; $expr"
            }
            else
            {
                Write-Verbose "Executing '$expr'"
                Invoke-Expression $expr
            }
        }
    )
}