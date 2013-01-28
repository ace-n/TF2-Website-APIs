Imports System.Text.RegularExpressions

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
        For Each S As String In SynonymsList.Synonyms_AndNot
            Input = Regex.Replace(Input, "(?<=(\A|\s))" & S & "(?=(\s|\Z))", "-")
        Next

        ' Addition
        Input = Regex.Replace(Input, "(?<=[a-z])\s+(?=\d+)", "+")
        For Each S As String In SynonymsList.Synonyms_And
            Input = Regex.Replace(Input, "(?<=(\A|\s))" & S & "(?=(\s|\Z))", "+")
        Next

        ' -- Conversions --
        Dim Replacement As String

        ' Craft hats
        Replacement = Math.Floor(SynonymsList.Value_Metal3 * SynonymsList.RefinedPerCraftHat).ToString
        For Each S As String In SynonymsList.Synonyms_CraftHats
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        ' Keys
        Replacement = SynonymsList.KeyValue.ToString
        For Each S As String In SynonymsList.Synonyms_Key
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        ' Metal
        Replacement = SynonymsList.Value_Metal3.ToString & "*" & SynonymsList.WeaponPriceWHc.ToString
        For Each S As String In SynonymsList.Synonyms_Metal3
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        Replacement = SynonymsList.Value_Metal2.ToString & "*" & SynonymsList.WeaponPriceWHc.ToString
        For Each S As String In SynonymsList.Synonyms_Metal2
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        Replacement = SynonymsList.Value_Metal1.ToString & "*" & SynonymsList.WeaponPriceWHc.ToString
        For Each S As String In SynonymsList.Synonyms_Metal1
            Input = Regex.Replace(Input, "((s|b)price\(|(?<=(\*))|\s|\A)" & S & "(s|\)|)", Replacement, RegexOptions.IgnoreCase)
        Next

        Replacement = SynonymsList.Value_Weapon.ToString & "*" & SynonymsList.WeaponPriceWHc.ToString
        For Each S As String In SynonymsList.Synonyms_Weapon
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
            For i = 0 To WHInteraction..Count - 1

                Dim W As WHListingObject = WHInteraction.WHItemListings.Item(i)

                ' Uses fuzzy search
                Dim FLS As Point = TradeComprehender.FindLongestSequence(W.Name, CurName)

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

                CurWHListing = WHInteraction.WHItemListings.Item(ItemIdx)
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

End Class
