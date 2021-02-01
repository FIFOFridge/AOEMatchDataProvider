Param(
    [Parameter( Mandatory = $true)]
    $vsproduct,
    [Parameter( Mandatory = $true)]
    $vsedition,
	[Parameter( Mandatory = $true)]
    $buildtoolsversion
)
$command = "C:\Program Files (x86)\Microsoft Visual Studio\$vsproduct\$vsedition\MSBuild\$buildtoolsversion\Bin\MSBuild.exe"
$arguments = @('./AOEMatchDataProviderTests.csproj', '/p:Configuration=Debug')
& $command $arguments
