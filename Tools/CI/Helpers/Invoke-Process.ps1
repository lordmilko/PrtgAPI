# From https://github.com/PowerShell/PowerShell/blob/master/build.psm1

function Invoke-Process
{
    param(
        [scriptblock]$ScriptBlock,
        [switch]$IgnoreExitCode,
        [switch]$WriteHost
    )

    $backupEAP = $script:ErrorActionPreference
    $script:ErrorActionPreference = "Continue"

    try
    {
        if($WriteHost)
        {
            $output = & $ScriptBlock | Merge-Stream
        }
        else
        {
            $output = & $ScriptBlock
        }

        # Note, if $ScriptBlock doesn't have a native invocation, $LASTEXITCODE will
        # point to the obsolete value
        if ($LASTEXITCODE -ne 0 -and -not $IgnoreExitCode)
        {
            if($WriteHost)
            {
                throw "Execution of {$ScriptBlock} failed with exit code $($LASTEXITCODE)"
            }

            # Get caller location for easier debugging
            $caller = Get-PSCallStack -ErrorAction SilentlyContinue

            if($caller)
            {
                $callerLocationParts = $caller[1].Location -split ":\s*line\s*"
                $callerFile = $callerLocationParts[0]
                $callerLine = $callerLocationParts[1]

                $errorMessage = "Execution of {$ScriptBlock} by ${callerFile}: line $callerLine failed with exit code $($LASTEXITCODE):`n$($output -join "`n")"
                throw $errorMessage
            }

            throw "Execution of {$ScriptBlock} failed with exit code $($LASTEXITCODE): $output"
        }
    }
    finally
    {
        $script:ErrorActionPreference = $backupEAP
    }
}

function Merge-Stream
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, ValueFromPipeline = $true)]
        $Object
    )

    Process {
        $Object
        $Object | Write-Host
    }
}