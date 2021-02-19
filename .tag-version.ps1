Param(
    [Parameter( Mandatory = $false)]
    $dry = $false,
    [Parameter( Mandatory = $true)]
    $commit,
    [Parameter( Mandatory = $true)]
    $version,
    [Parameter( Mandatory = $true)]
    $tagPrefix
)

# $commit = Read-Host "Enter commit hash to version: "
# $version = Read-Host "Enter application version: "
$deployPackage = "AOEMatchDataProvider-$version.nupkg"

# check if correct package exist
if (!(Test-Path $deployPackage)) 
{
  Write-Warning "Package: $deployPackage does not exists"
  Exit
}

# check if AssemblyInfo version is matching realease
$assemblyInfo = Get-Content -path ".\Properties\AssemblyInfo.cs" -Raw
if(!($assemblyInfo.IndexOf("AssemblyVersion(`"$version", [System.StringComparison]::InvariantCultureIgnoreCase) -ge 0))
{
  Write-Warning "Assembly info isn't matching to provided version: $version"
  Exit
}

$tag = "$tagPrefix-$version"

if($dry -eq $true) 
{
  Write-Host "Dry run completed with success (tag: $tag, commit: $commit, package: $deployPackage)"
  Exit
}

Write-Host "creating tag: "
git tag -a $tag $commit

Write-Host "execute: `"git push origin $tag`" to push tag"

# git push origin $tag
