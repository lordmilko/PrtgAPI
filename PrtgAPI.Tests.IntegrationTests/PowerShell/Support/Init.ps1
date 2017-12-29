. "$PSScriptRoot\..\..\..\PrtgAPI.Tests.UnitTests\ObjectTests\PowerShell\Support\Init.ps1"

if(!(Get-Module -ListAvailable Assert))
{
    Install-Package Assert -ForceBootstrap -Force | Out-Null
}

function ServerManager
{
    return [PrtgAPI.Tests.IntegrationTests.BasePrtgClientTest]::ServerManager
}

function Startup
{
    StartupSafe

    (ServerManager).Initialize();
}

function Log($message, $error = $false)
{
    [PrtgAPI.Tests.IntegrationTests.Logger]::Log($message, $error, "PS")
    Write-Host $message
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

function StartupSafe()
{
    Write-Host "Performing startup tasks"
    InitializeModules "PrtgAPI.Tests.IntegrationTests" $PSScriptRoot

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

function InitializePrtgClient
{
    Log "Starting PowerShell Tests"

    (ServerManager).ValidateSettings()
    (ServerManager).StartServices()

    try
    {
        Log "Connecting to PRTG Server"
        Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin) -Force
    }
    catch [exception]
    {
        Log $_.Exception.Message

        if(!($Global:FirstRun))
        {
            $Global:FirstRun = $true
            Log "Sleeping for 30 seconds as its our first test and we couldn't connect..."
            Sleep 30
            Log "Attempting second connection"

            try
            {
                Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin) -Force

                Log "Connection successful"
            }
            catch [exception]
            {
                Log $_.Exception.Message $true
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

function Shutdown
{
    Log "Performing cleanup tasks"
    (ServerManager).Cleanup()

    $global:PreviousTest = $true
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

function It($name, $script) {
    LogTestName "Running test '$name'"

    Pester\It $name {

        try
        {
            & $script
        }
        catch [exception]
        {
            LogTestDetail ($_.Exception.Message -replace "`n"," ") $true

            if($_.Exception.StackTrace -ne $null)
            {
                $stackTrace = $_.Exception.StackTrace -split "`n"

                foreach($line in $stackTrace)
                {
                    LogTestDetail " $line" $true
                }
            }

            throw
        }
    }
}