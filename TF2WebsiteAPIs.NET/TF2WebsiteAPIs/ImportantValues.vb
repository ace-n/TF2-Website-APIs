' This is where all the important-but-editable values (not just synonyms) go
Public Class ImportantValues

    Public Shared ReadOnly Synonyms_Each As String() = {"each", "ea."}
    Public Shared ReadOnly Synonyms_EqualSign As String() = {"-", ":", "for"}

    ' List of banned items
    Public Shared BannedItems As String() = {"game from steam", "offers", "craft hat", "weapon", "dota 2"}

    ' --- WH stuff ---
    ' Default overstock limit
    Public Const DefaultOverstock As Integer = 80

    ' Nonstandard overstock limits
    Public Shared ReadOnly Limit1000 As String() = {"refined metal", "mann co. supply crate", "tour of duty ticket", "mann co. supply crate key", "backpack expander"}
    Public Shared ReadOnly Limit200 As String() = {"reclaimed metal", "upgrade to premium gift", "scrap metal"}
    Public Shared ReadOnly Limit800 As String() = {"vintage bill's hat", "bill's hat"}

    ' --- PriceParser stuff ---
    '   NOTE: Most synonym arrays must be from LONGEST to SHORTEST to avoid match overlap
    Public Shared ReadOnly Synonyms_And As String() = {"and", "plus", "as well as", "in addition to"}
    Public Shared ReadOnly Synonyms_AndNot As String() = {"minus"}

    ' Craftable Hats
    Public Shared ReadOnly Synonyms_CraftHats As String() = {"craft hats", "craftable hats", "clean dropped hats", "clean hats"}
    Public Shared RefinedPerCraftHat As Double = 1.22

    ' Key(s)
    Public Shared ReadOnly Synonyms_Key As String() = {"mann co. supply crate key", "supply crate key", "key"}
    Public Shared KeyValue As Integer = 11000 ' 4500 * 3.66

    ' Metal
    Public Shared ReadOnly Synonyms_Metal3 As String() = {"refined metal", "refined", "ref"}       ' Refined
    Public Shared ReadOnly Value_Metal3 As Integer = 18

    Public Shared ReadOnly Synonyms_Metal2 As String() = {"reclaimed metal", "reclaimed", "rec"}   ' Reclaimed
    Public Shared ReadOnly Value_Metal2 As Integer = 6

    Public Shared ReadOnly Synonyms_Metal1 As String() = {"scrap metal", "scrap"}                  ' Scrap
    Public Shared ReadOnly Value_Metal1 As Integer = 2

    ' Weapons
    Public Shared ReadOnly Synonyms_Weapon As String() = {"weapon", "wep"}
    Public Shared ReadOnly Value_Weapon As Integer = 1
    Public Shared WeaponPriceWHc As Integer = 250

End Class
