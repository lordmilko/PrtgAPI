function Get-CIDependency
{
    # If you add a new entry here also make sure to add it to Simulate-PrtgCI.Tests.ps1, Install-PrtgDependency.ps1 and
    # Install-PrtgDependency.Tests.ps1 (including both the standalone test and the test as part of all dependencies)
    $dependencies = @(
        @{ Name = "chocolatey";               Chocolatey = $true;      MinimumVersion = "0.10.5.0";  Manager = $true }
        @{ Name = "dotnet";                   Dotnet     = $true }
        @{ Name = "codecov";                  Chocolatey = $true }
        @{ Name = "opencover.portable";       Chocolatey = $true;      MinimumVersion = "4.7.922.0"; CommandName = "opencover.console" }
        @{ Name = "reportgenerator.portable"; Chocolatey = $true;      MinimumVersion = "3.0.0.0";   CommandName = "reportgenerator" }
        @{ Name = "vswhere";                  Chocolatey = $true;      MinimumVersion = "2.6.7" }
        @{ Name = "NuGet.CommandLine";        Chocolatey = $true;      MinimumVersion = "5.2.0";     CommandName = "nuget" }
        @{ Name = "NuGetProvider";            PackageProvider = $true; MinimumVersion = "2.8.5.201" }
        @{ Name = "PowerShellGet";            PowerShell = $true;      MinimumVersion = "2.0.0" }
        @{ Name = "Pester";                   PowerShell = $true;      MinimumVersion = "3.4.5";     Version = "3.4.6"; SkipPublisherCheck = $true }
        @{ Name = "PSScriptAnalyzer";         PowerShell = $true }
        @{ Name = "net452";                   TargetingPack = $true;   Version = "4.5.2" }
        @{ Name = "net461";                   TargetingPack = $true ;  Version = "4.6.1" }
    )

    if($PSEdition -eq "Core" -and !$IsWindows)
    {
        # We want to be able to test PrtgAPI.Build on Linux, however the advanced mocking
        # required by these tests won't work in Pester 3, so when we're actually on Linux
        # use Pester 4 instead. We go with 4.7.2 because 4.7.3 truncates "Should Be" output
        # to 5 characters which is useless
        $pester = $dependencies|where { $_["Name"] -eq "Pester" }
        $pester.MinimumVersion = "4.7.0"
        $pester.Version = "4.7.2"
    }

    return $dependencies
}
