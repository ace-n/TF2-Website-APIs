' An object for storing trades
Public Class OPTradeObject

    ' NOTE: All the properties below must be initialized to their null values for things to work properly

    ' Lists of items in question
    Public ItemsSelling As New List(Of OPItemObject)
    Public ItemsBuying As New List(Of OPItemObject)

    ' Trade's Outpost ID (used to access the trade itself)
    Public OutpostID As String = ""

    ' Owner of trade
    Public OwnerName As String = ""

    ' Trade notes (if any)
    Public Notes As String = ""

    ' Convenience constructor
    Public Sub New(ByVal ItemsSelling As List(Of OPItemObject), ByVal ItemsBuying As List(Of OPItemObject), ByVal OwnerName As String, ByVal OutpostID As String, ByVal Notes As String)

        ' Item lists
        Me.ItemsSelling = ItemsSelling
        Me.ItemsBuying = ItemsBuying

        ' Other trade info
        Me.OutpostID = OutpostID
        Me.OwnerName = OwnerName
        Me.Notes = Notes

    End Sub

    ' Null constructor
    Public Sub New()
    End Sub

    ' Null checker
    Public Function IsNull()
        Return ItemsSelling.Count = 0 AndAlso ItemsBuying.Count = 0 ' Everything else is meaningless if these are null
    End Function

    ' Copy constructor
    Public Sub New(ByVal TradeObj As OPTradeObject)

        ' Item lists
        Me.ItemsSelling = TradeObj.ItemsSelling
        Me.ItemsBuying = TradeObj.ItemsBuying

        ' Other trade info
        Me.OutpostID = TradeObj.OutpostID
        Me.OwnerName = TradeObj.OwnerName
        Me.Notes = TradeObj.Notes

    End Sub

    ' Blacklist filter
    Public Function IsBlacklisted() As Boolean

        ' - Check for prohibited items -
        ' These are prohibited if the trade consists of nothing but them
        Dim TradeHasOKItems As Boolean = False

        ' Buying
        For Each I In ItemsSelling
            If Not ImportantValues.BannedItems.Contains(I.Name.ToLowerInvariant) Then
                TradeHasOKItems = True
                Exit For
            End If
        Next
        If Not TradeHasOKItems Then
            Return True ' All buying items are invalid
        End If

        ' Selling
        TradeHasOKItems = False ' Variable recycling
        For Each I In ItemsBuying
            If Not ImportantValues.BannedItems.Contains(I.Name.ToLowerInvariant) Then
                TradeHasOKItems = True
                Exit For
            End If
        Next
        If Not TradeHasOKItems Then
            Return True  ' All selling items are invalid
        End If

        ' - Tests passed -
        Return False

    End Function

End Class
