# Multiplayer

[💻 View Scripts on GitHub](https://github.com/1nf1n1t3f1r3/Multiplayer-Minirepo)

So this is the real swamp. Here's where I suddenly found out that there's weird/unexpected reasons for stuff not to work. I found things like, controlling multiple characters, sharing an inventory and not being able to control anything at all. But, that's just one part of it.

What's way worse is that the Physics wound up play out just slightly differently in Multiplayer. Networking conditions like lag and packet loss introduce just that slight amount of 'Jank' that permeates through other systems, which forced me to go back to change Physics that I thought worked perfectly fine before. That's where a lot of the aggressive Raycasting and Overlap fixing comes from in the Physics script!

Anyway... Let's start at the beginning.

## Multiplayer Paradox

Just sending data over the network isn't actually especially difficult, as Mirror already handles a lot of that. The difficulty is the delay between sending and reeiving the data. Networking introduces a delay between what a player does, when the server receives it and when the server sends back confirmation. If the client waits for confirmation before moving, the controls feel terrible. If the client doesn't wait, then the client and server will disagree about reality. This is essentially an unavoidable and unfixable paradox.

One thing I kept discovering was that every fix solved one problem while creating another. Hard corrections were accurate, but looked terrible. Soft corrections looked better, but could take too long to fix a bad prediction. Larger interpolation buffers hid jitter, but increased visible latency. Smaller buffers felt responsive, but exposed more network instability. Almost every networking decision felt like trading one form of jank for another.

Although they're not the only pieces, TickClient and TickServer demonstrate it the best, so I'm focusing on those for this write-up. And, even then, it'll stay relatively high-level.

### Ticks

In order to be able to have a shared framework to reason about, the server and the client first need a timeline to agree on. That's where 'Ticks' come in. In this system, there's 60 ticks in a second and both the client and the server keep track of which tick they're on separately. And, they'll store what the state of their own (client) Entity, or the state of every Entity (server) is via Snapshots. They both do it, because it's (practically) impossible to keep everything perfectly in sync; small discrepancies always find their way in even if you run the exact same logic. If you store the data, however, you can at least keep track of it and step in when it gets too out of sync.

Thus, in short, the server and client are constantly running the same code and talking to each other about what they think is actually happening. What's happening on the server's side is authoritative, so, if the client is out of sync, it'll get corrected. Basically all the code in TickServer is about the server registering its clients, running its own tick simulation, simulating all entities and informing the client of these entities, and its own ticks status

Similarly a few functions in TickClient are concerned with just keeping track of the connection statues. There's a 'Heartbeat' that's always thumping in CheckHeartbeatFromClientTimer and CmdRequestHeartbeat, and the server's TargetRespondToHeartbeat + CmdReturnHeartbeatResponse. Together, they keep track of when it's sent, and when it's confirmed again. The time that takes is the Round Trip Time (RTT), and the Half Trip Time (HTT) is RTT/2. This value is what's used to estimate when something the client does actually happens on the server. Since networking never stays fully consistent (it might be 'Jittery'), it keeps a running average, rather than a flat value, which get stored by TargetReceiveConnectionStatus, and used to update the predictions in UpdatePredictionDelay

### Local Prediction and Reconciliation

The idea is relatively simple, but making it work isn't. Every time the client/server attempt to communicate, there's some micro-seconds between them. For example, say that you're on a stable internet connection, but you're playing from the Europe, connecting to the United States. The time it takes for something you do to be sent to the server is 1 second. Likewise, the server takes 1 second to inform you of anything. You press 'Jump'. How long does it take for your character to actually jump?

It's a bit of a trick question, of course. You could say that it takes 1 second; that's how long it takes for the input to travel to the server to execute. However, since it takes the server another second to send that information back, it won't happen on your screen until 2 seconds have passed. When you're playing with 2 seconds of delay, a game goes into the 'Literally Unplayable' kind of territory pretty fast.

Another answer is that it takes 0 seconds, because we're predicting the movement locally. Now we're talking. Rather than waiting for the server, the client takes the reins into its own hands and just moves on its own end, 'ahead of time', as it handles ticks in UpdateClientTick. The player thinks the game's super responsive and feels smooth, buuut... It's of course not looking at what's really happening. If you appear to be 2 seconds ahead of where you actually were, you're liable to find a lot of situations where a player thinks they dodged out of the way of something, because that's how it looks on their screen, but the server will snap them back to reality and tell them they were, in fact, hit.

So, instead of prediction acting immediately, I'm more inclined to say that the right balance is to let the client predict 1 second ahead, rather than 2. So, when pressing 'Jump', it'll take 1 second to process on the client's screen, because that's the time it takes for the info to get to the Server, but then we don't wait for the Server to send it back. That way, what's visible on the client's screen should line up exactly with what's happening on the server.

But then there's another problem. Since we're constantly sending ticks and snapshots and attempting to reconcile them, if the client's helpfully anticipating the server's delay, the server's still going to fix it because... The client is one second out of sync from the server's perspective. ReceiveLocalSnapshot will send a snapshot from the server, it'll then check it with CheckLocalSnapshot, see that it's incorrect and fix it, even though we're trying to take manual control here. Ugh!

### Correction System

So, in order to see what's going on there, we need to look at CheckLocalSnapshots. The Client receives a constant stream of snapshots from the server and processes them here. It basically only looks at the tick and the position. Then it checks how different the position is across snapshots and starts looking for a fix if necessary.

With the client-side prediction that we're doing, we have to change the comparison to prevent this from firing every time the client does something. A 1 second discrepancy is a lot! So, rather than comparing the client's tick Nr. 120 with the server's tick Nr. 120, we compare it with the same 1 second (60 ticks) leeway that we know is between the client and server. And, for good measure, since the 1 second delay isn't always exactly 1 second, we add a little margin of error, as well. If it can't compare 60 with 120, it might compare 59 to 123, or something along those lines. Furthermore, since the snapshots are a continuous stream, it doesn't fire based on any single mismatch, but requires multiple mismatches before taking any action.

Basically, a lot of the code here is in trying to get the correction system to fire less. And, when it does, it'll prefer to do a 'SoftCorrect' rather than a 'HardCorrect. A HardCorrect is when the Server overwrites what the client sees with (its) truth. This leads to a visible snap. A SoftCorrect is an attempt to slightly nudge the discrepancy so it's less bad. For example, if the client thinks its on coords (10, 0) and the server thinks it's on (15, 0), the logic might decide to helpfully nudge it to (11 , 0). The reason is that this is less noticeable than the hard snap to (15, 0).

### Ghosts

Thus far, I've only written about the client-side prediction. For making other entities, like AI characters or other player characters, look smooth, there's a spearate kind of 'Ghost' logic. I was going to write more about them... but this post's already more than long enough.

## Conclusion

It's hard to write exactly the kind of struggle this was. It quickly starts to look/feel rather detached; even looking back, knowing that I did indeed struggle a _lot_. If I spent 2 weeks or so on the Inventontory and Physics, I spent 2 months on the Multiplayer. When handling networking, everything gets tangled up like a bag of snakes. Fix Predictions -> Constant Corrections. Reduce Corrections? -> Players Drift. Increase Interpolation? -> Delayed Movement. There's not really any clear 'solution', just different trade-offs. Of course, there are _some_ things you can do to make it almost entirely better, but most if it is tweaking, loading, testing a change, seeing no improvement (overall), and trying again.

The real mistake here is spending 2 months on this rather than just taking the Mirror/Fishnet default NetworkTransform and letting that do it's thing as a 'Good enough' solution. Because, frankly, it is good enough, and certainly a better fit for the state the project was in (remember that Minimum Viable Product idea?... Yeah, me neither) than anything I made. Unlike the Inventory and the Physics, this is something I'd be more inclined to completely toss out if I were to restart.
