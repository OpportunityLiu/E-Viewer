$folders = Get-ChildItem -Directory -Recurse -Depth 1;
foreach($item in $folders)
{
    if($item.Name -eq "obj" -or $item.Name -eq "bin")
    {
        $item.Delete($true);
    }
}