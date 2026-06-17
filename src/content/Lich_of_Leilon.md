# The Lich of Leilon

Strictly speaking, I lied. This isn't a _Github_ Readme, but a .md file in this project, so I can keep the Github repo private. Sue me!

## Obsidian

I've quickly grown fond of Obsidian for organizing notes. For keeping track of a D&D world, it's easily the best way I've found to do things and it's amazingly powerful. It uses plain .md files, stored locally, which it styles up nicely with .css, and you can just put the whole thing on Github to get source control, and control your data.

## Quartz

For putting it online, there's a couple of options. Quartz is both the free and probably the best option. Credit where it's due, Jacky Zhao really managed to make a super easy way to take your Obsidian Vault and 'just' put it online. The config allows for choosing which Folders and Files to display. I probably spent as much time re-organizing my Vault as I did setting this up. Or.. I would have, if it wasn't for ...

## My Nemesis

When Deploying with Vercel, I found a bug I couldn't solve.

✘ [ERROR] No matching export in ".quartz/plugins/index.ts" for import "CustomOgImagesEmitterName"
quartz/components/Head.tsx:7:9:
7 │ import { CustomOgImagesEmitterName } from "../../.quartz/plugins"
╵ ~~~~~~~~~~~~~~~~~~~~~~~~~
Failed to build Quartz. Check for syntax errors in your configuration or plugins.
Reason: Build failed with 1 error:
quartz/components/Head.tsx:7:9: ERROR: No matching export in ".quartz/plugins/index.ts" for import "CustomOgImagesEmitterName"
Error: Command "npx quartz plugin install && npx quartz build" exited with 1

I spent way too much time on trying to figure out the right fix. Long story short, eventually, I realized that the first build did, at least, Deploy, so I Redeployed that one, which Deployed again. I didn't immediately realize it was always going to remain stuck on that Redeployed version while I was tinkering with the code, so that didn't help the investigation. When I Redeployed another fix attempt later, it struck me that nothing in the actual code or config was the solution; the fix was the _actual Redeploying_. After updating the linked repo, Vercel's deploy always fails, but if you then Redeploy it straight away, it works. So, I tossed out the garbled mess that was the original repo at that point, made a new one, did all the basic steps again, deployed, changed the settings I wanted, redeployed, and done.

What's actually 'causing' that stupid bug, though, I don't know. Sometimes you just roll with it.
