function Invoke-TravisInstall
{
    Install-CIDependency Pester -Log -SilentSkip:$false
}