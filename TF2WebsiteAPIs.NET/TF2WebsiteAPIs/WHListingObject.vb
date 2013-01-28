' An object for storing TF2WH item listings
Public Class WHListingObject

    ' -- Listing properties --

    ' Item name
    Public Name As String = ""

    ' Prices
    Public SellToWHc As Integer = -1
    Public BuyFromWHc As Integer = -1

    ' Quantities
    Public CurrentCnt As Integer = -1
    Public OSLimitCnt As Integer = ImportantValues.DefaultOverstock ' Overstock limit - auto-initialized to default

    ' Convenience constructor
    Public Sub New(ByVal Name As String, ByVal BuyFromWHc As Integer, ByVal SellToWHc As Integer, ByVal CurrentCnt As Integer, Optional ByVal OSLimitCnt As Integer = ImportantValues.DefaultOverstock)

        Me.Name = Name

        ' Prices
        Me.BuyFromWHc = BuyFromWHc
        Me.SellToWHc = SellToWHc

        ' Quantities
        Me.CurrentCnt = CurrentCnt
        Me.OSLimitCnt = OSLimitCnt

    End Sub

    ' Copy constructor
    Public Sub New(ByVal ListingObj As WHListingObject)

        Me.Name = ListingObj.Name

        ' Prices
        Me.BuyFromWHc = ListingObj.BuyFromWHc
        Me.SellToWHc = ListingObj.SellToWHc

        ' Quantities
        Me.CurrentCnt = ListingObj.CurrentCnt
        Me.OSLimitCnt = ListingObj.OSLimitCnt

    End Sub

    ' Null constructor
    Public Sub New()
    End Sub

    ' Null checker
    Public Function IsNull()
        Return Me.SellToWHc = -1 AndAlso Me.BuyFromWHc = -1 AndAlso Me.CurrentCnt = -1
    End Function

    ' Checks if the current listing is of an item of currency
    Public Function IsCurrency() As Boolean
        Return Name.EndsWith(" Metal") OrElse Name.EndsWith(" Key")
    End Function

End Class
