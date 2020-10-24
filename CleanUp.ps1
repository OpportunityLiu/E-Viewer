$folders = Get-ChildItem -Recurse -Depth 1;
foreach($item in $folders)
{
    if($item.Name -eq "obj" -or $item.Name -eq "bin")
    {
        if($item.PSIsContainer)
        {
            $item.Delete($true);
        }
        else
        {
            $item.Delete();
        }
    }
}