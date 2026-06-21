# Items and Inventory

[💻 View Scripts on GitHub](https://github.com/1nf1n1t3f1r3/Inventory-and-Items-Minirepo)

The Items and Inventory systems are highly interconnected. The ItemDatabase is a relatively simple .JSON file containing all the raw data, like the ID, Name and Sprite, which everything has, and more specific things like itemDurability and meleeAttackDamage. The way Items work in Terraria, and similar games, is that your main 'Use' action (LMB) changes depending on what item you're holding. Thus, your LMB needs to inherit from the item. Swapping which Item is 'Active' changes the 'Use' action accordingly. A character effectively becomess a Swiss Army Knife by being able to quickly select any tool they need from their Hotbar (or even whole Inventory). It's a different approach from other games, where your actions are directly linked to a button, like pressing the 'A' or 'Bumper' button on a controller.

The Inventory system then gets the item by the ID. Every individual Item is created by Inheritance, Item.CS (ID, Sprite, etc.) -> ItemWeapon.CS (Attack Power/Speed etc.) or ItemBlock.CS (Durability, Placability, etc.) The PlayerStats script refreshes whenever the inventory changes and updates based on what's currently equipped or held, like miningSpeed for a Pickaxe and meleeAttackDamage for a Sword.All of this data is stored in a simple .JSON database file. In-game, the system only asks for IDs, then pulls the information from there.

I'll focus on the way Inventory works in this little write-up, but the UI, Database and propagation from Item.CS are, of course, also important parts of how it all works.

## Inventory

Of course, in order to be able to do something with an Item at all, an Inventory is needed to actually hold anything. The inventory is made out of individual InventorySlots, which is initialized with as many times as the Inventory script requests. The InventorySlots themselves don't care at all what exactly they're holding. They just want to know the ID and the Count. The rest is for other systems to look up in the Database. The Inventory as a whole is actually a SyncList, which is a Mirror feature to keep it working in Multiplayer (but, for brevity, I won't go into the Multiplayer aspect here). It's job is to manage the relations between slots.

For example, if a player picks up an item and one slot is full, it'll go to the next. If an item already exists in the Inventory, any subsequent pick-ups get stacked on it. The Player can move things around in its Inventory, or throw it out, by clicking and dragging, or by using the Right Mouse Button (RMB) to split one off the stack. When that happens, the Item goes into a special 'CursorSlot', which is like an InventorySlot that's only active when the Player is dragging things around. Tinkering with this was very interesting, because it's something that shows up all the time and I must have used thousands of times, but I had literally never thought about it before.

### Pieces

Most inventory interactions eventually boil down to moving an item from one slot to another. Which is simple but also requires accounting for splitting stacks, merging stacks, swapping items, dropping them, equipping them, and picking them up when the inventory is already full. Most of it can be reduced to these helper functions: TryGetSlot, SetSlot, MergeIntoSlot and SetCursorSlot. TryGetSlot is the safe way to check what's actually held somewhere. SetSlot updates it and gets used by basically everything else, wehther it's picking things off the ground, dragging things around the inventory, splitting, merging, dropping, or trying to pick up an item when the Inventory is already full. Getting all this working right isn't necessarily hard, but you do have ot make sure the system accounts for everything it needs to do.

Once that's all in place, the Inventory starts feeling very familiar. LMB to pick up a stack, and place it in an empty slot, or click on an already occupied slot to swap them around. RMB to detract one from a stack and put it in the CursorSlot.

<!-- Gif Here -->

### Selection

The Active Item is what makes a character a 'Swiss Army Knife'. This get changed by clicking or scrolling through the Inventory, which, in turn, passes information on to PlayerStats and changes their 'Use' Action. It also displays the item that's currently active on the character, which looks utterly ridiculous because I never bothered making it look good; the Blocks are from a Tilemap, the Pickaxe and Sword I drew in Microsoft Paint, and the pictures are some images of D&D characters I had on my Desktop at the time. The Sword doesn't even look that bad, though.

The smallest piece of selecting the Active Item is with SelectSlotIndex. It doesn't matter if you click, scroll or press a hotkey (which doesn't exist in my code), all it does is choose a number. This then triggers the UpdateActiveItemChain, which looks if there's something in the CursorSlot, then checks for the ActiveItem and triggers all the necessary updates.

Although my system is basically a direct copy of Terraria, one thing it does differently is that, if the Inventory is open, you can actually scroll through the whole thing, rather than just the Hotbar. I still think that's a pretty cool usability feature. In Terraria, I often played with the Inventory open while I was building a structure and doing a lot of back-and-forth mouse movement whe needing to grab something that's not on the Hotbar. With this structure, you can just keep your mouse where it is and scroll, instead!

<!-- Gif Here -->

### Equipment

Another special type of slot is the Equipment Slot. I didn't get very far with tinkering with it, but an Equipment Slot is simply an Inventory Slot, with logic to prevent placing wrong items in it, like a Block in a Head Slot. Anything equipped in such Slots would pass on their stats to PlayerStats, like equipping Armor and getting extra Defense or Hit Points, etc. Since the 'Iron Pickaxe' item has the 'Shield' type, this is probably something I was still tinkering with, but it works in that it prevents placing the item in any of the Equipment Slots where it doesn't belong.

<!-- Gif Here -->

## Conclusion

Overall, I think I was on the right track with this script. It works as expected, including in Multiplayer settings and re-uses the same helper functions everywhere. Even still, it's a 1000-line script, so there's room for improvement there. I also have a spot where MergeIntoCursorSlot instantiates a new InventorySlot. Likewise, RemoveItemFromInventory doesn't actually use SetSlot, which might be questionable design. What's neat about working with Inventory is that it's quite easy to reason about. It's probably one of the things that you can make and immediately feel like it belongs in a game. It looks completely ugly, of course, but the way it works is quickly familiar and intuitive.

### Bonus

After breaking a block, it of course needs to be able to go into the inventory. The way that usually works is that the block first drops to the ground, before it gets picked up. That part works in this version, though... The items on the ground were supposed to be affected by gravity, drawn to nearby players and able to be picked up. I broke that part somewhere a little too far back to find again. That's what happens when you leave things in half-broken states, don't properly keep track of what you're doing, and look back at code you wrote months ago.

<!-- Gif Here -->
