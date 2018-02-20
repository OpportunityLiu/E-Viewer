$Files = Get-ChildItem -Path $Env:APPCENTER_OUTPUT_DIRECTORY -Include @('*.cer', '*.appxsym', '*.appxbundle') -Recurse
$Version = $Files[0].Name.Split('_')[1]
$Auth = [convert]::ToBase64String([system.text.encoding]::UTF8.GetBytes("${ENV:GITHUB_USER}:${ENV:GITHUB_PASS}"))
$DefaultHeader = @{
    Authorization = "Basic $Auth"
}
$CreateRelease = Invoke-WebRequest "https://api.github.com/repos/${ENV:GITHUB_USER}/ExViewer/releases" -Method Post -Headers $DefaultHeader -Body @"
{
  "tag_name": "v$Version",
  "target_commitish": "${Env:APPCENTER_BRANCH}",
  "name": "v$Version",
  "body": "",
  "draft": true,
  "prerelease": false
}
"@
$CreateReleaseData = ConvertFrom-Json $CreateRelease.Content
$DefaultHeader['Content-Type'] = 'application/octet-stream'
$Files | %{
    $UpUri = $CreateReleaseData.upload_url.Replace('{?name,label}', "?name=$($_.Name)")
    Echo $UpUri
    Invoke-WebRequest $UpUri -Method Post -Headers $DefaultHeader -InFile $_ -ErrorAction Continue
}