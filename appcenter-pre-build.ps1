$NugetPackage = nuget locals global-packages -list
$NugetPackage = $NugetPackage.SubString(17)
$ToolVersions = (Get-ChildItem (Join-Path $NugetPackage 'Opportunity.ResourceGenerator') | Sort-Object Name -Descending)[0]
$ToolPath = Join-Path $ToolVersions.FullName '/tools/Opportunity.ResourceGenerator.Generator.exe'
&$ToolPath $Env:APPCENTER_UWP_SOLUTION