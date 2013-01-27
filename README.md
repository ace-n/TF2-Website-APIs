tf2-website-apis
================

Web-scraping APIs used to fetch data with TF2 Outpost and TF2 Warehouse.

More coming in a few days.

Note: Most (if not all) functions require the "ImportantValues.vb" file to run properly.

This project uses my RegexCalc math parser (which can be found on my GitHub page.)

LICENSING: Use it for whatever you like, as long as you give me credit by name. One restriction: no profit can be made off of these files by themselves. (Profiting off of derivative works is fine.)

======== API classes ========
[Warehouse API - WHInteraction.vb]
To use this API, first call its GetNewWHData() method. This method will download item data from TF2 Warehouse into the program.
The downloaded data is stored as a list of WHItemObjects in the class' WHDataCache member variable.

[Outpost API - OPInteraction.vb]
To use this API, pass a URL to either the TF2 Outpost homepage ("http://www.tf2outpost.com") or a specific 
TF2 Outpost trade ("http://www.tf2outpost.com/trade/[trade number]") to its GetTrades() method. This method
returns trade information as a list of OPTradeObjects.

========= API objects ========
[OPItemObject.vb]
The member values of this class are self-explanatory. By default, they are initialized to either -1 (for numbers) or "" (for strings).
IsCurrency() - Checks if the current item is a common currency item (either a type of Metal, or a Supply Crate Key)
IsNull() - Returns TRUE if ALL important values of the item are equal to their null values, FALSE otherwise.

[OPTradeObject.vb]
Once again, the member values of this class are self-explanatory and its default values are either -1 (for numbers) or "" (for strings).
IsNull() - Returns TRUE if no items are being bought OR being sold, FALSE otherwise.
IsBlacklisted() - Returns TRUE if an item's name is blacklisted, FALSE otherwise. The blacklist is contained in ImportantValues.vb.
                  Note that if the item's name CONTAINS (not necessarily equals) a blacklisted term, the item WILL be blacklisted.

[WHListingObject.vb]
Once again, the member values of this class are self-explanatory and its default values are either -1 (for numbers) or "" (for strings).
IsCurrency() - See OPItemObject's method of the same name
IsNull() - See OPItemObject's method of the same name

[PriceConverter.vb]
PriceParse() - When passed a string, this method will attempt to extract a price (in terms of TF2 Warehouse Credits) from the string. (E.g. "3.66 ref" --> "3.66*4500" --> "16470")
               However, it can make mistakes (especially if fed input that doesn't contain a price or an item name it doesn't recognize)
               NOTE: Item names must be formatted as either "bprice(Item Name)" or "sprice(Item Name)".
                    bprice() - Price to BUY the item from TF2 Warehouse
                    sprice() - Price to SELL the item to TF2 Warehouse



