$NugetPackage = nuget locals global-packages -list
$NugetPackage = $NugetPackage.SubString(17)
$ToolPath = Join-Path $NugetPackage 'Opportunity.ResourceGenerator\1.2.3\tools\Opportunity.ResourceGenerator.Generator.exe'
&$ToolPath $Env:APPCENTER_UWP_SOLUTION