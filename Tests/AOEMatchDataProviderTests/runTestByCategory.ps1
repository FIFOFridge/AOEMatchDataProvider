Param(
    [Parameter( Mandatory = $true)]
    $vsproduct,
    [Parameter( Mandatory = $true)]
    $vsedition,
    [Parameter( Mandatory = $true)]
    $category
)
$command = "C:\Program Files (x86)\Microsoft Visual Studio\$vsproduct\$vsedition\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
$arguments = @('./bin/debug/AOEMatchDataProviderTests.dll')
& $command $arguments "/TestCaseFilter:`"TestCategory=$category`"" "/Enablecodecoverage"
