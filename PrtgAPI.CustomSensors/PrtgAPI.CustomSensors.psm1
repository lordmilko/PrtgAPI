function __resultProperty($value)
{
    $name = (Get-PSCallStack)[1].Command

    return "`t`t<$name>$value</$name>`n"
}

function Prtg($scriptBlock)
{
    if(!$scriptBlock)
    {
        throw "Prtg block requires an inner element"
    }

    return "<Prtg>`n$(& $scriptBlock)</Prtg>"
}

function Result($scriptBlock)
{
    if(!$scriptBlock)
    {
        throw "Result block requires an inner element"
    }

    return "`t<Result>`n$(& $scriptBlock)`t</Result>`n"
}

function Text($value)
{
    return "`t<Text>$value</Text>`n"
}

function Error($value)
{
    return "`t<Error>$value</Error>`n"
}

function Channel($value)          { __resultProperty $value }
function Value($value)            { __resultProperty $value }
function Unit($value)             { __resultProperty $value }
function CustomUnit($value)       { __resultProperty $value }
function SpeedSize($value)        { __resultProperty $value }
function VolumeSize($value)       { __resultProperty $value }
function SpeedSize ($value)       { __resultProperty $value }
function SpeedTime ($value)       { __resultProperty $value }
function Mode ($value)            { __resultProperty $value }
function Float ($value)           { __resultProperty $value }
function DecimalMode ($value)     { __resultProperty $value }
function Warning ($value)         { __resultProperty $value }
function ShowChart ($value)       { __resultProperty $value }
function ShowTable ($value)       { __resultProperty $value }
function LimitMaxError ($value)   { __resultProperty $value }
function LimitMaxWarning ($value) { __resultProperty $value }
function LimitMinWarning ($value) { __resultProperty $value }
function LimitMinError ($value)   { __resultProperty $value }
function LimitErrorMsg ($value)   { __resultProperty $value }
function LimitWarningMsg ($value) { __resultProperty $value }
function LimitMode ($value)       { __resultProperty $value }
function ValueLookup ($value)     { __resultProperty $value }
function NotifyChanged ($value)   { __resultProperty $value }