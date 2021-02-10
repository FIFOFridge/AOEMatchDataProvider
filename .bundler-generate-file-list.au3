#include <File.au3>

const $sVersion = "0_1_0_0"
const $sDeployTargetVariable = "$sTempFolder"
const $sOutput = ".bundle-file-list.au3"

;Fileinstall(".\.deploy\Application Files\AOEMatchDataProvider_0_1_0_0\", )

$sVersioned = ".\.deploy\Application Files\AOEMatchDataProvider_" & $sVersion & "\"
$sVersionedTarget = StringReplace($sVersioned, ".\.deploy\", "")

;grab files
local $asFiles = _FileListToArray ($sVersioned , "*", $FLTA_FILESFOLDERS)

if FileExists($sOutput) then FileDelete($sOutput)

AppendCode('DirCreate($sTempFolder & "\Application Files")')
AppendCode('DirCreate($sTempFolder & "\' & $sVersionedTarget & '")')

;produce output
for $i = 1 to $asFiles[0]
   AppendFile($asFiles[$i])
next

Func AppendCode($_code)
   FileWriteLine($sOutput, $_code)
EndFunc

Func AppendFile($_path)
   local $sFullRelativePath = $sVersioned & $_path

   WriteLine("Including file: " & $sFullRelativePath & " ...")
   ;generate bundler code
   FileWriteLine($sOutput, 'Fileinstall("' & $sFullRelativePath &  '",' & $sDeployTargetVariable & ' & "\' & $sVersionedTarget & $_path & '")')
EndFunc

Func WriteLine($_content)
   ConsoleWrite($_content & @CRLF)
EndFunc

