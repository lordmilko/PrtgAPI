. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Add-Group_IT" {
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
}