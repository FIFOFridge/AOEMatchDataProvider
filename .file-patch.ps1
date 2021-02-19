Param(
    [Parameter( Mandatory = $true)]
    $filePath,
    [Parameter( Mandatory = $true)]
    $toReplace,
    [Parameter( Mandatory = $true)]
    $replaceWith
)

$backup = "$filePath.patch-bak"

Copy-Item $filePath -Destination $backup

if (!(Test-Path $backup)) 
{
  Write-Warning "backup file not found $backup"
  Exit
}

#((Get-Content -path $filePath -Raw) -replace $toReplace, $replaceWith) | Write-Host
((Get-Content -path $filePath -Raw) -replace $toReplace, $replaceWith) | Set-Content -NoNewLine -Path $filePath

#git stash push --no-keep-index -m "File patch: $toReplace -> $replaceWith" $filePath
