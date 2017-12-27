. $PSScriptRoot\UnitTest.ps1

function Describe($name, $script)
{
    Pester\Describe $name {

        BeforeAll { InitializeUnitTestModules }

        & $script
    }
}