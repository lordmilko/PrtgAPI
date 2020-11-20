. "$PSScriptRoot\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\Init.ps1"

if(!(Get-Module -ListAvailable Assert))
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

    Install-Package Assert -ProviderName PowerShellGet -RequiredVersion 0.8.1 -ForceBootstrap -Force | Out-Null
}

function ServerManager
{
    return [PrtgAPI.Tests.IntegrationTests.BasePrtgClientTest]::ServerManager
}

function Startup($testName)
{
    StartupSafe $testName

    (ServerManager).Initialize();
}

function Log($message, $error = $false)
{
    [PrtgAPI.Tests.IntegrationTests.Logger]::Log($message, $error, "PS")
    Write-Host $message
}

function IsEnglish
{
    $properties = Get-ObjectProperty -Id 810 -Raw

    return $properties.languagefile -eq "english.lng"
}

function ForeignMessage($msg)
{
    if(IsEnglish)
    {
        return $msg
    }

    return $null
}

function LogTest($message, $error)
{
    if($error -ne $true)
    {
        $error = $false
    }

    [PrtgAPI.Tests.IntegrationTests.Logger]::LogTest($message, $error, "PS")
    Write-Host $message
}

function LogTestName($message, $error = $false)
{
    [PrtgAPI.Tests.IntegrationTests.Logger]::LogTestDetail($message, $error, "PS")
}

function LogTestDetail($message, $error = $false)
{
    LogTestName "    $message" $error
}

function StartupSafe($testName)
{
    Write-Host "Performing startup tasks"
    InitializeModules "PrtgAPI.Tests.IntegrationTests" $PSScriptRoot

    SetTestName $testName

    if(!(Get-PrtgClient))
    {
        InitializePrtgClient
    }

    if($global:PreviousTest -and !$psISE)
    {
        Log "Sleeping for 30 seconds as tests have run previously"
        Sleep 30

        Log "Refreshing objects"

        Get-Device | Refresh-Object

        Log "Waiting for refresh"
        Sleep 30
    }
    else
    {
        try
        {
            Get-SensorTotals
        }
        catch [exception]
        {
            (ServerManager).StartServices()
            (ServerManager).ValidateSettings()

            Log "PRTG service may still be starting up; pausing 15 seconds"
            Sleep 10

            try
            {
                Get-Sensor | Refresh-Object
                Sleep 5
            }
            catch
            {
                LogTest "Failed to refresh objects: $($_.Exception.Message)"
            }
        }
    }

    (ServerManager).RepairState()
    (ServerManager).WaitForObjects()
}

function SetTestName($name)
{
    if($name.EndsWith("_IT"))
    {
        $name = $name.Substring(0, $name.Length - 3)
    }

    [PrtgAPI.Tests.IntegrationTests.Logger]::PSTestName = $name
}

function ClearTestName
{
    try
    {
        SetTestName $null
    }
    catch
    {
    }
}

function InitializePrtgClient
{
    Log "Starting PowerShell Tests"

    (ServerManager).ValidateSettings()
    (ServerManager).StartServices()

    try
    {
        Log "Connecting to PRTG Server"
        ConnectToServer UserName Password
    }
    catch [exception]
    {
        Log $_.Exception.Message

        if(!($Global:FirstRun))
        {
            $Global:FirstRun = $true
            LogTest "Sleeping for 30 seconds as its our first test and we couldn't connect..."
            Sleep 30
            LogTest "Attempting second connection"

            try
            {
                ConnectToServer UserName Password

                LogTest "Connection successful"
            }
            catch [exception]
            {
                LogTest $_.Exception.Message $true
                throw
            }

            Log "Refreshing all sensors"

            Get-Sensor | Refresh-Object

            Log "Sleeping for 30 seconds"

            Sleep 30
        }
        else
        {
            throw
        }
    }
}

function ConnectToServer($usernameSetting, $passwordSetting)
{
    $server = (Settings ServerWithProto)
    $username = (Settings $usernameSetting)
    $password = (Settings $passwordSetting)

    Connect-PrtgServer $server (New-Credential $username $password) -Force
}

function Shutdown
{
    Log "Performing cleanup tasks"
    (ServerManager).Cleanup()

    $global:PreviousTest = $true
}

function ReadOnlyClient($script)
{
    try
    {
        ConnectToServer ReadOnlyUserName ReadOnlyPassword

        & $script
    }
    finally
    {
        ConnectToServer UserName Password
    }
}

function Settings($property)
{
    $val = [PrtgAPI.Tests.IntegrationTests.Settings]::$property

    if($val -eq $null)
    {
        throw "Setting '$property' could not be found."
    }

    return $val
}

function It {
    [CmdletBinding(DefaultParameterSetName = 'Normal')]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$name,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock] $script,

        [Parameter(Mandatory = $false)]
        [System.Collections.IDictionary[]] $TestCases
    )

    Pester\It $name {

        try
        {
            if($null -eq $TestCases)
            {
                LogTestName "Running test '$name'"

                & $script
            }
            else
            {
                foreach($test in $TestCases)
                {
                    $str = $test["name"]

                    if(!$str)
                    {
                        $matches = $name | sls "<.+?>" -AllMatches | % matches | % Value | foreach { $_.Trim('<', '>') }

                        $v = $matches | select -first 1

                        if($v)
                        {
                            $str = $test[$v]
                        }
                    }

                    LogTestName "Running test '$($name): $str'"

                    & $script @test
                }
            }
        }
        catch [exception]
        {
            $messages = @($_.Exception.Message -split "`n") -replace "`r",""

            foreach($message in $messages)
            {
                LogTestDetail $message $true
            }

            if($_.Exception.StackTrace -ne $null)
            {
                $stackTrace = ($_.Exception.StackTrace -split "`n") -replace "`r",""

                foreach($line in $stackTrace)
                {
                    LogTestDetail " $line" $true
                }
            }

            throw
        }
    }
}