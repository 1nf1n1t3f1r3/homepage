# Blackrider.Quest

Strictly speaking, I lied. This isn't a _Github_ Readme, but a .md file in this project, so I can keep the Github repo private.

## Flip-a-Coin

I'll admit, I way overestimated this project. I was mulling over if I wanted to do a Character Builder or a Loot Generator. I intended both to be pretty quick projects. The Character Builder is because I love doing some theorycrafting for new Player Characters with different builds (which I'm not sure I'll ever get to play because I'm the DM), and D&D makes it rather difficult to do that in one place; honestly launching Baldur's Gate III is the most convenient way to do it, which isn't very convenient at all. The Loot Generator idea is something I wanted because I never really know what kind of stuff to hand out to my players. Thus, I flipped a coin and it landed on the Character Builder.

## Character Builder

Initially, I didn't expect this to take as long as it did. It wound up being more than a month, but the groundworks I basically managed in a single weekend. I created a quick flow to fill in your Name, Alignment, Species, Class, Level, Origin Feats, Skills and Spells. But then I realized... Even though it's super easy to click through all of it, it's kind of pointless if nothing actually contains the data saying what everything does, so I had to start filling that in. At first I'd hoped to keep it minimal, but that frankly just doesn't work; you have to do everything, which is a lot. A lot of data-wrangling and edge cases later, though, I do think this thing is better than what other character builders have on offer.

###

Everything the User clicks gets stored in CharacterData, which then pulls from the /Data folder that stores all the raw .JS files containing the data the user sees on screen. It winds up with a pretty hefty .JS object which then updates on a separate 'Sheet' section that can be turned into a PDF, and printed. What I'm quite pleased with is that almost everything the user could need is readable on screen, without having to go look up elsewhere what it does, or where it came from, while still striking a decent balance between clarity and visual bloat. If the user wants, most things are overridable with the Sandbox mode. The PDF itself isn't editable, unfortunately. It's a design choice I made with the type of PDF, which is essentially HTML + CSS being wrangled on A4 Format, then converted to a PDF. There might be an arument for changing it, but Pros and Cons. At the end of the day, the finished sheet is, in my humble opinion, super easy to read, in contrast to what you can find in some other character builders!
