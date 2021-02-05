$vsproduct = "2019"
$vsedition = "Community"
$buildtoolsversion = "15.0"

Write-Host "Current code page:"
chcp

#this SHOULD force english output from MSBuild.exe, but it tricky, for details check below issue:
#https://github.com/dotnet/msbuild/issues/1596
Write-Host "Overriding current code page..." -ForegroundColor Yellow
chcp 437

Write-Host "Configuration:"
Write-Host "Visual Studio: product: " -NoNewLine
Write-Host "$vsproduct" -ForegroundColor magenta
Write-Host "Visial Studio: edition: " -NoNewLine
Write-Host "$vsedition" -ForegroundColor magenta
Write-Host "Build tools version: " -NoNewLine
Write-Host "$buildtoolsversion" -ForegroundColor magenta

function Run-TestByCategory {
	[CmdletBinding()]
	param (
		[string]$product,
		[string]$edition,
		[string]$testCategory
    )
	
	Write-Host "-----------------------------------------------" -ForegroundColor Gray
	Write-Host "Running tests by category: " -NoNewLine
	Write-Host "$testCategory" -ForegroundColor magenta
	Write-Host "-----------------------------------------------" -ForegroundColor Gray

	#run script in its self scope
	& ".\\runTestByCategory.ps1" -vsproduct $vsproduct -vsedition $vsedition -category "Service"
	
	Write-Host "-----------------------------------------------" -ForegroundColor Gray
	Write-Host "Test end: " -NoNewLine
	Write-Host "$testCategory" -ForegroundColor magenta
	Write-Host "-----------------------------------------------" -ForegroundColor Gray
}

function Run-CompileTestProject {
	[CmdletBinding()]
	param (
		[string]$product,
		[string]$edition,
		[string]$buildtools
    )
	
	Write-Host "-----------------------------------------------" -ForegroundColor Gray
	Write-Host "Compiling tests..." -ForegroundColor magenta
	Write-Host "-----------------------------------------------" -ForegroundColor Gray
	
	& ".\\compileTests.ps1" -vsproduct $product -vsedition $edition -buildtoolsversion $buildtools | 
		Select-String 'Build succeeded|failed' -Context 0, 20 #filter output to provide minimal info about build state
	
	Write-Host "-----------------------------------------------" -ForegroundColor Gray
	Write-Host "Compiling part completed" -ForegroundColor magenta
	Write-Host "-----------------------------------------------" -ForegroundColor Gray
}

Write-Host "--------------------------------------------------------------------------------------------------------------------" -ForegroundColor Gray
Write-Host "MSBuild.exe might not provide build result as expected, check script comment and follow MSBuild issue for details" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------------------------------------------------------------" -ForegroundColor Gray

#compie tests
Run-CompileTestProject -product $vsproduct -edition $vsedition -buildtools $buildtoolsversion

#test services
Run-TestByCategory -product $vsproduct -edition $vsedition -testCategory "Service"

#todo: test view models
#Run-TestByCategory -product $vsproduct -edition $vsedition -testCategory "ViewModels"

#todo: test views and controls
#Run-TestByCategory -product $vsproduct -edition $vsedition -testCategory "UI"