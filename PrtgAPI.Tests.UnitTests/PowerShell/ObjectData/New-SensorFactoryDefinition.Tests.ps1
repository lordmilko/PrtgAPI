. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-SensorFactoryDefinition" -Tag @("PowerShell", "UnitTest") {

    $sensors = Run "Sensor" {

        $item1 = GetItem
        $item1.ObjId = 1001
        $item2 = GetItem
        $item2.ObjId = 1002

        WithItems($item1, $item2) {
            Get-Sensor
        }
    }

    $aggregateModeCases = @(
        @{ Name = "Sum";     Expected = @("#1:Aggregated"
                                          "channel(1001,0) + channel(1002,0)") -join "`n"
        }
        @{ Name = "Max";     Expected = @("#1:Aggregated"
                                          "max(channel(1001,0), channel(1002,0))") -join "`n"
        }
        @{ Name = "Min";     Expected = @("#1:Aggregated"
                                          "min(channel(1001,0), channel(1002,0))") -join "`n"
        }
        @{ Name = "Average"; Expected = @("#1:Aggregated"
                                          "(channel(1001,0) + channel(1002,0)) / 2") -join "`n"
        }
    )

    $summaryModeCases = @(
        @{ Name = "Sum";     Expected = @("#1:Summary"
                                          "channel(1001,0) + channel(1002,0)"
                                          "#2:dc1"
                                          "channel(1001,0)"
                                          "#3:dc1"
                                          "channel(1002,0)") -join "`n"
        }
        @{ Name = "Max";     Expected = @("#1:Summary"
                                          "max(channel(1001,0), channel(1002,0))"
                                          "#2:dc1"
                                          "channel(1001,0)"
                                          "#3:dc1"
                                          "channel(1002,0)") -join "`n"
        }
        @{ Name = "Min";     Expected = @("#1:Summary"
                                          "min(channel(1001,0), channel(1002,0))"
                                          "#2:dc1"
                                          "channel(1001,0)"
                                          "#3:dc1"
                                          "channel(1002,0)") -join "`n"
        }
        @{ Name = "Average"; Expected = @("#1:Summary"
                                          "(channel(1001,0) + channel(1002,0)) / 2"
                                          "#2:dc1"
                                          "channel(1001,0)"
                                          "#3:dc1"
                                          "channel(1002,0)") -join "`n"
        }
    )

    It "uses a name and ID" {

        $expected = "#1:dc1`n" +
                    "channel(1001,1)`n" +
                    "#2:dc1`n" +
                    "channel(1002,1)"

        (
            $sensors | New-SensorFactoryDefinition {$_.Device} 1

        ) -Join "`n" | Should Be $expected
    }

    It "uses just a name" {

        $expected = "#1:dc1`n" +
                    "channel(1001,0)`n" +
                    "#2:dc1`n" +
                    "channel(1002,0)"

        (
            $sensors | New-SensorFactoryDefinition {$_.Device}

        ) -Join "`n" | Should Be $expected
    }

    It "creates a sensor factory with a custom unit" {

        $expected = "#1:dc1 [bananas]`n" +
                    "channel(1001,0)`n" +
                    "#2:dc1 [bananas]`n" +
                    "channel(1002,0)"

        (
            $sensors | New-SensorFactoryDefinition { "$($_.Device) [bananas]" } 0

        ) -join "`n" | Should Be $expected
    }

    It "specifies a custom start index" {

        $expected = "#2:dc1`n" +
                    "channel(1001,2)`n" +
                    "#3:dc1`n" +
                    "channel(1002,2)"

        (
            $sensors | New-SensorFactoryDefinition { $_.Device } 2 -StartId 2

        ) -join "`n" | Should Be $expected
    }

    It "specifies a name expression that returns null" {
        { $sensors | fdef { $null } } | Should Throw "'' is not a valid channel name"
    }

    It "specifies an empty name expression" {
        { $sensors | fdef { } } | Should Throw "'' is not a valid channel name"
    }

    It "specifies an empty name" {
        { $sensors | fdef "" } | Should Throw "'' is not a valid channel name"
    }

    It "specifies a whitespace name" {
        { $sensors | fdef " " } | Should Throw "' ' is not a valid channel name"
    }

    It "specifies a newline name" {
        { $sensors | fdef "`n" } | Should Throw "'`n' is not a valid channel name"
    }

    It "specifies a name expression that returns empty" {
        { $sensors | fdef { "" } } | Should Throw "'' is not a valid channel name"
    }

    It "specifies a name expression that returns whitespace" {
        { $sensors | fdef { " " } } | Should Throw "' ' is not a valid channel name"
    }

    It "specifies a name expression that returns newline" {
        { $sensors | fdef { "`n" } } | Should Throw "'`n' is not a valid channel name"
    }

    It "specifies a name using an alias" {

        $expected = "#1:dc1`n" +
                    "channel(1001,0)`n" +
                    "#2:dc1`n" +
                    "channel(1002,0)"

        (
            $sensors | New-SensorFactoryDefinition -ChannelName {$_.Device}

        ) -Join "`n" | Should Be $expected
    }

    Context "Expression" {

        It "uses a name, expression (with the default expression) and ID" {

            $expected = "#1:dc1`n" +
                        "100 - channel(1001,2)`n" +
                        "#2:dc1`n" +
                        "100 - channel(1002,2)"

            (
                $sensors | New-SensorFactoryDefinition {$_.Device} -Expr {"100 - $expr"} 2

            ) -join "`n" | Should Be $expected
        }

        It "uses a name, expression (with the current item) and ID" {

            $expected = "#1:dc1`n" +
                        "channel(1001,100)`n" +
                        "#2:dc1`n" +
                        "channel(1002,100)"
        
            (
                $sensors | New-SensorFactoryDefinition {$_.Device} -Expr {"channel($($_.Id),100)"} 1

            ) -join "`n" | Should Be $expected
        }

        It "uses a string name with an expression" {

            $expected = "#1:test`n" +
                        "channel(1001,100)`n" +
                        "#2:test`n" +
                        "channel(1002,100)"

            (
                $sensors | New-SensorFactoryDefinition "test" -Expr { "channel($($_.Id),100)" } 1

            ) -join "`n" | Should Be $expected
        }

        It "specifies an expression that returns null" {
            { $sensors | fdef { $_.Device } -Expression { $null } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies an empty expression" {
            { $sensors | fdef { $_.Device } -Expression { } } | Should Throw "'' is not a valid channel expression"
        }
    }

    Context "Aggregation" {

        It "aggregates (with the default expression) with a name and ID" {

            $expected = "#1:Sum`n" +
                        "channel(1001,4) + channel(1002,4)"
        
            (
                $sensors | New-SensorFactoryDefinition {"Sum"} -Aggregator {"$acc + $expr"} 4

            ) -join "`n" | Should Be $expected
        }

        It "aggregates (with the current item) with a name, expression and ID" {

            $expected = "#1:Sum`n" +
                        "100 - channel(1001,5) + channel(1002,20)"
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
                        "(channel(1001,0) + channel(1002,0))/2"

            (
                $sensors | New-SensorFactoryDefinition {"Average"} -Aggregator {"$acc + $expr"} -Finalizer {"($acc)/$($sensors.Count)"} 0

            ) -join "`n" | Should Be $expected
        }

        It "creates an aggregation using a string name" {

            $expected = "#1:Average`n" +
                        "(channel(1001,0) + channel(1002,0))/2"

            (
                $sensors | New-SensorFactoryDefinition "Average" -Aggregator {"$acc + $expr"} -Finalizer {"($acc)/$($sensors.Count)"} 0

            ) -join "`n" | Should Be $expected
        }

        It "specifies an aggregation that returns null" {
            { $sensors | fdef "Average" -Aggregator { $null } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies an empty aggregator" {
            { $sensors | fdef "Average" -Aggregator { } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies a finalizer that returns null" {
            { $sensors | fdef "Average" -Aggregator { "$acc + $expr" } -Finalizer { $null } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies an empty finalizer" {
            { $sensors | fdef "Average" -Aggregator { "$acc + $expr" } -Finalizer { } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies a <name> expression" -TestCases $aggregateModeCases {
            param($name, $expected)

            ( $sensors | fdef "Aggregated" -Aggregator $name ) -join "`n" | Should Be $expected
        }

        It "throws specifying a finalizer when a known summary mode has been specified" {
            { $sensors | fdef "Aggregated" -Aggregator Sum -Finalizer { "$acc / $($sensors.Count)" } } | Should Throw "Cannot specify -Finalizer when -Aggregator is not a ScriptBlock."
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

        It "specifies a null value" {
            { fdef "Line at 40.2 [msec]" -Value $null } | Should Throw "Cannot bind argument to parameter 'Value' because it is null"
        }

        It "specifies an empty value" {
            { fdef "Line at 40.2 [msec]" -Value "" } | Should Throw "Cannot bind argument to parameter 'Value' because it is an empty string"
        }

        It "specifies a whitespace value" {
            { fdef "Line at 40.2 [msec]" -Value " " } | Should Throw "' ' is not a valid channel expression"
        }
    }

    Context "Summary" {

        It "specifies a <name> expression" -TestCases $summaryModeCases {
            param($name, $expected)
            
            ( $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression $name ) -join "`n" | Should Be $expected
        }

        It "utilizes aliases" {
            $expected = "#1:Summary`n" +
                        "(channel(1001,0) + channel(1002,0))/2`n" +
                        "#2:dc1`n" +
                        "channel(1001,0)`n" +
                        "#3:dc1`n" +
                        "channel(1002,0)"

            (
                $sensors | fdef { $_.Device } -sn "Summary" -se { "$acc + $expr" } -sf { "($acc)/$($sensors.Count)" }
            ) -join "`n" | Should Be $expected
        }

        It "specifies a custom summary expression" {
            $expected = "#1:Summary`n" +
                        "channel(1001,0) + channel(1002,0)`n" +
                        "#2:dc1`n" +
                        "channel(1001,0)`n" +
                        "#3:dc1`n" +
                        "channel(1002,0)"

            (
                $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression { "$acc + $expr" }
            ) -join "`n" | Should Be $expected
        }

        It "specifies a custom summary expression with a finalizer" {
            $expected = "#1:Summary`n" +
                        "(channel(1001,0) + channel(1002,0))/2`n" +
                        "#2:dc1`n" +
                        "channel(1001,0)`n" +
                        "#3:dc1`n" +
                        "channel(1002,0)"

            (
                $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression { "$acc + $expr" } -SummaryFinalizer { "($acc)/$($sensors.Count)" }
            ) -join "`n" | Should Be $expected
        }

        It "throws specifying a finalizer when a known summary mode has been specified" {
            { $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression Sum -SummaryFinalizer { "$acc / $($sensors.Count)" } } | Should Throw "Cannot specify -SummaryFinalizer when -SummaryExpression is not a ScriptBlock."
        }

        It "specifies a summary expression that returns null" {
            { $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression { $null } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies an empty summary expression" {
            { $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression { } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies a summary finalizer that returns null" {
            { $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression { "$acc + $expr" } -SummaryFinalizer { $null } } | Should Throw "'' is not a valid channel expression"
        }

        It "specifies an empty summary finalizer" {
            { $sensors | fdef { $_.Device } -SummaryName "Summary" -SummaryExpression { "$acc + $expr" } -SummaryFinalizer { } } | Should Throw "'' is not a valid channel expression"
        }
    }
}