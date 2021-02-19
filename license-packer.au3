;pack file formated license (from file) to single line format that can be easly embedded in source files
local $in = FileRead("unprocessed-license.txt")
local $out = StringReplace($in, @CRLF, '\n')
$out = StringReplace($out, '"', '\"')

ConsoleWrite(@CRLF)
ConsoleWrite("------------------------------------------------------------------------------------------" & @CRLF)
ConsoleWrite($out & @CRLF)
ConsoleWrite("------------------------------------------------------------------------------------------" & @CRLF)
ConsoleWrite(@CRLF)

