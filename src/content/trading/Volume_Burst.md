# Volume Burst

[💻 View Scripts on GitHub](https://github.com/1nf1n1t3f1r3/Volume_Burst_Minirepo)

This Repo doesn't contain the proper Python environment to run the script on its own, but it shows the Pine and Python scripts side by side.

So, this is a relatively simple idea I had, which turns a little complex when you try turn it into Python. It makes it a good candidate for demonstration, though. When testing out a new idea, it helps to write it in Pinescript. You get immediate feedback when you save the script, and can visually determine if it looks like it makes sense at all. You can spend 15 minutes deciding exactly what parameters to use, until it looks right on one chart, then swap to another chart and realize that you just managed to beautifully overfit on one example.

Volume Burst is pretty simple, so it doesn't suffer from that overfitting problem as much. There's two pieces. The first is on the daily chart. It very simply takes an Average Volume, then checks a bar's volume against that average. If it's larger than some ratio, it'll get classified as a 'Burst'. The idea is that it finds a bar where 'something happens' to put us on alert.

The second piece is on intraday chart. After finding the Burst, we still need to do something with it. The simplest approach when finding an entry signal of some kind, is to trade it on the same timeframe as the one you found it on. That's explicitly not the case here. After finding a Burst on the daily, we drill down to an intraday timeframe (5 Minutes) and get ready to watch it on the open.

The intraday logic is slightly more complex, but only slightly. The gist of it is that we enter a position very aggressively, simply entering over a previous High (for Longs (I.E. buying)). However, we only do that when the price is actually far away from the Exponential Moving Average (EMA). That way, we're making sure that the Volume Burst from yesterday actually led to a jump in prices, either yesterday, or overnight through what we call a gap. That indicates there's momentum we can join (the Python version has slightly more advanced tools to attempt to reach the same effect).We also gate the amount of time we wait for an entry. If it doesn't appear soon enough, there's no momentum out of the open, so we stop looking for a trade.

The rest of the script is stop management. It works by setting a Stop based on the Average True Range (ATR), and mirroring the distance from the Entry to set a Profit Target. There's also a Trailing Stop mechanic that can be tinkered with. It'll trail the price action as it moves in the trade's favour and logs the results.

There's slight differences in the Pine and Python versions I'm sharing here. The Pine is simpler, both in how it works and how to get it running.

## Pine to Py

Now this is where it gets interesting. Pinescript is, of course, limited to Tradingview. You can do analysis with it on a chart, and it's the best way to visually see what you're doing. In a way, Pine might be my favourite language just because it's such a breeze to use. However, if you want to analyze how it works on multiple tickers, you have to go ticker by ticker. There's ways to do that, but they're all 'hacky', like downloading the Strategy Report for each ticker, then combining them. It's better, in my opinion, to use Python instead.

Just based on the size of the script, it's clear Python is a lot more work. It requires setting up an environment and getting the data from elsewhere. Additionally, trading indicators that Tradingview understands natively have to be recreated when working with Python. Some technical analysis libraries exist for Python, but I prefer just writing it out myself.

The most challenging aspect of the project was translating a multi-timeframe TradingView workflow into Python. Daily bars are used exclusively for signal generation, while trade execution occurs on 5-minute data during the following session. This means keeping track of multiple dataframes. The way I achieved it was by writing the daily section first in a way that could run on its own. Then, the intraday section, which takes that output file as its input. After I had two separate Python scripts, I bolted them together. Doing so does unlock a the possibility to tighten the intraday logic with daily data, that doesn't exist on the Pinescript version.

When working with Python, the biggest challenge is wrangling the data-frames. Looking at the script, most of it is hardly even about trading, but about fetching, modifying/calculating and storing data.

### Daily Section

#### fetch_stock_data

A large chunk of the script is dedicated to fetching data from YFinance, the unofficial Yahoo Finance API. Since I've used it so much, I decided to cache my data locally. That way, I didn't have to send repeated API requests to YF. I've tinkered with this fetch stock data function quite a lot over different projects. It started as a simple YF-wrapper and has grown into something that smartly maintains my own data cache, filling in gaps and taking account of potential changes in ticker names by referencing the metadata. I never made a central storage for it, with an import in different scripts. It's a little annoying when look back on my scripts and find I used a previous version of it. Still, it works like a charm. For somebody that doesnt want/need to fill up their hard drive with financial data, it would work just fine with a modest 'yf.download' call rather than this behemoth function.

#### compute_ema & compute_atr_rma

This is where the script manually calculates technical analysis tools, like an EMA and the ATR.

#### daily_signal_generator & volume_burst_finder

This is the part handling the daily section of the strategy. It takes the simple Volume Ratio to determine if there's been a spike. Compared to the Pine version, it also has extra features to improve the signals. These can work because we're now combining the daily and intraday signals in one function, meaning we can look at the daily data and see if there's something there that should stop us from taking the trade the following day. The liquidity_filter makes sure we're not trading something that's 'dead in the water'. The high_low_filter helps make sure we're trading in the same direction (for example, if the candle that is a 'Burst' on the daily is a Green candle, we only go Long), atr_range_filter, which helps see if the candle includes large price movement. The clv_filter which checks where the candle closed relative to its body. For a green candle, if the Close is near the High, that indicates conviction, which is what we're interested in. If the Close is far away from the High, it indicates a lack thereof. The atr_range_filter doesn't check this on its own. With those it does some dataframe wrangling to create a clean output dataframe which'll get passed along to the intraday section.

## Intraday Section

#### fetch_5m_data_localcache

This is another part of data fetching. Again, using a simple yf.download call can work, but only as long as you're not going too far back. Intraday data is hard to come by, which is a good reason for storing it locally.

#### build_indicator_dataframe & get_next_trading_day_from_df

Since we're going to be taking the entry on the intraday, we'll need a full dataframe here for analysis. We add the EMA, the ATR and the Rolling_Low + Rolling_High (which we use for setting Stops) to the 5-minute Dataframe.
get_next_trading_day_from_df is what we use to select the day that counts as the 'next' following a burst. In other words, this is the day we're interested in finding a trade.

#### detect_breakout_entries & backtest_entry_inplace

These are the main engine for the strategy. detect_breakout_entries finds the entry location, taking account of all the filters set in the daily section, and sets the Entry, Stop and Target prices. backtest_entry_inplace plays them out. The logic there is essentially related to trailing the stop and calculating the Profit/Loss in R, a standardized metric.

#### backtest_signals

This functions runs all of the intraday stuff in order, fetching the data, the correct trading day, and playing them out.

### Main Runner

This runs the daily_signal_generator, which finds the Burst on the daily timeframe, then passes those signals into backtest_signals to handle the intraday execution logic.

![Volume Burst](/images/VB_AAPL.png)
Just barely didn't reach the 1/1 Profit Target on this hypothetical AAPL Earnings trade example. But, the Trailing Stop protects a chunk of the profits anyway.
