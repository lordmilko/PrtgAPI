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
            SetResponseAndClientWithArguments "TreeRequestResponse" "FastPath"

            $result = Get-PrtgTree

            $result | Should Not BeNullOrEmpty
        }

        It "pipes from an object" {
            SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

            Get-Object -Id 1001 # Hack to match up with the TreeRequestResponse

            $result = Get-Probe -Id 1001 | Get-PrtgTree

            $result | Should Not BeNullOrEmpty
        }
    }

    Context "Manual" {
        It "specifies an ID" {
            SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

            $result = Get-PrtgTree -Id 1001

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

    It "retrieves lazily" {
        SetAddressValidatorResponse @(
            [Request]::Groups("filter_objid=0", [Request]::DefaultObjectFlags)
        )

        $tree = Get-PrtgTree -Lazy
    }

    It "specifies options" {
        SetResponseAndClientWithArguments "TreeRequestResponse" "AllObjectTypes"

        $tree = Get-PrtgTree -Options All
    }
}