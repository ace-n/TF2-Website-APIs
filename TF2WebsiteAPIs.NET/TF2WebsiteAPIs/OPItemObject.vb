' An object for storing items and their related methods
Public Class OPItemObject

    ' Key attributes of an item: Level, Craft, Name, Quality, Craftable
    '   NOTE: These must be initialized to their null values for things to work properly
    Public Level As Integer = -1
    Public Craft As Integer = -1
    Public Name As String = ""
    Public Quality As String = ""
    Public Uncraftable As Boolean = False

    ' Convenience constructor
    Public Sub New(ByVal Level As Integer, ByVal Craft As Integer, ByVal Name As Integer, ByVal Quality As String, Optional ByVal Uncraftable As Boolean = False)

        Me.Level = Level
        Me.Craft = Craft
        Me.Name = Name
        Me.Quality = Quality
        Me.Uncraftable = Uncraftable

    End Sub

    ' Copy constructor
    Public Sub New(ByVal ItemObj As OPItemObject)

        Me.Level = ItemObj.Level
        Me.Craft = ItemObj.Craft
        Me.Name = ItemObj.Name
        Me.Quality = ItemObj.Quality
        Me.Uncraftable = ItemObj.Uncraftable

    End Sub

    ' Null constructor
    Public Sub New()
    End Sub

    ' Null checker
    Public Function IsNull()
        Return Me.Level = -1 AndAlso Me.Craft = -1 AndAlso Me.Name.Length = 0 AndAlso Me.Quality.Length = 0
    End Function

    ' Checks if the current item is an item of currency
    Public Function IsCurrency() As Boolean
        Return Name.EndsWith(" Metal") OrElse Name.EndsWith(" Key")
    End Function

End Class
