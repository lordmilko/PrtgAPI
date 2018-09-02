. $PSScriptRoot\Init.ps1

#region Support

function InstallInEmptyProfile($baseExpected)
{
    New-Item $Profile -Type File -Force

    Install-GoPrtgServer

    $content = gc $Profile -Raw

    $content | Should BeLike $baseExpected
}

function InstallInProfileWithContent($baseExpected, $multiLine)
{
    New-Item $Profile -Type File -Force

    Add-Content $Profile "Write-Host `"hello`""

    if($multiLine)
    {
        Add-Content $Profile "Write-Host `"what what?`""
    }

    Install-GoPrtgServer

    $content = gc $Profile -Raw

    $expected = "Write-Host `"hello`"`r`n$baseExpected"

    if($multiLine)
    {
        $expected = "Write-Host `"hello`"`r`nWrite-Host `"what what?`"`r`n$baseExpected"
    }

    $content | Should BeLike $expected
}

function InstallMultipleInProfile
{
    Install-GoPrtgServer

    try
    {
        Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

        Install-GoPrtgServer
    }
    finally
    {
        Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
    }

    $content = gc $Profile -Raw

    $expected = "########################### Start GoPrtg Servers ###########################`r`n`r`n"
    $expected += "function __goPrtgGetServers {@(`r`n"
    $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",`r`n"
    $expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"`r`n"
    $expected += ")}`r`n`r`n"
    $expected += "############################ End GoPrtg Servers ############################`r`n"

    $expected = $expected.Replace("``", "````")

    $content | Should BeLike $expected
}

function InstallMultipleWithAlias
{
    Install-GoPrtgServer prod

    try
    {
        Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

        Install-GoPrtgServer dev
    }
    finally
    {
        Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
    }

    $content = gc $Profile -Raw

    $expected = "########################### Start GoPrtg Servers ###########################`r`n`r`n"
    $expected += "function __goPrtgGetServers {@(`r`n"
    $expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`",`r`n"
    $expected += "    `"```"prtg.example2.com```",```"dev```",```"username2```",```"*```"`"`r`n"
    $expected += ")}`r`n`r`n"
    $expected += "############################ End GoPrtg Servers ############################`r`n"

    $expected = $expected.Replace("``", "````")

    $content | Should BeLike $expected
}

function InstallInProfileFunctionWithoutHeaderFooter
{
    Install-GoPrtgServer

    $contents = gc $Profile

    $newContents = $contents | where {
        $_ -ne "########################### Start GoPrtg Servers ###########################" -and
        $_ -ne "############################ End GoPrtg Servers ############################"
    }

    Set-Content $Profile $newContents
}

#endregion
#region Init

function GoPrtgBeforeAll
{
    if(!$Profile)
    {
        $Global:Profile = "$TestDrive\Microsoft.PowerShell_profile.ps1"
    }

    InitializeModules "PrtgAPI.Tests.UnitTests" $PSScriptRoot
}

function GoPrtgBeforeEach
{
    if(Test-Path $Profile)
    {
        if(Test-Path "$Profile.tmp")
        {
            throw "Cannot create temp profile; $Profile.tmp already exists"
        }
        else
        {
            mv $Profile "$Profile.tmp"
        }
    }

    if(Get-Command __goPrtgGetServers -ErrorAction SilentlyContinue)
    {
        Remove-Item Function:\__goPrtgGetServers
    }

    if(Test-Path $Profile)
    {
        throw "Could not rename PowerShell Profile"
    }

    Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
}

function GoPrtgAfterEach
{
    if(Test-Path $Profile)
    {
        Remove-Item $Profile -Force
    }

    if(Test-Path "$Profile.tmp")
    {
        mv "$Profile.tmp" $Profile
    }

    Disconnect-PrtgServer
}

#endregion