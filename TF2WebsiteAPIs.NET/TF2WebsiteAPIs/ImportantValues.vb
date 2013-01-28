' This is where all the important-but-editable values (not just synonyms) go
Public Class ImportantValues

    Public Shared Synonyms_Each As String() = {"each", "ea."}
    Public Shared Synonyms_EqualSign As String() = {"-", ":", "for"}

    ' List of banned items
    Public Shared BannedItems As String() = {"game from steam", "offers", "craft hat", "weapon", "dota 2"}

    ' --- WH stuff ---
    ' Default overstock limit
    Public Shared DefaultOverstock As Integer = 80

    ' Nonstandard overstock limits
    Public Shared Limit1000 As String() = {"refined metal", "mann co. supply crate", "tour of duty ticket", "mann co. supply crate key", "backpack expander"}
    Public Shared Limit200 As String() = {"reclaimed metal", "upgrade to premium gift", "scrap metal"}
    Public Shared Limit800 As String() = {"vintage bill's hat", "bill's hat"}

    ' --- PriceParser stuff ---
    '   NOTE: Most synonym arrays must be from LONGEST to SHORTEST to avoid match overlap
    Public Shared Synonyms_And As String() = {"and", "plus", "as well as", "in addition to"}
    Public Shared Synonyms_AndNot As String() = {"minus"}

    ' Craftable Hats
    Public Shared Synonyms_CraftHats As String() = {"craft hats", "craftable hats", "clean dropped hats", "clean hats"}
    Public Shared RefinedPerCraftHat As Double = 1.22

    ' Key(s)
    Public Shared Synonyms_Key As String() = {"mann co. supply crate key", "supply crate key", "key"}
    Public Shared KeyValue As Integer = 11000 ' 4500 * 3.66

    ' Metal
    Public Shared Synonyms_Metal3 As String() = {"refined metal", "refined", "ref"}     ' Refined
    Public Shared Value_Metal3 As Integer = 18

    Public Shared Synonyms_Metal2 As String() = {"reclaimed metal", "reclaimed", "rec"}   ' Reclaimed
    Public Shared Value_Metal2 As Integer = 6

    Public Shared Synonyms_Metal1 As String() = {"scrap metal", "scrap"}              ' Scrap
    Public Shared Value_Metal1 As Integer = 2

    ' Weapons
    Public Shared Synonyms_Weapon As String() = {"weapon", "wep"}
    Public Shared Value_Weapon As Integer = 1
    Public Shared WeaponPriceWHc As Integer = 250

End Class
