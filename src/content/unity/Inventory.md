### Inventory

At that point, it's time to start thinking about things like breaking blocks. The way something like that works in Terraria is that your main 'Use' action (LMB) changes depending on what item you're holding. Thus, your LMB needs to inherit from the item. Swaping which Item is 'Active' changes the 'Use' action. Setting up the ItemDatabase and the Inventory system was relatively straight-forward (at first). The ItemDatabase is a relatively simple .JSON file containing all the raw data. The Inventory system then gets the item by the ID. Every individual Item is created by Inheritance, Item.CS (ID, Sprite, etc.) -> ItemWeapon.CS (Attack Power/Speed etc.) or ItemBlock.CS (Durability, Placability, etc.) The PlayerStats script refreshes whenever the inventory changes and updates based on what's currently equipped or held, like miningSpeed for a Pickaxe amd meleeAttackDamage for a Sword.

<!-- Link to Inventory -->

After breaking a block, it of course needs to be able to go into the inventory. The way that usually works is that the block first drops to the ground, so I needed a DroppedItem script, so things could fall to the ground, and then they'll get attracted, like a Magnet, to any Players that are closeby.
