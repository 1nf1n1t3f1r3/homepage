# Data Analysis

[💻 View Full Source Code on GitHub](https://github.com/1nf1n1t3f1r3/Data_Analysis)

A repo containing statistical Data Analysis work. I was working on an automated script, which has a lot of parameters, like a setting for the ATR, the Risk/Reward Ratio, whether to use a Trailing Stop or not, etc. So, I could run all of these and then see how well they did. Some would do better than others, but even with just a handful of parameters, the possible combinations absolutely explode. Looking through these in a .csv file manually gets very hard to read. And, the main goal here is to discern a pattern. If one result is very good, say an ATR of 1 and a R/R of 1, then we'd expect an adjacent result, like an ATR of 1.1 and a R/R of 1.1 to also be good. If it's not, then that makes the result suspicious. Displaying the data visually works a lot better to discern that.

The notable libraries here are KFold and Matplotlib. KFold splits the data into folds, on which it tries to optimize, before putting them back together. It's a little step that tries to guard against randomness, as far as that's possible. Matplotlib is the library responsible for turning them into pretty pictures for visual inspection.

The script on the right is the one that shows the 3D Plot. There's a few others you can find in the Repo, or you can scroll down to see their results.

---

## Parameters

The scripts analyze these parameters:

- Base ATR Multiplier
- RR Ratio
- ATR Trail Decay
- Use Trailing Stop
- Close Upon Red Entry
- Use Close as Trailing Stop
- Use Recent Structure for Trailing Stop

The parameters here are about deciding which way to manage a Stop Loss. When entering a trade you should always set a Stop Loss, which closes your position when the trade doesn't play out the way you want. Setting a Profit Target is optional, but common. If you set both, you'll automatically be taken out of the trade, at some point in the future, either at a win or a loss. There's a number of ways to handle them. I like to set the Stop based on the ATR (Average True Range), which is a distance that accounts for the way the ticker moves. I then mirror the distance for the Profit Target, and multiply that value (the RR Ratio). A Trailing Stop entails moving the Stop, meaning you move it in the direction of the trade. If you're Long (you bought), you only move the Stop up, never down. This way, your Stop will always get hit eventually, taking you out of the trade. If all goes well, it will have tightly trailed the price going up (assuming we're still Long), and you capture a large part of the move, without giving a lot back when it comes down to hit that Trailing Stop. The ATR Trail Decay is how tightly we trail that stop, following the price action.

The last values are On/Off switches. Some people prefer waiting until the Close before doing anything, rather than leaving an active Stop Loss order. I don't really think it's worth the hassle. Close Upon Red Entry is about closing the position when you enter and immediately have the market move against you; happens sometimes. A common advice is to exit, but I didn't find that actually helps. The problem with it is that you're already in the red, anyway, so you just wind up locking it in. The final section is using Recent Structure for the Trailing Stop, which means anchoring it to the candles to respect the market geometry. It doesn't appear as relevant as I thought, but still makes the most sense to me.

## Script versions

All of these are mostly a version of the same thing, but they display slightly differently. One displays a 3d plot, another a barchart, another a graph. The winners-losers one and the trail=false are mostly there to focus on a slice of the data. winners-losers is interesting, because they tend to trade differently. Losses tend to 'hit' harder.

## Conclusions

We're aiming for the highest Mean Average R-Multiple (the average gain per trade), with the lowest Standard Deviation (most consistent results).

### 3D Plot

The 3D Plot is, in my opinion, the most informative. It quickly informs us about three variables, RR Ratio and Base ATR Multiplier by the axes and the ATR Trail decay by the colour of the dots. The 0.5 and 1.0 ATR Multiplier score best. RR Ratio doesn't matter as much, but it favours the higher values. The ATR Trail Decay, indicated by the colour of the dots, shows that the lower value generally scores better.

Also, you can turn those plots around. How cool is that? I mean... Not on the website... These are just .png files, but you get the idea.

![SEC Scraper Spreadsheet Output](/images/3d_Figure_1.png)
![SEC Scraper Spreadsheet Output](/images/3d_Figure_2.png)

---

### Bar Chart

Here's some images of the bar charts output, showing the 0.5 and 1.0 Base ATR Multiplier do much better. The sharp drop to their sides is a little suspicious... However, it also shows the Standard Deviation of using 1 Base ATR Multiplier is lower than using 0.5, which works in its favour.

![SEC Scraper Spreadsheet Output](/images/Bar_Figure_1.png)
![SEC Scraper Spreadsheet Output](/images/Bar_Figure_2.png)

### Line Chart

Finally the line charts, giving their opinion on the various On/Off toggles. Most notable to me is the idea of Close upon Red Entry being notably worse here.

---

![SEC Scraper Spreadsheet Output](/images/Line_1.png)
![SEC Scraper Spreadsheet Output](/images/Line_2.png)
