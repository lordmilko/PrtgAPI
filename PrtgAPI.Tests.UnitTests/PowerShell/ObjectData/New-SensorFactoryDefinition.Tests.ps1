. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-SensorFactoryDefinition" -Tag @("PowerShell", "UnitTest") {

    $sensors = Run "Sensor" {
        WithItems((GetItem), (GetItem)) {
            Get-Sensor
        }
    }

    It "uses a name and ID" {

        $expected = "#1:Volume IO _Total`n" +
                    "channel(2203,1)`n" +
                    "#2:Volume IO _Total`n" +
                    "channel(2203,1)"

        (
            $sensors | New-SensorFactoryDefinition {$_.Name} 1

        ) -Join "`n" | Should Be $expected
    }

    It "creates a sensor factory with a custom unit" {

        $expected = "#1:dc1 [bananas]`n" +
                    "channel(2203,0)`n" +
                    "#2:dc1 [bananas]`n" +
                    "channel(2203,0)"

        (
            $sensors | New-SensorFactoryDefinition { "$($_.Device) [bananas]" } 0

        ) -join "`n" | Should Be $expected
    }

    It "specifies a custom start index" {

        $expected = "#2:dc1`n" +
                    "channel(2203,2)`n" +
                    "#3:dc1`n" +
                    "channel(2203,2)"

        (
            $sensors | New-SensorFactoryDefinition { $_.Device } 2 -StartId 2

        ) -join "`n" | Should Be $expected
    }

    Context "Expression" {

        It "uses a name, expression (with the default expression) and ID" {

            $expected = "#1:Volume IO _Total`n" +
                        "100 - channel(2203,2)`n" +
                        "#2:Volume IO _Total`n" +
                        "100 - channel(2203,2)"

            (
                $sensors | New-SensorFactoryDefinition {$_.Name} -Expr {"100 - $expr"} 2

            ) -join "`n" | Should Be $expected
        }

        It "uses a name, expression (with the current item) and ID" {

            $expected = "#1:Volume IO _Total`n" +
                        "channel(2203,100)`n" +
                        "#2:Volume IO _Total`n" +
                        "channel(2203,100)"
        
            (
                $sensors | New-SensorFactoryDefinition {$_.Name} -Expr {"channel($($_.Id),100)"} 1

            ) -join "`n" | Should Be $expected
        }

        It "uses a string name with an expression" {

            $expected = "#1:test`n" +
                        "channel(2203,100)`n" +
                        "#2:test`n" +
                        "channel(2203,100)"

            (
                $sensors | New-SensorFactoryDefinition "test" -Expr { "channel($($_.Id),100)" } 1

            ) -join "`n" | Should Be $expected
        }
    }

    Context "Aggregation" {

        It "aggregates (with the default expression) with a name and ID" {

            $expected = "#1:Sum`n" +
                        "channel(2203,4) + channel(2203,4)"
        
            (
                $sensors | New-SensorFactoryDefinition {"Sum"} -Aggregator {"$acc + $expr"} 4

            ) -join "`n" | Should Be $expected
        }

        It "aggregates (with the current item) with a name, expression and ID" {

            $expected = "#1:Sum`n" +
                        "100 - channel(2203,5) + channel(2203,20)"
            (
                $sensors | New-SensorFactoryDefinition {"Sum"} -Expression {"100 - channel($($_.Id),5)"} -Aggregator {"$acc + channel($($_.Id),20)"} 10

            ) -join "`n" | Should Be $expected
        }

        It "aggregates a definition by calculating the max value" {

            $s = Run "Sensor" {
                WithItems((GetItem), (GetItem), (GetItem)) {
                    Get-Sensor
                }
            }
        
            $expected = "#1:Max Value`n" +
                        "max(channel(2203,3),max(channel(2203,3),channel(2203,3)))"

            (
                $s | New-SensorFactoryDefinition {"Max Value"} -Aggregator {"max($expr,$acc)"} 3

            ) -join "`n" | Should Be $expected
        }

        It "creates an aggregation and a list of all channels" {

            $s = Run "Sensor" {
                WithItems((GetItem), (GetItem), (GetItem)) {
                    Get-Sensor
                }
            }

            $expected = "#1:Max Value`n" +
                        "max(channel(2203,3),max(channel(2203,3),channel(2203,3)))`n" +
                        "#2:dc1`n" +
                        "channel(2203,3)`n" +
                        "#3:dc1`n" +
                        "channel(2203,3)`n" +
                        "#4:dc1`n" +
                        "channel(2203,3)"

            $aggr = $s | New-SensorFactoryDefinition {"Max Value"} -Aggregator {"max($expr,$acc)"} 3
            $channels = $s | New-SensorFactoryDefinition {$_.Device} 3 -StartId 2

            ($aggr + $channels) -join "`n" | Should Be $expected
        }

        It "calculates an average by executing a finalizer" {
        
            $expected = "#1:Average`n" +
                        "(channel(2203,0) + channel(2203,0))/2"

            (
                $sensors | New-SensorFactoryDefinition {"Average"} -Aggregator {"$acc + $expr"} -Finalizer {"($acc)/$($sensors.Count)"} 0

            ) -join "`n" | Should Be $expected
        }

        It "creates an aggregation using a string name" {

            $expected = "#1:Average`n" +
                        "(channel(2203,0) + channel(2203,0))/2"

            (
                $sensors | New-SensorFactoryDefinition "Average" -Aggregator {"$acc + $expr"} -Finalizer {"($acc)/$($sensors.Count)"} 0

            ) -join "`n" | Should Be $expected
        }
    }

    Context "Value" {

        It "specifies a static value with a string name" {

            $expected = "#1:Line at 40.2`n" +
                        "40.2"

            (
                New-SensorFactoryDefinition "Line at 40.2" -Value 40.2

            ) -join "`n" | Should Be $expected
        }

        It "specifies a static value with script block name" {

            $expected = "#1:Line at 40.2`n" +
                        "40.2"

            (
                New-SensorFactoryDefinition {"Line at 40.2"} -Value 40.2

            ) -join "`n" | Should Be $expected
        }

        It "specifies a static value and a start ID" {

            $expected = "#2:Line at 40.2 [msec]`n" +
                        "40.2"

            (
                New-SensorFactoryDefinition "Line at 40.2 [msec]" -Value 40.2 -StartId 2

            ) -join "`n" | Should Be $expected
        }
    }
}