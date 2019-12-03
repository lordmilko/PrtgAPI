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

    $expected = "Write-Host `"hello`"$nl$baseExpected"

    if($multiLine)
    {
        $expected = "Write-Host `"hello`"$($nl)Write-Host `"what what?`"$nl$baseExpected"
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

    $nl = [Environment]::NewLine

    $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
    $expected += "function __goPrtgGetServers {@($nl"
    $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",$nl"
    $expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"$nl"
    $expected += ")}$nl$nl"
    $expected += "############################ End GoPrtg Servers ############################$nl"

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

    $nl = [Environment]::NewLine

    $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
    $expected += "function __goPrtgGetServers {@($nl"
    $expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`",$nl"
    $expected += "    `"```"prtg.example2.com```",```"dev```",```"username2```",```"*```"`"$nl"
    $expected += ")}$nl$nl"
    $expected += "############################ End GoPrtg Servers ############################$nl"

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
    if($Profile)
    {
        $script:originalProfile = $Profile
    }

    $Global:Profile = Join-Path $TestDrive "Microsoft.PowerShell_profile.ps1"

    InitializeModules "PrtgAPI.Tests.UnitTests" $PSScriptRoot
}

function GoPrtgAfterAll
{
    if($script:originalProfile)
    {
        $Global:Profile = $script:originalProfile
    }
}

function GoPrtgBeforeEach
{
    if (Test-Path $Profile)
    {
        if (Test-Path "$Profile.tmp")
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

    # Set the PrtgClient via reflection rather than Connect-PrtgServer to avoid potential conflicts with the mock in Update-GoPrtgCredential.Tests
    $newClient = New-Object PrtgAPI.PrtgClient -ArgumentList ("prtg.example.com", "username", "passhash", [PrtgAPI.AuthMode]::PassHash)

    SetPrtgClient $newClient
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