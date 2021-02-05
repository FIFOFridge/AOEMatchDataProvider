Param(
    [Parameter( Mandatory = $true)]
    $vsproduct,
    [Parameter( Mandatory = $true)]
    $vsedition,
	[Parameter( Mandatory = $true)]
    $buildtoolsversion
)
#this one stands for Visual Studio 2017
#$command = "C:\Program Files (x86)\Microsoft Visual Studio\$vsproduct\$vsedition\MSBuild\$buildtoolsversion\Bin\MSBuild.exe"
$command = "C:\Program Files (x86)\Microsoft Visual Studio\$vsproduct\$vsedition\MSBuild\Current\Bin\MSBuild.exe"
$arguments = @('./AOEMatchDataProviderTests.csproj', '/p:Configuration=Debug')
& $command $arguments
