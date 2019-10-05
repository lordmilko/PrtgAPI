﻿. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function CreateTestTree
{
    SetMultiTypeResponse | Out-Null

    ProbeNode -Id 1000 {
        DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        DeviceNode -Id 3001 {
            SensorNode -Id 4001
        }
    }
}

Describe "NodeExtension" -Tag @("PowerShell", "UnitTest") {

    $probe = CreateTestTree

    Context "FindNodes" {
        It "finds with FindNode" {

            $results = $probe.FindNode({$_.Type -eq "Device" -and $_.Value.Id -eq 3000})
            $results.Count | Should Be 1
            $results.Name | Should Be "Probe Device0"
        }

        It "finds with FindNodes" {

            $results = $probe.FindNodes({$_.Type -eq "Sensor"})
            $results.Count | Should Be 2
            $results[0].Name | Should Be "Volume IO _Total0"
            $results[1].Name | Should Be "Volume IO _Total1"
        }

        It "doesn't specify a ScriptBlock" {
            
            $results = $probe.FindNodes()
            $results.Count | Should Be 4
        }

        It "specifies a `$null ScriptBlock" {

            $results = $probe.FindNodes($null)
            $results.Count | Should Be 4
        }

        It "specifies a ScriptBlock that retuens `$null" {
            $result = $probe.FindNodes({$null})
            $result | Should Be $null
        }

        It "specifies a ScriptBlock that returns a non-bool type" {
            $result = $probe.FindNodes({"a"})
            $result | Should Be $null
        }

        It "throws specifying a ScriptBlock of an invalid type" {
            { $probe.FindNodes("a") } | Should Throw "Cannot convert the `"a`" value of type `"System.String`" to type `"System.Management.Automation.ScriptBlock`""
        }
    }

    Context "InsertNodesAfter" {
        It "inserts with InsertNodeAfter" {

            $node = $probe.FindNode({$_.Value.Id -eq 3000})
            $node | Should Not BeNullOrEmpty

            $newNode = DeviceNode -Id 3002

            $newProbe = $probe.InsertNodeAfter($node, $newNode)
            $newProbe.Type | Should Be Probe

            $newProbe.Children.Count | Should Be 3
            $newProbe.Children[0].Value.Id | Should Be 3000
            $newProbe.Children[1].Value.Id | Should Be 3002
            $newProbe.Children[2].Value.Id | Should Be 3001
        }

        It "inserts with InsertNodesAfter" {
            $node = $probe.FindNode({$_.Value.Id -eq 3000})
            $node | Should Not BeNullOrEmpty

            $newNodes = DeviceNode -Id 3002,3003

            $newProbe = $probe.InsertNodesAfter($node, $newNodes)

            $newProbe.Children.Count | Should Be 4
            $newProbe.Children[0].Value.Id | Should Be 3000
            $newProbe.Children[1].Value.Id | Should Be 3002
            $newProbe.Children[2].Value.Id | Should Be 3003
            $newProbe.Children[3].Value.Id | Should Be 3001
        }
    }

    Context "InsertNodesBefore" {
        It "inserts with InsertNodeBefore" {
            $node = $probe.FindNode({$_.Value.Id -eq 3000})
            $node | Should Not BeNullOrEmpty

            $newNode = DeviceNode -Id 3002

            $newProbe = $probe.InsertNodeBefore($node, $newNode)
            $newProbe.Type | Should Be Probe

            $newProbe.Children.Count | Should Be 3
            $newProbe.Children[0].Value.Id | Should Be 3002
            $newProbe.Children[1].Value.Id | Should Be 3000
            $newProbe.Children[2].Value.Id | Should Be 3001
        }

        It "inserts with InsertNodesBefore" {
            $node = $probe.FindNode({$_.Value.Id -eq 3000})
            $node | Should Not BeNullOrEmpty

            $newNodes = DeviceNode -Id 3002,3003

            $newProbe = $probe.InsertNodesBefore($node, $newNodes)

            $newProbe.Children.Count | Should Be 4
            $newProbe.Children[0].Value.Id | Should Be 3002
            $newProbe.Children[1].Value.Id | Should Be 3003
            $newProbe.Children[2].Value.Id | Should Be 3000
            $newProbe.Children[3].Value.Id | Should Be 3001
        }
    }

    Context "RemoveNodes" {
        It "removes with RemoveNode" {
            $node = $probe.FindNode({$_.Value.Id -eq 3000})
            $node | Should Not BeNullOrEmpty

            $newProbe = $probe.RemoveNode($node)

            $newProbe.Children.Count | Should Be 1
            $newProbe.Children[0].Value.Id | Should Be 3001
        }

        It "removes with RemoveNodes" {
            $sensors = $probe.FindNodes({$_.Type -eq "Sensor"})
            $sensors.Count | Should Be 2

            $newProbe = $probe.RemoveNodes($sensors)
            $newProbe.Children.Count | Should Be 2

            $newProbe["Probe Device0"].Children.Count | Should Be 0
            $newProbe["Probe Device1"].Children.Count | Should Be 0
        }
    }

    Context "ReplaceNodes" {
        It "replaces with ReplaceNode" {
            $node = $probe.FindNode({$_.Value.Id -eq 3000})
            $node | Should Not BeNullOrEmpty

            $replacement = DeviceNode -Id 3002

            $newProbe = $probe.ReplaceNode($node, $replacement)

            $newProbe.Children.Count | Should Be 2
            $newProbe.Children[0].Value.Id | Should Be 3002
            $newProbe.Children[1].Value.Id | Should Be 3001
        }

        It "replaces with ReplaceNodes" {
            $node = $probe.FindNode({$_.Value.Id -eq 3000})
            $node | Should Not BeNullOrEmpty

            $replacements = DeviceNode -Id 3002,3003

            $newProbe = $probe.ReplaceNodes($node, $replacements)

            $newProbe.Children.Count | Should Be 3
            $newProbe.Children[0].Value.Id | Should Be 3002
            $newProbe.Children[1].Value.Id | Should Be 3003
            $newProbe.Children[2].Value.Id | Should Be 3001
        }
    }

    Context "WithChildren" {
        It "replaces children" {
            $replacements = DeviceNode -Id 3002,3003

            $newProbe = $probe.WithChildren($replacements)

            $newProbe.Children.Count | Should Be 2

            $newProbe.Children[0].Value.Id | Should Be 3002
            $newProbe.Children[1].Value.Id | Should Be 3003
        }
    }
}
