$NugetPackage = nuget locals global-packages -list
$NugetPackage = $NuetPackage.SubString(17)
$ToolPath = Join-Path $NugetPackage 'opportunity.resourcegenerator\1.2.3\tools\Opportunity.ResourceGenerator.Generator.exe'
&$ToolPath $Env:APPCENTER_UWP_SOLUTION