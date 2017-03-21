. $PSScriptRoot\Init.ps1

function Describe($name, $script) {

    Pester\Describe $name {
		BeforeAll { StartupSafe }

		& $script
	}
}