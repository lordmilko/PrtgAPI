. $PSScriptRoot\Support\Standalone.ps1

function SetCloneResponse
{
	$client = [PrtgAPI.Tests.UnitTests.ObjectTests.BaseTest]::Initialize_Client((New-Object PrtgAPI.Tests.UnitTests.ObjectTests.Responses.CloneResponse))

	SetPrtgClient $client
}

Describe "Clone-Sensor" {

    SetCloneResponse

    It "Retries resolving an object" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor.Count | Should Be 1

        $output = [string]::Join("`n",(&{try { $sensor | Clone-Sensor 1234 3>&1 | %{$_.Message} } catch [exception] { }}))

        $expected = "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 5`n" +
                    "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 4`n" +
                    "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 3`n" +
                    "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 2`n" +
                    "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 1"

        $output | Should Be $expected
    }
}