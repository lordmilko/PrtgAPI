<#
.SYNOPSIS
Simulates building PrtgAPI under a Continuous Integration environment

.DESCRIPTION
The Simulate-PrtgCI simulates the entire workflow of building PrtgAPI under either Appveyor or Travis CI. By default, Simulate-PrtgCI will invoke all steps that would normally be performed as part of the CI process. This can be limited by specifying a specific list of tasks that should be simulated via the -Task parameter.

.PARAMETER Appveyor
Specifies to simulate Appveyor CI

.PARAMETER Travis
Specifies to simulate Travis CI

.PARAMETER Task
CI task to execute. If no value is specified, all CI tasks will be executed.

.PARAMETER Legacy
Specifies whether to use .NET Core CLI or legacy .NET infrastructure when simulating CI tasks

.EXAMPLE
C:\> Simulate-PrtgCI
Simulate Appveyor CI

.EXAMPLE
C:\> Simulate-PrtgCI -Travis
Simulate Travis CI

.EXAMPLE
C:\> Simulate-PrtgCI -Task Test
Simulate Appveyor CI tests
#>
function Test-PrtgCI
{
    [CmdletBinding(DefaultParameterSetName = "Appveyor")]
    param(
        [Parameter(Mandatory = $false, ParameterSetName = "Appveyor")]
        [switch]$Appveyor,

        [Parameter(Mandatory = $true, ParameterSetName = "Travis")]
        [switch]$Travis,

        [Parameter(Mandatory = $false, Position = 0, ParameterSetName="Appveyor")]
        [ValidateSet("Install", "Restore", "Build", "Package", "Test", "Coverage")]
        [string[]]$Task,

        [Parameter(Mandatory=$false)]
        [Configuration]$Configuration = "Debug",

        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy
    )

    switch($PSCmdlet.ParameterSetName)
    {
        "Appveyor" {

            if(!(Test-IsWindows))
            {
                throw "Appveyor can only be simulated on Windows"
            }

            Set-AppveyorBuildMode -IsCore:(-not $Legacy)

            if($null -eq $Task)
            {
                Simulate-Appveyor -Configuration $Configuration
            }
            else
            {
                Simulate-Environment {
                    if("Install" -in $Task) {
                        Invoke-AppveyorInstall
                    }
                    if("Restore" -in $Task) {
                        Invoke-AppveyorBeforeBuild
                    }
                    if("Build" -in $Task) {
                        Invoke-AppveyorBuild
                    }
                    if("Package" -in $Task) {
                        Invoke-AppveyorBeforeTest
                    }
                    if("Test" -in $Task) {
                        Invoke-AppveyorTest
                    }
                    if("Coverage" -in $Task)
                    {
                        Invoke-AppveyorAfterTest
                    }
                } -Configuration $Configuration
            }
        }
        "Travis" {
            Simulate-Travis -Configuration $Configuration
        }
    }
}