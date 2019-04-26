#region Initialization

. $PSScriptRoot\Init.ps1

function Describe($name, $script) {

    Pester\Describe $name {
        BeforeAll { Startup $name.Substring($name.indexof("-") + 1) }
        AfterAll { Shutdown }

        & $script
    }
}

function GetItem
{
    if($global:tester -eq $null)
    {
        throw "Cannot get item as `$global:tester is `$null"
    }

    return $global:tester.GetItem()
}

function WithItems($items, $assert)
{
    $oldClient = Get-PrtgClient

    $global:tester.SetPrtgSessionState($items)

    try
    {
        $result = & $assert

        if($result)
        {
            return $result
        }
    }
    finally
    {
        $global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)
    }
}

function Run($objectType, $script)
{
    $oldClient = Get-PrtgClient
    $oldTester = $global:tester
    $global:tester = (New-Object PrtgAPI.Tests.UnitTests.ObjectData.$($objectType)Tests)
    $global:tester.SetPrtgSessionState()

    try
    {
        $result = & $script
    }
    finally
    {
        $global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)
        $global:tester = $oldTester
    }    

    return $result
}

function RunCustomCount($hashtable, $action)
{
    $dictionary = GetCustomCountDictionary $hashtable

    $oldClient = Get-PrtgClient

    $newClient = [PrtgAPI.Tests.UnitTests.BaseTest]::Initialize_Client((New-Object PrtgAPI.Tests.UnitTests.Support.TestResponses.MultiTypeResponse -ArgumentList $dictionary))

    try
    {
        SetPrtgClient $newClient

        & $action
    }
    catch
    {
        throw
    }
    finally
    {
        SetPrtgClient $oldClient
    }
}

function WithStrict($script)
{
    try
    {
        Set-StrictMode -Version 3

        & $script
    }
    finally
    {
        Set-StrictMode -Off
    }
}

function GetCustomCountDictionary($hashtable)
{
    $dictionary = New-Object "System.Collections.Generic.Dictionary[[PrtgAPI.Content],[int]]"

    foreach($entry in $hashtable.GetEnumerator())
    {
        $newKey = $entry.Key -as "PrtgAPI.Content"

        if($newKey -eq $null)
        {
            throw "$($entry.Key) is not a valid PrtgAPI.Content value"
        }

        $dictionary.Add($newKey, $entry.Value)
    }

    return $dictionary
}

#region Custom Responses

function WithResponse($responseName, $scriptBlock) {

    $client = Get-PrtgClient

    try
    {
        SetResponseAndClient $responseName | Out-Null

        & $scriptBlock
    }
    finally
    {
        SetPrtgClient $client
    }
}

function WithResponseArgs($responseName, $arguments, $scriptBlock) {

    $client = Get-PrtgClient

    try
    {
        SetResponseAndClientWithArguments $responseName $arguments | Out-Null

        & $scriptBlock
    }
    finally
    {
        SetPrtgClient $client
    }
}

function WithReadOnly($scriptBlock)
{
    $multiTypeResponse = New-Object PrtgAPI.Tests.UnitTests.Support.TestResponses.MultiTypeResponse

    WithResponseArgs "ReadOnlyResponse" $multiTypeResponse $scriptBlock
}

function SetMultiTypeResponse
{
    SetResponseAndClient "MultiTypeResponse"
}

function SetActionResponse
{
    SetResponseAndClientWithArguments "BasicResponse" ""
}

function SetAddressValidatorResponse($strArr, $exactMatch = $false)
{
    $arr = $null

    if($strArr -is [System.Array])
    {
        $exactMatch  = $true

        $arr = $strArr
    }
    else
    {
        if($exactMatch)
        {
            $arr = BuildStr $strArr
        }
        else
        {
            $arr = $strArr
        }
    }
    
    SetResponseAndClientWithArguments "AddressValidatorResponse" @($arr, $exactMatch)
}

function SetResponseAndClient($responseName)
{
    $response = New-Object PrtgAPI.Tests.UnitTests.Support.TestResponses.$responseName

    SetResponseAndClientInternal $response
}

function SetResponseAndClientWithArguments($responseName, $arguments)
{
    $response = New-Object PrtgAPI.Tests.UnitTests.Support.TestResponses.$responseName -ArgumentList $arguments

    SetResponseAndClientInternal $response
}

function SetResponseAndClientInternal($response)
{
    $client = [PrtgAPI.Tests.UnitTests.BaseTest]::Initialize_Client($response)

    SetPrtgClient $client

    $response
}

#endregion

function BuildStr($str)
{
    $str = "https://prtg.example.com/" + $str + "username=username&passhash=12345678"

    return $str
}

function SetPrtgClient($client)
{
    $type = (gcm Connect-PrtgServer).ImplementingType.Assembly.GetType("PrtgAPI.PowerShell.PrtgSessionState")
    $property = $type.GetProperty("Client", [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::NonPublic)

    $property.SetValue($null, $client)
}

function SetVersion($versionStr)
{
    $client = Get-PrtgClient

    $flags = [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance

    $field = $client.GetType().GetField("version", $flags)

    $version = [Version]$versionStr

    $field.SetValue($client, $version)
}