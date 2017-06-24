. $PSScriptRoot\Init.ps1

function Describe($name, $script) {

    Pester\Describe $name {
		BeforeAll {
			Startup

			LogTest "Running unsafe test '$name'"
		}
		AfterAll { Shutdown }

		& $script
	}
}