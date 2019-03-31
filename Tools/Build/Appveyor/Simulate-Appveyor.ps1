function Simulate-Appveyor
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    if($env:APPVEYOR)
    {
        throw "Simulate-Appveyor should not be run from within Appveyor"
    }

    InitializeEnvironment

    Clear-AppveyorBuild $IsCore

    Invoke-AppveyorInstall             # install            Install Chocolatey packages, NuGet provider for NuGet testing
    Invoke-AppveyorBeforeBuild $IsCore # before_build       Restore NuGet packages
    Invoke-AppveyorBuild $IsCore       # build_script       Build for all target frameworks
    Invoke-AppveyorAfterBuild          # after_build        Set Appveyor build from PrtgAPI version
    Invoke-AppveyorBeforeTest $IsCore  # before_test        Build/test NuGet
    Invoke-AppveyorTest $IsCore        # test_script        Test .NET and Pester
    Invoke-AppveyorAfterTest $IsCore   # after_test         .NET Coverage
}

function Simulate-Environment($scriptBlock)
{
    InitializeEnvironment

    & $scriptBlock
}

function InitializeEnvironment
{
    $env:CONFIGURATION = "Debug"
    $env:APPVEYOR_BUILD_FOLDER = $script:SolutionDir
    $env:APPVEYOR_REPO_COMMIT_MESSAGE = 'Did some stuff'
    $env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED = 'For #4'
    $env:APPVEYOR_ACCOUNT_NAME = "lordmilko"
    $env:APPVEYOR_PROJECT_SLUG = "prtgapi"

    #Get-AppveyorLocalConfig | Out-Null
}