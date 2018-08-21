. $PSScriptRoot\Init.ps1

function Describe($name, $script) {

    $init = $false

    Pester\Describe $name {
        BeforeAll { 
            StartupSafe $name

            $init = $true

            LogTest "Running safe test '$name'"
        }
        AfterAll {
            if($init)
            {
                LogTest "Completed '$name' tests; no need to clean up"
            }

            ClearTestName
        }

        & $script
    }
}