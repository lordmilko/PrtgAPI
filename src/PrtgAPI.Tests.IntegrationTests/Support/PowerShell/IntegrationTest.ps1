. $PSScriptRoot\Init.ps1

function Describe($name, $script) {

    Pester\Describe $name {
        BeforeAll {
            Startup $name

            LogTest "Running unsafe test '$name'"
        }
        AfterAll {
            try
            {
                LogTest "Completed '$name' tests. Shutting down"
                Shutdown
                LogTest "'$name' shut down successfully"
            }
            finally
            {
                ClearTestName
            }
        }

        & $script
    }
}

function Unsafe($script)
{
    # Protect against operations that can cause PRTG to crash (e.g. deleting a device
    # that is in the middle of performing an auto-discovery in PRTG 18.4)

    try
    {
        & $script
    }
    finally
    {
        LogTestDetail "Checking whether PRTG Crashed"
        Sleep 5

        (ServerManager).StartServices($false)
        (ServerManager).WaitForObjects()
    }
}

function WaitForStatus($sensors, $status, $duration, $refreshInterval = 30)
{
    LogTestDetail "Sleeping for up to $duration seconds for status to turn '$status'"

    $newSensors = $null

    while($duration -gt 0)
    {
        $newSensors = Get-Sensor -Id $sensors.Id

        # If every status matches, we're done
        if(!($newSensors|where Status -ne $status))
        {
            break
        }

        if($duration % $refreshInterval -eq 0)
        {
            $sensors | Refresh-Object
        }

        $duration -= 5
        Sleep 5
    }

    return $newSensors
}