. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Add-Group_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "adds a new group" {
        
        $name = "newGroup"

        $params = New-GroupParameters $name

        $probe = Get-Probe -Id (Settings Probe)

        $probe | Add-Group $params

        $group = @(Get-Group $name)
        $group.Count | Should Be 1
        $group.Name | Should Be $name

        $group | Remove-Object -Force
    }

    It "resolves a new group" {
        $name = "resolveGroup"

        $probe = Get-Probe -Id (Settings Probe)

        $originalGroups = Get-Group

        $newGroup = $probe | Add-Group $name

        $newGroups = Get-Group

        $newGroups.Count | Should BeGreaterThan $originalGroups.Count

        $diffGroup = $newGroups|where name -EQ $name

        $diffGroup.Id | Should Be $newGroup.Id

        $newGroup | Remove-Object -Force
    }
}