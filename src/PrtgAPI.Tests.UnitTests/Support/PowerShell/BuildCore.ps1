if(!$skipBuildModule)
{
    ipmo $PSScriptRoot\..\..\..\..\build\PrtgAPI.Build -Scope Local
}

ipmo $PSScriptRoot\..\..\..\..\build\CI\ci.psm1 -Scope Local

Install-CIDependency pester

if(!(gmo pester))
{
    ipmo pester
}

if((gmo pester).Version.Major -gt 3 -and !(gcm Assert-VerifiableMocks -CommandType alias -ErrorAction SilentlyContinue))
{
    New-Alias Assert-VerifiableMocks Assert-VerifiableMock -Scope Global
}

if($PSEdition -eq "Core" -and !$IsWindows)
{
    # Progress bars seem to hurt script performance on Linux
    $global:ProgressPreference = "SilentlyContinue"
}

function WithoutTestDrive($script)
{
    $drive = Get-PSDrive TestDrive -Scope Global

    $drive | Remove-PSDrive -Force
    Remove-Variable $drive.Name -Scope Global -Force

    try
    {
        & $script
    }
    finally
    {
        New-PSDrive $drive.Name -PSProvider $drive.Provider -Root $drive.Root -Scope Global
        New-Variable $drive.Name -Scope Global -Value $drive.Root
    }
}

function Get-SolutionRoot
{
    return Resolve-Path "$PSScriptRoot\..\..\..\.."
}

function Get-SourceRoot
{
    Join-Path (Get-SolutionRoot) "src"
}

function Get-ProcessTree
{
    if(!(gcm gwmi -ErrorAction SilentlyContinue))
    {
        return @()
    }

    $all = gwmi win32_process

    $list = @()

    while($true)
    {
        if($me -eq $null)
        {
            if($list.Count -eq 0)
            {
                $me = $all|where ProcessID -eq $pid
            }
            else
            {
                break
            }
        }
        else
        {
            $me = $all|where ProcessID -eq $me.ParentProcessID

            if($list|where ProcessID -eq $me.ProcessID)
            {
                # We're about to get stuck in a loop. Looks like a process ID
                # has been reused (such as the parent of explorer.exe)
                break
            }
        }

        $list += $me
    }

    return $list
}

function IsChildOf
{
    param(
        [string[]]$Name
    )

    if(Test-IsWindows)
    {
        $tree = Get-ProcessTree

        foreach($item in $tree)
        {
            foreach($n in $Name)
            {
                if($n -eq $item.Name)
                {
                    return $true
                }
            }
        }

        return $false
    }
    else
    {
        return $false
    }
}

function SkipBuildTest
{
    IsChildOf devenv.exe
}

function WithNewProcess($command, $exe = $null)
{
    $path = Join-Path $PSScriptRoot "BuildCore.ps1"
    $fixPath = $null

    if($null -eq $exe)
    {
        $exe = "pwsh"

        if((Test-IsWindows) -and $PSEdition -ne "Core")
        {
            $exe = "powershell"
        }
    }

    if($exe -eq "powershell" -and $PSEdition -eq "Core")
    {
        $fixPath = "`$env:PSModulePath = '$env:ProgramFiles\WindowsPowerShell\Modules;$env:SystemRoot\system32\WindowsPowerShell\v1.0\Modules'; remove-module microsoft.powershell.utility; ipmo Microsoft.PowerShell.Utility"
    }

    & $exe -NonInteractive -Command "$fixPath; . '$path'; $command" | Write-Host

    if($LASTEXITCODE -ne 0)
    {
        throw "Invocation of command '$command' failed. Check PrtgAPI.Build.log for details"
    }
}