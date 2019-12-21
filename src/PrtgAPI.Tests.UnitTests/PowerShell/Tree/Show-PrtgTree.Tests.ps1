. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Show-PrtgTree" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    Context "Tree" {
        It "pipes a PrtgNode" {

            $node = DeviceNode -Id 3000

            $node | Show-PrtgTree
        }

        It "pipes a CompareNode" {

            $tree = DeviceNode -Id 3000
            $comparison = $tree.CompareTo($tree)
            $comparison | Show-PrtgTree
        }

        It "pipes the children of a PrtgNode" {

            $tree = DeviceNode -Id 3000 {
                SensorNode -Id 4000,4001
            }

            $tree.Children.Count | Should Be 2

            $tree.Children | Show-PrtgTree
        }

        It "pipes the children of a CompareNode" {

            $tree = DeviceNode -Id 3000 {
                SensorNode -Id 4000,4001
            }

            $comparison = $tree.CompareTo($tree)
            $comparison.Children.Count | Should Be 2

            $comparison | Show-PrtgTree
        }

        It "reduces a CompareNode" {
            $first = DeviceNode -Id 3000 {
                SensorNode -Id 4000
            }

            $second = DeviceNode -Id 3000 {
                SensorNode -Id 4001
            }

            $comparison = $first.CompareTo($second)

            $comparison.Reduce() | Should Not BeNullOrEmpty

            $comparison | Show-PrtgTree -Reduce
        }

        It "reduces a CompareNode to nothing" {
            $tree = DeviceNode -Id 3000 {
                SensorNode -Id 4000
            }

            $comparison = $tree.CompareTo($tree)

            $comparison.Reduce() | Should BeNullOrEmpty

            $comparison | Show-PrtgTree -Reduce
        }

        It "throws attempting to reduce a PrtgNode" {
            $tree = DeviceNode -Id 3000

            { $tree | Show-PrtgTree -Reduce } | Should Throw "tree does not support reduction"
        }
    }

    Context "Object" {
        It "constructs a tree from an object" {

            SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

            Get-Object -Id 1001 # Hack to match up with the TreeRequestResponse

            Get-Probe -Id 1001 | Show-PrtgTree
        }
    }

    Context "Manual" {
        It "constructs a tree from an ID" {
            SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

            Show-PrtgTree -Id 1001
        }
    }
}