Dim patternString, patterns, patparts, rxp, inp
patternString = WScript.Arguments(0)
'WScript.Echo pat

patterns = Split(patternString,";")
Do While Not WScript.StdIn.AtEndOfStream
    inp = WScript.StdIn.ReadLine()
    For each pattern in patterns    
        If pattern <> "" Then
            patparts = Split(pattern,"/")
            'WScript.Echo patparts(1)
            'WScript.Echo patparts(2)    

            Set rxp = new RegExp
            rxp.Global = True
            rxp.Multiline = False
            patparts(1)=ConvertEscape(patparts(1))
            rxp.Pattern = patparts(1)
            inp = rxp.Replace(inp, patparts(2))
        End If
    Next
    WScript.Echo inp
Loop

Function ConvertEscape(strString)
    Dim RegEx
    Set RegEx = New RegExp              ' Create regular expression.
    RegEx.IgnoreCase = True             ' Make case insensitive.
    RegEx.Global=True                   'Search the entire String

    RegEx.Pattern="\$"
    strString = regEx.Replace(strString, "\$") ' Make replacement.

    ConvertEscape = strString
End Function
