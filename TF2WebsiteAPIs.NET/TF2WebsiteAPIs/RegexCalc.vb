Imports System.Text.RegularExpressions

' The good old Regex Calculator
Public Class RegexCalc

    Public Shared Answer = 0

    ' Inserts multiplication signs into functions so that the regex function can parse them (e.g. "2(x+3)" --> "2*(x+3)")
    Public Shared Function InsertMultiplySymbols(ByVal ExprStrMaster) As String

        Dim MultInsertRegex As New Regex("(\d(\(|[a-z])|\)\()")
        Dim MultInsertMatch As Match = MultInsertRegex.Match(ExprStrMaster)
        While MultInsertMatch.Success
            ExprStrMaster = ExprStrMaster.Insert(MultInsertMatch.Index + 1, "*")
            MultInsertMatch = MultInsertRegex.Match(ExprStrMaster, MultInsertMatch.Index) ' Start at previous match index to speed things up
        End While

        Return ExprStrMaster

    End Function

    ' Replaces named constants with their numerical values
    Public Shared Function ReplaceConstants(ByVal ExprStrMaster As String) As String

        '   Euler's constant (e)
        Dim ConstantRegex As New Regex("(\W|\s)e(\W|\s)")
        Dim ConstantMatch As Match = ConstantRegex.Match(ExprStrMaster)
        Dim ConstantStr As String = Math.E.ToString
        While ConstantMatch.Success
            ExprStrMaster = ExprStrMaster.Remove(ConstantMatch.Index + 1, 1).Insert(ConstantMatch.Index + 1, ConstantStr)
            ConstantMatch = ConstantRegex.Match(ExprStrMaster, ConstantMatch.Index + ConstantStr.Length) ' Start at previous match index to speed things up
        End While

        '   Pi
        ConstantRegex = New Regex("(\W|\s)pi(\W|\s)")
        ConstantMatch = ConstantRegex.Match(ExprStrMaster)
        ConstantStr = Math.PI.ToString
        While ConstantMatch.Success
            ExprStrMaster = ExprStrMaster.Remove(ConstantMatch.Index + 1, 2).Insert(ConstantMatch.Index + 1, ConstantStr)
            ConstantMatch = ConstantRegex.Match(ExprStrMaster, ConstantMatch.Index + ConstantStr.Length)  ' Start at previous match index to speed things up
        End While

        ' Return result
        Return ExprStrMaster

    End Function

    Public Shared Function RegexFunctionEvaluate(ByVal ExprStrMasterIn As String, Optional ByVal UseDegrees As Boolean = False, Optional ByVal NotPreformatted As Boolean = True)

        ' Check for improperly formatted strings
        If Regex.IsMatch(ExprStrMasterIn, "(\+|-|\\|/|\*){2,}") Then
            Return 0
        End If

        ' Precision of the calculations being performed
        Dim Precision As Integer = 5

        ' Skip null strings
        If ExprStrMasterIn.Length = 0 OrElse String.IsNullOrWhiteSpace(ExprStrMasterIn) Then
            Return 0
        End If

        ' Conversion factor between radians and degrees
        Dim RadDegConver As Double = 1
        If UseDegrees Then
            RadDegConver = Math.PI / 180
        End If

        ' Make sure the number of parenthesis is appropriate
        Dim StrA As String = ExprStrMasterIn.Replace("(", "").Replace("[", "")
        Dim StrB As String = ExprStrMasterIn.Replace(")", "").Replace("]", "")
        If StrA.Length <> StrB.Length Then
            MsgBox("Mismatched parenthesis/brackets")
            'Throw New Exception("FunctionEvaluate ERROR: The parentheses do not line up properly. Function: " & ExprStrMasterIn)
            Return 0
        End If

        ' Variables
        Dim ExprStrMaster As String = " " & Regex.Replace(ExprStrMasterIn, "\.{2,}", ".") ' The space in front allows constants to be properly replaced

        ' Preformatting = add multiplication symbols, then replace constants with their approximate numerical values
        If NotPreformatted Then

            ' Multiplication symbol insertion
            ExprStrMaster = InsertMultiplySymbols(ExprStrMaster)

            ' Constant replacement
            ExprStrMaster = ReplaceConstants(ExprStrMaster)

        End If

        ' Remove any commas
        If ExprStrMaster.Contains(",") Then
            ExprStrMaster = ExprStrMaster.LastIndexOf(",")
        End If

        ' Regex
        Dim NumberRegex As New Regex("(-|_|)(\d|:v:)+")
        Dim SubRegex As New Regex("\d+-(_|\d)+")
        Dim RegexParenConstant As New Regex(":y:(_|\d|:v:)+:z:")
        Dim RegexParen As New Regex("(:y:)(:A:|:S:|:M:|:D:|:E:|_|-|\d|:v:)*(:z:)")
        Dim RegexOperator As New Regex("(:A:|:S:|:M:|:D:|:E:|:y:|:z:|:w:|:x:)")

        ' If there is a constant that is enclosed by parenthesis, remove the parenthesis
        ExprStrMaster = ExprStrMaster.Replace("(", ":y:").Replace(")", ":z:").Replace(".", ":v:")

        ' ---------- Negative formatting ----------

        ' :S: a negative = :A:
        ExprStrMaster = ExprStrMaster.Replace("--", ":A:")

        ' For - (:S: and negative)
        Dim SubMatch As Match = SubRegex.Match(ExprStrMaster)

        ' --------- Mass formatting ---------
        ' Note: perhaps make a multiple parameter replacement system that does one pass and replaces everything
        ' HINT: Conduct all possible preformat functions before sending function (in a batch manner)
        ExprStrMaster = ExprStrMaster.Replace("pi", CStr(Math.PI)).Replace(" ", "").Replace("+", ":A:").Replace("*", ":M:").Replace("/", ":D:").Replace("^", ":E:").Replace("ans", CStr(Answer))

        ' For functions
        ExprStrMaster = ExprStrMaster.Replace("[", ":w:").Replace("]", ":x:")

        ' Define Regexes (EMDAS)
        Dim AddRegex As New Regex("(-|_|\d|:v:)+:A:(_|\d|:v:)+")
        Dim MulRegex As New Regex("(-|_|\d|:v:)+:M:(_|\d|:v:)+")
        Dim DivRegex As New Regex("(-|_|\d|:v:)+:D:(_|\d|:v:)+")
        Dim ExpRegex As New Regex("(-|_|\d|:v:)+:E:(_|\d|:v:)+")
        Dim SubNumRegex As New Regex("(-|_|\d|:v:)+(:S:|_)(_|\d|:v:)+")

        ' Define Regexes (Trig functions)
        '   NOTE: :y: and :z: ( '(' and ')' ) used to be :w: and :x: ( '[' and ']' ) respectively

        ' Loop through parenthesis
        Dim PastExprStrMaster As String = ""
        While NumberRegex.Match(ExprStrMaster).Length <> ExprStrMaster.Length

            ' DEBUG
            'Console.WriteLine("----- MainEx: " & ExprStrMaster & " -----")

            ' If past expression equals current one, there is an error
            Dim MatchParen As Match = RegexParen.Match(ExprStrMaster)

            ' Operate on parenthesis-enclosed string
            Dim ExprStr As String = ExprStrMaster
            If MatchParen.Success() Then

                ExprStr = RegexParen.Match(ExprStrMaster).Value
                ExprStr = ExprStr.Substring(3, ExprStr.Length - 6)

            End If

            ' Unaltered copy of ExprStr
            Dim ExprStrCopy As String = ExprStr

            ' If ExprStr is a constant, drop the surrounding parenthesis and try again
            If NumberRegex.Match(ExprStr).Length = ExprStr.Length And MatchParen.Success Then

                ExprStrMaster = ExprStrMaster.Replace(MatchParen.Value, ExprStr)
                Continue While

            End If

            ' Step 0: Organize negatives
            ' Replace all subtraction -'s with ":S:"
            While SubRegex.Match(ExprStr).Success

                SubMatch = SubRegex.Match(ExprStr)
                ExprStr = ExprStr.Replace(SubMatch.Value, SubMatch.Value.Replace("-", ":S:"))

            End While

            ExprStr = ExprStr.Replace("--", ":A:")
            ExprStr = ExprStr.Replace("-", "_")
            ExprStr = ExprStr.Replace(".", ":v:")

            ' If the function is purely a number, exit the solving loop
            If NumberRegex.Match(ExprStrMaster).Length = ExprStrMaster.Length Then
                Continue While
            End If

            ' Main loop
            While True

                ' Debug
                'Console.WriteLine("TempEx: " & ExprStr)

                ' If the function isn't fully solvable, throw an exception (to say that there was a problem with the function)
                'If PastExprStrMaster = ExprStr Then
                '    Throw New Exception("FunctionEvaluate ERROR: Improper function entered; Function: " & ExprStrMasterIn & ", Solution: " & ExprStrMaster)
                '    Return 0
                'End If

                ' Basic formatting - after all, "--" = "+"
                ExprStr = ExprStr.Replace("--", ":A:")
                ExprStr = ExprStr.Replace(".", ":v:")

                ' Uses PEMDAS

                ' Step 2: Define Regex matches
                Dim AddMatch As Match = AddRegex.Match(ExprStr)
                SubMatch = SubNumRegex.Match(ExprStr)
                Dim MulMatch As Match = MulRegex.Match(ExprStr)
                Dim DivMatch As Match = DivRegex.Match(ExprStr)
                Dim ExpMatch As Match = ExpRegex.Match(ExprStr)

                ' Step 5: Exponents
                If ExpMatch.Success Then

                    ' Get A number
                    Dim NumA As String = NumberRegex.Match(ExpMatch.Value).Value
                    NumA = NumA.Replace(":v:", ".").Replace("_", "-")

                    ' Get B number
                    Dim NumB As String = NumberRegex.Match(ExpMatch.Value, NumA.Length + 2).Value
                    NumB = NumB.Replace(":v:", ".").Replace("_", "-")

                    ' Conduct operation
                    Dim Result As String = Math.Round(NumA ^ NumB, Precision)
                    Result = Result.Replace("-", "_").Replace(".", ":v:")

                    ExprStr = ExprStr.Replace(ExpMatch.Value, Result)


                    ' Reformat
                    ExprStr = ExprStr.Replace(".", ":v:")
                    ExprStr = ExprStr.Replace("-", "neg")

                    ExprStrMaster = ExprStrMaster.Replace(ExprStrCopy, ExprStr)

                    PastExprStrMaster = ExprStrMaster

                    ' Retry loop
                    Continue While

                End If

                ' Step 6: Multiplication (only acts if there is no division closer to the left)
                If MulMatch.Success And ((MulMatch.Index < DivMatch.Index) Or Not DivMatch.Success) Then

                    ' Get A number
                    Dim NumA As String = NumberRegex.Match(MulMatch.Value).Value
                    NumA = NumA.Replace(":v:", ".").Replace("_", "-")

                    ' Get B number
                    Dim NumB As String = NumberRegex.Match(MulMatch.Value, NumA.Length + 2).Value
                    NumB = NumB.Replace(":v:", ".").Replace("_", "-")

                    ' Conduct operation
                    Dim Result As String = CStr(Math.Round(CDbl(NumA) * CDbl(NumB), Precision))
                    Result = Result.Replace("-", "_").Replace(".", ":v:")

                    ExprStr = ExprStr.Replace(MulMatch.Value, Result)

                    ' Reformat
                    ExprStr = ExprStr.Replace(".", ":v:")
                    ExprStrMaster = ExprStrMaster.Replace(ExprStrCopy, ExprStr)

                    PastExprStrMaster = ExprStrMaster

                    ' Retry loop
                    Continue While

                End If

                ' Step 7: Division
                If DivMatch.Success Then

                    ' Get A number
                    Dim NumA As String = NumberRegex.Match(DivMatch.Value).Value
                    NumA = NumA.Replace(":v:", ".").Replace("_", "-")

                    ' Get B number
                    Dim NumB As String = NumberRegex.Match(DivMatch.Value, NumA.Length + 2).Value
                    NumB = NumB.Replace(":v:", ".").Replace("_", "-")

                    ' Conduct operation
                    Dim Result As String = CStr(Math.Round(CDbl(NumA) / CDbl(NumB), Precision))
                    Result = Result.Replace("-", "_").Replace(".", ":v:")

                    ExprStr = ExprStr.Replace(DivMatch.Value, Result)

                    ' Reformat
                    ExprStrMaster = ExprStrMaster.Replace(ExprStrCopy, ExprStr)

                    PastExprStrMaster = ExprStrMaster

                    ' Retry loop
                    Continue While

                End If

                ' Step 8: Addition (only acts if there is no subtraction closer to the left)
                If AddMatch.Success And ((AddMatch.Index < SubMatch.Index) Or (Not SubMatch.Success)) Then

                    ' Get A number
                    Dim NumA As String = NumberRegex.Match(AddMatch.Value).Value
                    NumA = NumA.Replace(":v:", ".").Replace("_", "-")

                    ' Get B number
                    Dim NumB As String = NumberRegex.Match(AddMatch.Value, NumA.Length + 2).Value
                    NumB = NumB.Replace(":v:", ".").Replace("_", "-")

                    ' Conduct operation
                    Dim Result As String = Math.Round(CDbl(NumA) + CDbl(NumB), Precision)
                    Result = Result.Replace("-", "_").Replace(".", ":v:")

                    ExprStr = ExprStr.Replace(AddMatch.Value.Replace("-", "_"), Result)

                    ' Reformat
                    ExprStr = ExprStr.Replace(".", ":v:")
                    ExprStrMaster = ExprStrMaster.Replace(ExprStrCopy, ExprStr)

                    PastExprStrMaster = ExprStrMaster

                    ' Retry loop
                    Continue While

                End If

                ' Step 9: Subtraction
                '   NOTE: Subtraction operates slightly differently than the other operators - this is because "1:S:3" and "1_3" are considered equivalents (all other operations only have a :*: form)
                If SubMatch.Success Then

                    ' Get A number
                    Dim NumA As String = NumberRegex.Match(SubMatch.Value).Value
                    NumA = NumA.Replace(":v:", ".").Replace("_", "-")

                    ' Get B number
                    Dim NumB As String = NumberRegex.Match(SubMatch.Value, NumA.Length + 2 - SubMatch.Value.Contains(":S")).Value
                    NumB = NumB.Replace(":v:", ".").Replace("_", "-")

                    ' Conduct operation
                    Dim Result As String = Math.Round(CDbl(NumA) - CDbl(NumB), Precision)
                    Result = Result.Replace("-", "_").Replace(".", ":v:")

                    ExprStr = ExprStr.Replace(SubMatch.Value, Result)
                    ExprStrMaster = ExprStrMaster.Replace(ExprStrCopy, ExprStr)

                    PastExprStrMaster = ExprStrMaster

                    ' Retry loop
                    Continue While

                End If

                ' Update master expression string
                ExprStrMaster = ExprStrMaster.Replace(ExprStrCopy, ExprStr)
                Exit While

            End While

            ' Check to make sure that the function is written properly
            If ExprStrCopy = ExprStr Then

                ' If a valid number is found, perform a few exception fixing operations and return it
                ExprStrMaster = ExprStrMaster.Replace(":v:", ".").Replace("_", "-")
                If NumberRegex.Match(ExprStrMaster).Length = ExprStrMaster.Length Then

                    Return CDbl(ExprStrMaster)

                Else

                    ' If the function cannot be completed, throw an exception
                    'Throw New ArithmeticException("FunctionEvaluate ERROR: Improper or unsolvable function entered: Function: " & ExprStrMasterIn & ", Solution: " & ExprStrMaster)
                    Return Integer.MaxValue

                End If

            End If

        End While

        ExprStrMaster = ExprStrMaster.Replace(":v:", ".").Replace("_", "-")
        If Double.TryParse(ExprStrMaster, New Double) Then
            Return CDbl(ExprStrMaster)
        Else
            Return 0
        End If

    End Function


End Class
