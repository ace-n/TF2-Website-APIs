Imports System.IO

Public Class WHInteraction

    ' URL: http://www.tf2wh.com/inventory.php?csv=1

    Private Shared Q As String = Chr(34)

    ' List of items
    Public Shared WHDataCache As New List(Of WHListingObject)

    ' A WebClient (used to download stuff)
    Public Shared WC1 As New Net.WebClient

    ' Fetches (most) values
    Public Shared Sub GetNewWHData()

        ' -- Read local WH Csv --
        Dim WHLines As String() = WC1.DownloadString("http://www.tf2wh.com/inventory.php?csv=1").Replace(vbLf, vbCrLf).Split(vbCrLf)

        ' -- Iterate through CSV file --
        Dim WHTempList As New List(Of WHListingObject)
        For i = 1 To WHLines.Count - 1

            ' Get current line
            Dim CurLine As String = WHLines.GetValue(i).ToString

            ' Skip null/error-triggering lines (less than 23 characters causes the "Remove chaff" block below to break)
            If CurLine.Length < 23 Then
                Continue For
            End If

            ' Remove chaff
            CurLine = CurLine.Remove(0, 23)
            Dim CurSplit As String() = CurLine.Split(",")

            ' - Item properties -
            Dim WHListing As New WHListingObject

            ' Name
            WHListing.Name = CurSplit.GetValue(0).ToString
            WHListing.Name = WHListing.Name.Remove(WHListing.Name.Length - 8)

            ' Count
            WHListing.CurrentCnt = Integer.Parse(CurSplit.GetValue(2))

            ' Prices
            WHListing.SellToWHc = Integer.Parse(CurSplit.GetValue(3))  ' Sell = what the WH pays YOU for the item
            WHListing.BuyFromWHc = Integer.Parse(CurSplit.GetValue(4)) ' Buy  = what YOU pay the WH for the item

            ' Overstock
            Dim NameLower As String = WHListing.Name.ToLowerInvariant
            WHListing.OSLimitCnt = ImportantValues.DefaultOverstock
            If NameLower.Length > 0 Then
                If ImportantValues.Limit1000.Contains(NameLower) Then
                    WHListing.OSLimitCnt = 1000
                ElseIf ImportantValues.Limit800.Contains(NameLower) Then
                    WHListing.OSLimitCnt = 800
                ElseIf ImportantValues.Limit200.Contains(NameLower) Then
                    WHListing.OSLimitCnt = 200
                End If
            End If

            ' Add listing object to temporary list
            WHTempList.Add(WHListing)

        Next

        ' Update main list (so it contains the updated data in the temporary list)
        '   The temporary list setup is used to minimize the downtime of the main item list
        WHDataCache.Clear()
        WHDataCache.AddRange(WHTempList)

    End Sub

End Class
