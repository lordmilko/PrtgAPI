. $PSScriptRoot\Support\Standalone.ps1

Describe "Acknowledge-Sensor" {

    SetActionResponse

    $sensor = Run Sensor { Get-Sensor }


    It "acknowledges for a duration" {

        $sensor | Acknowledge-Sensor -Duration 10
    }

    It "acknowledges until a specified time" {
        $sensor | Acknowledge-Sensor -Until (get-date).AddDays(1)
    }

    It "acknowledges forever" {
        $sensor | Acknowledge-Sensor -Forever
    }

    It "acknowledges with a message" {
        $sensor | Acknowledge-Sensor -Duration 10 -Message "Acknowledging object!"
    }
}