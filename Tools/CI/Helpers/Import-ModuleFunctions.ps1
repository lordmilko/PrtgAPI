function Import-ModuleFunctions($folder)
{
    $functions = Get-ChildItem $folder

    foreach($function in $functions)
    {
        . $function.FullName

        Export-ModuleMember $function.Name.Substring(0, $function.Name.Length - $function.Extension.Length)
    }
}