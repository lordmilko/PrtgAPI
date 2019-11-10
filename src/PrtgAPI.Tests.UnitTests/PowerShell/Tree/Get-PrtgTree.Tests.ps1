. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Get-PrtgTree" -Tag @("PowerShell", "UnitTest") {

    Invoke-Expression @'
class TreeVisitor : PrtgAPI.Tree.PrtgNodeWalker
{
    $Names = @()

    [void]VisitSensor([PrtgAPI.Tree.SensorNode]$node)
    {
        $this.Names += $node.Name
    }
}
'@

    Context "Object" {
        It "doesn't specify an object" {
            SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

            $result = Get-PrtgTree

            $result | Should Not BeNullOrEmpty
        }

        It "pipes from an object" {
            SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

            $result = Get-Group -Id 0 | Get-PrtgTree

            $result | Should Not BeNullOrEmpty
        }
    }

    Context "Manual" {
        It "specifies an ID" {
            SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

            $result = Get-PrtgTree -Id 0

            $result | Should Not BeNullOrEmpty
        }
    }


    It "implements a custom visitor" {

        SetMultiTypeResponse

        $node = DeviceNode -Id 3000 {
            SensorNode -Id 4000,4001
        }

        $visitor = [TreeVisitor]::new()

        $visitor.Visit($node)

        $visitor.Names -join ", " | Should Be "Volume IO _Total0, Volume IO _Total1"
    }
}