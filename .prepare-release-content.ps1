#copy release
Copy-Item -Path ".\bin\Release\*" -Destination ".\lib\net45" -Recurse

#clear debug files
del ".\lib\net45\*.pdb"
del ".\lib\net45\*.vshost"

Write-Host "Release copied, execute .patch-for-staging.ps1 to repatch!" 
