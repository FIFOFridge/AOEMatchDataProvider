Param(
  [Parameter( Mandatory = $true)]
  $filePath,
  [Parameter( Mandatory = $true)]
  $substringToTest
)

if (!(Test-Path $filePath)) 
{
  Write-Warning "unable to comple file test target file not found: $filePath"
  Exit
}

$content = (Get-Content -path $filePath -Raw)
if(!($content.IndexOf($substringToTest, [System.StringComparison]::InvariantCultureIgnoreCase) -ge 0))
{
  Write-Warning "file don't include substring: $substringToTest"
  Exit
}


