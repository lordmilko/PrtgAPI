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