<#function New-TestSettings
{

    #import settings and, if it exists, settings.local. then do reflection and analyze all the values
    #we wrote some code in basetest or something which validates every field has a value. property must not be null or the string -1
    #todo: also need to use that validation function before integration tests in general (albiet from the imported module, not importing a cs file)

    #need to have the nuget.org package source, and then need to install codedom

    #check whether we have the nuget.org provider, or even if we do check its the right version. prompt the user if theyd like to install/update

    #todo: need to make codedom path dynamic

    #i think we need to do this in a separate powershell.exe process so we can add-type again on the next run

    write-host "$PSScriptRoot"

    $default = (gc -Path "$PSScriptRoot\Support\Settings.cs"|where {$_ -notlike "*#pragma*"}) -join ([Environment]::NewLine)
    $localPath = "$PSScriptRoot\Settings.Local.cs"

    #Add-Type -TypeDefinition $default

    if(Test-Path $localPath)
    {
        $local = (gc -Path $localPath|where { $_ -NotLike "using*"}) -join ([Environment]::NewLine)

        $default += $local

        Add-Type -TypeDefinition $default
    }

    $type = [PrtgAPI.Tests.IntegrationTests.Settings]

    $fields = $type.GetFields()

    $type::Probe = (Get-Probe).Id
    $type::Group = (Get-Group Servers).Id
}

class TestSettings
{
    $type

    Main()
    {
        $this.SetProperty("Probe", { (Get-Probe).Id })
    }

    SetProperty($name, [ScriptBlock]$setter)
    {
        $this.type::$name = & $setter
    }
}

function 
{

}

New-TestSettings#>

throw "Not currently supported"