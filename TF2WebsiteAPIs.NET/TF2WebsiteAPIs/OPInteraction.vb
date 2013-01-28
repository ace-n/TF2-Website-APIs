Imports System.Text.RegularExpressions

' This class interacts with Outpost (for now, that means scraping/processing the Outpost HTML)
Public Class OutpostInteraction

    Public Shared Q As String = Chr(34)
    Public Shared WClient As New Net.WebClient

    ' Fetches values
    Public Shared Function GetTrades(ByVal URL As String) As List(Of OPTradeObject)

        ' -- Download Outpost HTML --
        Dim OPHTML As String = WClient.DownloadString(URL).Replace("<br />", vbLf)

        ' -- Format Outpost HTML --

        ' Remove everything before and including the "</noscript>" (so only trade HTML code remains)
        Dim Idx As Integer = OPHTML.IndexOf("</noscript>")
        If Idx > 0 Then
            OPHTML = OPHTML.Remove(0, Idx)
        End If

        ' Format line breaks properly
        OPHTML = OPHTML.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim OPLines As String() = OPHTML.Split(vbLf)

        ' -- Iterate through Outpost HTML --

        ' List of trades (the function's output)
        Dim TradeList As New List(Of OPTradeObject)

        ' One way flag to turn trade scanning on (turns on when the first <div class="trade"> is encountered)
        Dim ParsingActive As Boolean = False

        ' (Semi - ) One way flag to indicate whether current item list is being bought or sold
        Dim IsBuying As Boolean = False

        ' Active trade
        Dim ActiveTrade As New OPTradeObject

        ' Active item
        Dim ActiveItem As New OPItemObject

        ' Whether notes are currently being added to
        Dim NotesEditingMode As Integer = 0

        ' Iteration
        For i = 0 To OPLines.Count - 1

            Dim CurLine As String = OPLines.GetValue(i).ToString.Trim

            ' -- Handle new trades --
            If CurLine.StartsWith("<div class=" & Q & "trade" & Q) OrElse i = OPLines.Count - 1 Then

                '  - Add last item of current trade to current trade object's item list -
                If Not ActiveItem.IsNull Then
                    If IsBuying Then
                        ActiveTrade.ItemsBuying.Add(New OPItemObject(ActiveItem))
                    Else
                        ActiveTrade.ItemsSelling.Add(New OPItemObject(ActiveItem))
                    End If
                End If

                ' Add current trade to trade list
                If Not ActiveTrade.IsNull Then
                    TradeList.Add(New OPTradeObject(ActiveTrade))
                End If

                ' - Initialize new trade -
                ParsingActive = True ' Turn HTML parsing on

                ActiveItem = New OPItemObject   ' Refresh item object
                ActiveTrade = New OPTradeObject ' Refresh trade object
                IsBuying = False              ' Reset IsBuying

                Continue For

            End If

            ' Get trade ID
            If CurLine.StartsWith("<span><a href=" + Q + "/trade/") Then

                ActiveTrade.OutpostID = Regex.Match(CurLine.Substring(22), "\d+").Value

            End If

            ' -- Handle new items --
            If CurLine.StartsWith("<div class=" & Q & "item") Then

                ' Add previous item to current trade object's item list
                If Not ActiveItem.IsNull Then
                    If IsBuying Then
                        ActiveTrade.ItemsBuying.Add(New OPItemObject(ActiveItem))
                    Else
                        ActiveTrade.ItemsSelling.Add(New OPItemObject(ActiveItem))
                    End If
                End If

                ActiveItem = New OPItemObject

                ' Determine item's quality (which is included in the new item line)
                Dim Quality As String = CurLine.Remove(0, 17)
                Quality = Quality.Remove(Quality.IndexOf(Q))

                ' Update item and continue loop
                ActiveItem.Quality = Quality
                Continue For

            End If

            ' -- Handle current item (and its details) --

            ' IsBuying toggle
            If CurLine.StartsWith("<div class=" & Q & "arrow" & Q & "></div>") Then

                ' Add previous item to current trade object's item list (before IsBuying is toggled, so that it is in the right list)
                If Not ActiveItem.IsNull Then
                    If IsBuying Then
                        ActiveTrade.ItemsBuying.Add(New OPItemObject(ActiveItem))
                    Else
                        ActiveTrade.ItemsSelling.Add(New OPItemObject(ActiveItem))
                    End If
                End If

                ActiveItem = New OPItemObject

                ' Toggle IsBuying
                IsBuying = Not IsBuying
                Continue For

            End If

            ' Name
            If CurLine.StartsWith("<h1 class=" & Q & ActiveItem.Quality) Then ' Note that ActiveItem.quality should be valid by this point (because quality comes before name in the HTML)

                Dim NameStr As String = CurLine.Remove(0, 13 + ActiveItem.Quality.Length)
                If NameStr.Length > 5 Then
                    NameStr = NameStr.Remove(NameStr.Length - 5)
                Else
                    NameStr = ""
                End If

                ' Update item and continue loop
                ActiveItem.Name = NameStr
                Continue For

            End If

            ' Level
            If CurLine.StartsWith("<span class=" & Q & "level" & Q & ">") Then

                ' Update item and continue loop
                Integer.TryParse(Regex.Match(CurLine.Remove(0, 26), "\d+").Value, ActiveItem.Level) ' TryParse is necessary because Parse doesn't handle null strings well
                Continue For

            End If

            ' Craft # (if any)
            If CurLine.StartsWith("<div class=" & Q & "equipped") Then '  + Q + ">#"

                Integer.TryParse(Regex.Match(CurLine.Remove(0, 23), "\d+").Value, ActiveItem.Craft) ' TryParse is necessary because Parse doesn't handle null strings well
                Continue For

            ElseIf CurLine.StartsWith("<span class=" & Q & "label" & Q & ">Craft #</span>") Then

                Integer.TryParse(Regex.Match(CurLine.Remove(0, 34), "\d+").Value, ActiveItem.Craft) ' TryParse is necessary because Parse doesn't handle null strings well
                Continue For

            End If

            ' Craftability
            If CurLine.StartsWith("<span class=" & Q & "negative" & Q & ">(Uncraftable)</span>") Then
                ActiveItem.Uncraftable = True
            End If

            ' Notes
            Dim Cond1 As Boolean = TradeList.Count <> 0 AndAlso TradeList.Last.Notes.Length < 2
            If Cond1 AndAlso CurLine.StartsWith("<div class=" & Q + "text" & Q & ">") Then
                'ActiveTrade.Notes = CurLine.Remove(0, 18) ' NOTE: This doesn't support multiple line notes
                'ActiveTrade.Notes = Regex.Replace(ActiveTrade.Notes, "<(/|\w|\W)+?>", "")

                ' TF2OP is derpy and includes the notes in a separate <div class="trade">
                TradeList.Last.Notes = CurLine.Remove(0, 18) ' NOTE: This doesn't support multiple line notes

                ' Turn on note editing
                NotesEditingMode = 1

            ElseIf Cond1 AndAlso CurLine.StartsWith("<div class=" & Q & "notes expandable" & Q & ">") Then

                ActiveTrade.Notes = CurLine.Remove(0, 30) ' NOTE: This doesn't support multiple line notes

                ' Turn on note editing
                NotesEditingMode = 2

            ElseIf CurLine.StartsWith("</div>") And NotesEditingMode <> 0 Then

                ' Turn off note editing
                NotesEditingMode = 0

                ' Remove HTML from notes
                If TradeList.Count <> 0 Then
                    TradeList.Last.Notes = Regex.Replace(TradeList.Last.Notes, "<(/|\w|\W)+?>", "")
                End If

            ElseIf NotesEditingMode = 1 Then

                TradeList.Last.Notes &= vbLf & CurLine

            ElseIf NotesEditingMode = 2 Then

                ActiveTrade.Notes &= vbLf & CurLine

            End If
        Next

        ' Return result
        Return TradeList

    End Function

End Class
