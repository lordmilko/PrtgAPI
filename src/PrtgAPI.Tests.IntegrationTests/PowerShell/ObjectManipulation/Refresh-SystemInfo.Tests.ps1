. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

function RefreshAndValidate($deviceId, $type)
{
    $original = Get-SystemInfo -Id $deviceId $type
    $originalLastUpdated = $original | Select -First 1 -ExpandProperty LastUpdated

    $originalLastUpdated | Should Not BeNullOrEmpty

    Refresh-SystemInfo -Id $deviceId $type

    for($i = 0; $i -lt 10; $i++)
    {
        $new = Get-SystemInfo -Id $deviceId $type
        $newLastUpdated = $new | Select -First 1 -ExpandProperty LastUpdated

        if($newLastUpdated -gt $originalLastUpdated)
        {
            break
        }
        else
        {
            LogTestDetail "'$type' information wasn't refreshed; sleeping for 20 seconds"
            Sleep 20
        }
    }

    $newLastUpdated | Should BeGreaterThan $originalLastUpdated
}

Describe "Refresh-SystemInfo_IT" -Tag @("PowerShell", "IntegrationTest") {

    $cases = @(
        @{name="system"}
        @{name="software"}
        @{name="hardware"}
        @{name="users"}
        @{name="processes"}
        @{name="services"}
    )

    It "refreshes <name> info" -TestCases $cases {
        param($name)

        RefreshAndValidate (Settings Device) $name
    }
}