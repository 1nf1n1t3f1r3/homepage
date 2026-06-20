# Post-Mortem... For now

While I did spend a lot of time with Unity, I never made something that actually wound up looking like a game at all. What I wound up with instead were a couple of (mostly disparate) systems that never really came together, backed up by a _lot_ of ideas about Stories, Settings, Characters, etc., but I'll keep those to myself for the time being and focus on the code and the systems. There's a couple reasons why the it didn't quite start to connect into a full game.

## Let's see how Far I'll get

First, I did realize this was going to be a massive undertaking, and I had little reason to assume that I would actually be able to pull it off. I was going in with more of a 'Let's see how far I'll get' mentality. And, in order to work that way, I decided to start with some basics, but then pivot into the more difficult areas first. My reasoning for this was that, I didn't want to build something, then realize I did it a 'lazy way', and then had to refactor it. Or, even worse realize that I had put off difficult work that I wasn't going to be able to finish, making the work I had done up until that point moot. It was kind of a 'defensive' approach, in that, if I was going to bang my head against the wall, I'd at least get it over with quickly.

Of course, it didn't really pan out that way. The Good/Bad thing about writing code is that, actually, you almost always _can_ finish something. If you're stuck, you can tear something down and rebuild it better. Dive into the docs, make a new plan, finally notice the bug you've been looking straight past for weeks, etc. So, the problem's rarely 'I can't build this because I don't know what I'm doing' (even if that's also true), it's actually 'I can't build this because I don't have the time/energy to figure it out'. And that entirely changes how and when to decide to call it!

## What to Build

So, one of the first jokes of the story is what I wanted to build. One of my favourite games is Terraria, which was released in 2011. It's been a minute, and Re-logic, its developer still hasn't made a sequel, even though the game's still incredibly good. There have been similar games, like Starbound or Dig or Die, but they didn't quite manage to reach the same heights. Now, if you know Terraria, you might wonder what business I'd have to make a Terraria-like, and you'd be absolutely correct to wonder about that. The arguments I had in mind is that it's a 2D Platformer, which is arguably a lot simpler than 3D, and a block-based system has the potential to be relatively straight-forward. Neither are realistic graphics expected in the genre. My thinking was that it might be able to get pretty far based on building good controls and digging stuff.

So, when building, a (good?) advice is to start small, and make a Minimum Viable Product (MVP), then take it from there. The idea is that you create something which didn't require spending years on, and get a good 'feel' for how it plays. If it feels bad, throw it out quickly and go do something else. If it does feel good, that's when you go the extra mile on it. At first, you might be using literal cubes or circles. So, if you're making a 2D Platformer, only when that cube moves around in a way that feels legit, you make it into a character (or don't, and you get Super Meat Boy).

In a way, it does make a lot of sense. Arguably, Terraria itself works that way. If you look at where it started in 2011 and compare it to where it's wound up 10+ years later, the difference is night and day. However, if you're there figuring everything out for the first time, it doesn't work as well. Or, perhaps, it's on my penchant for anticipating all the future problems and overengineering.

## The Plan

Based on the MPV theory of keeping everything ugly, combined with my idea of tackling the hardest things first, I came up with the rough action plan I put in the code block on the right ->

I actually did make it a partly through the list. In Part 2, I didn't include a Chunk Loading system or Terrain Generation, but other than that I made it all the way through to point 5. One funny thing about the list, in my opinion, is that it doesn't include anything about, Art, Animation, Sound Effects or Soundtracks. As if any such things would be easy to take care of. Arguably, this wound up being that actual wall I slammed into. Or rather, working with that that was when my last bit of patience for it ran out.

## Takeaways

It's funny in hindsight because... _Of Course_ you're going to lose patience with it if you overdesign one system, then overdesign another, then have to go back to the first because the second messes with the first. The thing that's most fun about writing code is being able to write something and immediately watch it work. Like building a component in React, then being able to use it in multiple places while updating in one, or creating an enemy type and then spawning 100 of them and suddenly you're playing a bullet hell. It's almost as if the MVP people were really onto something and I shouldn't have overengineered everything without having anything worth overengineering _for_.

Despite all that, I don't think I'm permanently done with game dev. The problem with it is that it basically forces a huge investment of Time/Resources before anything comes out of it. I've some ideas fow how to manage that for any next time, but that's for the future.

In the meantime, there's some things I built that didn't wind up too bad. I'll share some of those in the other stores of this section.
