. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Compare-PrtgTree" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    It "compares two trees" {

        $first = DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        $second = DeviceNode -Id 3000 {
            SensorNode -Id 4001
        }

        $comparison = $first | Compare-PrtgTree $second

        $comparison.TreeDifference | Should Be "Removed, Added"
    }

    It "reduces a comparison" {
        $first = DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        $second = DeviceNode -Id 3000 {
            SensorNode -Id 4000,4001
        }

        $withoutReduce = $first | Compare-PrtgTree $second
        $withoutReduce.Children.Count | Should Be 2

        $withReduce = $first | Compare-PrtgTree $second -Reduce
        $withReduce.Children.Count | Should Be 1
    }

    It "only considers specific comparison types" {
        $first = DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        $second = DeviceNode -Id 3000 {
            SensorNode -Id 4001
        }

        $comparison = $first | Compare-PrtgTree $second -Include Added

        $comparison.TreeDifference | Should Be "Added"
        $comparison.Children.Count | Should Be 2

        $reduced = $first | Compare-PrtgTree $second -Include Added -Reduce

        $reduced.TreeDifference | Should Be "Added"
        $reduced.Children.Count | Should Be 1
    }

    It "ignores specific comparison types" {
        $first = DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        $second = DeviceNode -Id 3000 {
            SensorNode -Id 4001
        }

        $comparison = $first | Compare-PrtgTree $second -Ignore Added

        $comparison.TreeDifference | Should Be "Removed"
        $comparison.Children.Count | Should Be 2

        $reduced = $first | Compare-PrtgTree $second -Ignore Added -Reduce

        $reduced.TreeDifference | Should Be "Removed"
        $reduced.Children.Count | Should Be 1
    }

    It "specifies non-conflicting comparison types to include and ignore" {
        $first = DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        $second = DeviceNode -Id 3000 {
            SensorNode -Id 4001
        }

        $comparison = $first | Compare-PrtgTree $second -Include Added -Ignore Removed

        $comparison.TreeDifference | Should Be "Added"
        $comparison.Children.Count | Should Be 2

        $reduced = $first | Compare-PrtgTree $second -Include Added -Reduce

        $reduced.TreeDifference | Should Be "Added"
        $reduced.Children.Count | Should Be 1
    }

    It "specifies conflicting comparison types to include and ignore" {
        $first = DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        $second = DeviceNode -Id 3000 {
            SensorNode -Id 4001
        }

        $comparison = $first | Compare-PrtgTree $second -Include Added,Removed -Ignore Added

        $comparison.TreeDifference | Should Be "Removed"
        $comparison.Children.Count | Should Be 2

        $reduced = $first | Compare-PrtgTree $second -Ignore Added -Reduce

        $reduced.TreeDifference | Should Be "Removed"
        $reduced.Children.Count | Should Be 1
    }
}