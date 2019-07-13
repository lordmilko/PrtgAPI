function Clear-CIBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        $BuildFolder,

        [Parameter(Mandatory = $false, Position = 1)]
        $Configuration = $env:CONFIGURATION,

        [switch]$IsCore,

        [switch]$NuGetOnly
    )

    if(!$NuGetOnly)
    {
        if($IsCore)
        {
            Install-CIDependency dotnet

            $path = (Join-Path $BuildFolder PrtgAPIv17.sln)

            $cleanArgs = @(
                "clean"
                "`"$path`""
                "-c"
                $Configuration
            )

            if(!(Test-IsWindows))
            {
                # Running dotnet clean on Linux can result in ResolvePackageAsserts
                # unexpectedly failing due to something to do with the NuGet fallback cache.
                # Executing a package restore resolves this problem.
                Invoke-Process { dotnet restore $path }
            }            

            Write-Verbose "Executing command 'dotnet $cleanArgs'"

            Invoke-Process { dotnet @cleanArgs } -WriteHost
        }
        else
        {
            $path = Join-Path $BuildFolder PrtgAPI.sln

            $msbuild = Get-MSBuild

            $msbuildArgs = @(
                "/t:clean"
                "`"$path`""
                "/p:Configuration=$Configuration"
            )

            Write-Verbose "Executing command '$msbuild $msbuildArgs'"

            Invoke-Process { & $msbuild @msbuildArgs } -WriteHost
        }
    }

    $nupkgs = @(gci $BuildFolder -Recurse -Filter *.*nupkg | where {
        !$_.FullName.StartsWith((Join-Path $BuildFolder "packages"))
    })
    
    $nupkgs += (gci $BuildFolder -Filter *.zip)
    
    $nupkgs | foreach {
        Write-LogError "`tRemoving $($_.FullName)"

        $_ | Remove-Item -Force
    }
}