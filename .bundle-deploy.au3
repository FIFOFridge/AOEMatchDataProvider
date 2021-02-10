#include <File.au3>
#include <GUIConstantsEx.au3>
#include <WindowsConstants.au3>

#pragma compile(Out, BundledInstaller.exe)
#pragma compile(Icon, .\.assets\appIcon.ico)
#pragma compile(UPX, False)
#pragma compile(FileDescription, Single file installer for AoE Match Data Provider)
#pragma compile(ProductName, AoEMatchDataProvider Installer)

local $hPopup
local $sTempFolder = _TempFile(@TempDir, "~", "")
;local $sLicense = 'Copyright 2020-2021 FIFOFridge'& @CRLF &'Licensed under the Apache License, Version 2.0 (the "License")'& @CRLF &'you may not use this file except in compliance with the License.'& @CRLF &'You may obtain a copy of the License at'& @CRLF &''& @CRLF &'    http://www.apache.org/licenses/LICENSE-2.0 '& @CRLF &''& @CRLF &'Unless required by applicable law or agreed to in writing, software '& @CRLF &'distributed under the License is distributed on an "AS IS" BASIS, '& @CRLF &'WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. '& @CRLF &'See the License for the specific language governing permissions and '& @CRLF &'limitations under the License.'

$hPopup = GUICreate("Installing AoE Match Data Provider (Part 1 of 2)", 400, 100, -1, -1, $WS_POPUP, $WS_EX_DLGMODALFRAME)

local $hLabel = GUICtrlCreateLabel("Unpacking files...", 150, 40)
GUISetState(@SW_SHOW, $hPopup)

DirCreate($sTempFolder)

;pack into single file
Fileinstall(".\.deploy\setup.exe", $sTempFolder & "\setup.exe")
Fileinstall(".\.deploy\AOEMatchDataProvider.application", $sTempFolder & "\AOEMatchDataProvider.application")

#include ".bundle-file-list.au3"

Run($sTempFolder & "\setup.exe")