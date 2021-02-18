$commit = Read-Host "Enter commit hash to version: "
$version = Read-Host "Enter application version: "
$deployPackage = "AOEMatchDataProvider-$version.nupkg"

if (!(Test-Path $deployPackage)) 
{
  Write-Warning "Package: $deployPackage does not exists"
  Exit
}

git tag -a $version $commit
git push origin $version
