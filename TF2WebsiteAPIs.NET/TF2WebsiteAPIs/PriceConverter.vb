Imports System.Text.RegularExpressions
Imports System.Drawing

' This class converts price strings into item representations using some simple NLP
'   Currently, it uses arrays to store things. Text files would be a better choice because of the sheer volume of data involved.
Public Class PriceParser

    ' --- Methods ---
    Public Shared Function PriceParse(ByVal Input As String) As String ' When complete, this will return an Integer

        ' If the input is a pure number...
        If Regex.Match(Input, "\d+").Length = Input.Length Then
            If Input.Length < 8 AndAlso Input.Length > 0 Then
                Return Input
            Else
                Return Integer.MaxValue.ToString ' Something probably went wrong
            End If
        End If

        ' If the input is null
        If Input.Length < 1 Then
            Return Integer.MaxValue
        End If

        ' Value
        Dim Value As Integer = 0

        ' Subtraction
        Input = Regex.Replace(Input, "(?<=\d+)\s+(?=[a-z])", "*")
        For Each S As String In ImportantValues.Synonyms_AndNot
            Input = Regex.Replace(Input, "(?<=(\A|\s))" & S & "(?=(\s|\Z))", "-")
        Next

        ' Addition
        Input = Regex.Replace(Input, "(?<=[a-z])\s+(?=\d+)", "+")
        For Each S As String In ImportantValues.Synonyms_And
            Input = Regex.Replace(Input, "(?<=(\A|\s))" & S & "(?=(\s|\Z))", "+")
        Next

        ' -- Conversions --
        Dim Replacement As String

        ' Craft hats
        Replacement = Math.Floor(ImportantValues.Value_Metal3 * ImportantValues.RefinedPerCraftHat).ToString
        For Each S As String In ImportantValues.Synonyms_CraftHats
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        ' Keys
        Replacement = ImportantValues.KeyValue.ToString
        For Each S As String In ImportantValues.Synonyms_Key
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        ' Metal
        Replacement = ImportantValues.Value_Metal3.ToString & "*" & ImportantValues.WeaponPriceWHc.ToString
        For Each S As String In ImportantValues.Synonyms_Metal3
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        Replacement = ImportantValues.Value_Metal2.ToString & "*" & ImportantValues.WeaponPriceWHc.ToString
        For Each S As String In ImportantValues.Synonyms_Metal2
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        Replacement = ImportantValues.Value_Metal1.ToString & "*" & ImportantValues.WeaponPriceWHc.ToString
        For Each S As String In ImportantValues.Synonyms_Metal1
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        Replacement = ImportantValues.Value_Weapon.ToString & "*" & ImportantValues.WeaponPriceWHc.ToString
        For Each S As String In ImportantValues.Synonyms_Weapon
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
            'Input = Regex.Replace(Input, "\*" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        ' Remove bprice(itemPrice) functions - only bprice(itemName) is valid
        Input = Regex.Replace(Input, "(s|b)price\((?<n>\d+)\)", "${n}")

        ' -- Price searching --
        '   This handles all of the "price(x)" functions by searching the WH Listings (which come from WHInteraction.vb)
        Dim PriceRegex As New Regex("(s|b)price\((\w|'|\.|,|\s)+\)")
        Dim PriceMatch As Match = PriceRegex.Match(Input)
        While PriceMatch.Success

            ' Formatting
            Dim CurName As String = PriceMatch.Value.Remove(0, 7)
            CurName = CurName.Remove(CurName.Length - 1)

            ' Find the price of the current match
            Dim ItemIdx As Integer = -1
            Dim BestSimilarity As Double = 0.6
            For i = 0 To WHInteraction.WHDataCache.Count - 1

                Dim W As WHListingObject = WHInteraction.WHDataCache.Item(i)

                ' Uses fuzzy search
                Dim FLS As Point = FindLongestSequence(W.Name, CurName)

                Dim TempSimilarity As Double = FLS.Y / Math.Max(W.Name.Length, CurName.Length)
                If TempSimilarity > BestSimilarity AndAlso FLS.X < 4 Then
                    BestSimilarity = TempSimilarity
                    ItemIdx = i
                End If

            Next

            ' Add result to input
            Dim CurWHListing As WHListingObject
            Input = Input.Remove(PriceMatch.Index, PriceMatch.Length)
            If ItemIdx <> -1 Then

                CurWHListing = WHInteraction.WHDataCache.Item(ItemIdx)
                If PriceMatch.Value.Chars(0) = "b"c Then
                    Input = Input.Insert(PriceMatch.Index, CurWHListing.BuyFromWHc.ToString)
                Else
                    Input = Input.Insert(PriceMatch.Index, CurWHListing.SellToWHc.ToString)
                End If

            Else

                Input = Input.Insert(PriceMatch.Index, 0) ' Assume the item's price is 0

            End If

            ' Update PriceMatch
            PriceMatch = PriceRegex.Match(Input, PriceMatch.Index)

        End While

        ' -- Chaff removal --
        '   Note that this may cause inaccurate interpretations of trades due to missing information. (Though such error is acceptable once in a while)

        ' Select first valid algebraic string
        Input = Regex.Match(Input.Replace(" ", ""), "(\+|-|/|\*|\.|\d)+").Value

        ' Letters
        ' Input = Regex.Replace(Input, "([a-zA-Z]|\s|\.|,|')+", "").Trim

        ' Beginning/ending operators
        Dim OperatorsStr As String = "(\*|\\|/|\+|-|\.)"
        Input = Regex.Replace(Input, "\A" & OperatorsStr, "")
        Input = Regex.Replace(Input, OperatorsStr & "\Z", "")



        ' ---Algebraic Operations --
        '   NOTE: RegexCalc WILL be used here (mainly because MDAS functionality is necessary, but also in case P/E function becomes necessary later on)
        If Not Integer.TryParse(Input, New Integer) Then
            Input = RegexCalc.RegexFunctionEvaluate(Input)
        End If

        ' Check for nulls
        If Input.Length < 1 Then
            Return Integer.MaxValue
        End If

        Return Input

    End Function

    ' Sequence finder, used for fuzzy searches
    Public Shared Function FindLongestSequence(ByRef Haystack As String, ByRef Needle As String) As Point

        ' X = Longest sequence start; Y = Longest sequence length
        Dim ReturnPoint As New Point(0, 0)

        ' Longest complete sequence
        Dim LongestSeqLen As Integer = 0

        For i = 0 To Haystack.Length - 1

            ' Longest length in the current string position
            Dim TempLen As Integer = 0

            ' Iteration
            For j = 0 To Needle.Length - 1

                ' Exception handler
                If i + TempLen >= Haystack.Length Then
                    Exit For
                End If

                ' Equality check
                If Haystack.Chars(i + TempLen) = Needle.Chars(j) Then
                    TempLen += 1
                ElseIf TempLen > 0 Then
                    Exit For
                End If

            Next

            ' Longest length so far
            If TempLen > ReturnPoint.Y Then
                ReturnPoint.X = i
                ReturnPoint.Y = TempLen
            End If

        Next

        ' Return
        Return ReturnPoint

    End Function


End Class
