# Band Breakout Strategies

[💻 View Full Code on GitHub](https://github.com/1nf1n1t3f1r3/Minirepo_Investing_Strategies)

This is more of an Investing Strategy, than a Trading Strategy. This makes it function a lot differently from a more active trading strategy, where you're expecting to jump in and out of positions regularly. This approach is a lot less stressful by comparison. When looking at this timeframe, we're mostly concerned with beating the 'Buy and Hold' approach. This is actually not that simple. The reason why Buy and Hold is hard to beat is because it's always in the market and, statistically, markets go up. So, when you're not in the market, because you're using a stricter strategy, you're potentially missing out on a period in which the market would be statistically likely to go up. Of course, the market doesn't always do that, and not every stock in the market goes up equally, which is what gives room for outperformance..

## Band Versions

In the Repo, there's two versions of the script. One uses Bollingers and one uses Keltners. Bollingers tend to react faster to volatility, meaning their bands grow and shrink a lot faster than the Keltners do. The premise is the same along both versions: by buying something that's breaking out of its bands, we're attempting to hitch a ride with a stock that's experiencing strong upward momentum, hopefully outperforming the wider market. We trail a stop by closing the position when it dips below its lower band; we're not interested in holding onto losers!

Additionally, both versions also have a Monte Carlo version to do some extra statistical analysis. More on that below.

### fetch_stock_data

This is my behemoth function that fetches stock data. It stores Yahoo Finance data locally on my system to reduce API calls. The script would still work with a humble yf.download call, of course!

### moving_average and bolling_bands/keltner_channels

These are relatively simple Technical Analysis tools. We'd get them for free on Tradingview, but alas this is Python, so we have to write them ourselves.

### get_combined_data

This is the function that calls fetch_stock_data and calculates the indicators, generating a nice full dataframe for us to work with.

### process_trades_day_by_day

This is the main function keeping track of our trades. If our bar closes above the bands, we go long. That is, if we don't already have a trade, and we still have room in our max_concurrent_trades to do so. The system keeps track of where and when we bought and goes day by day until it finds an exit condition.

### calculate_performance_metrics

For active trading strategies, I think looking at R is the most useful thing. However, for this longer timeframe, that's not ideal. Instead, we're looking at metrics like CAGR, MAR, Max Drawdown, Sharpe, etc. In short, these look at how much money the system would've made, how much it loses, and what its worst losing streak looks like, in order to decide if it's worth the risk of using this strategy.

### process_input_file

This is simply the main runner, calling each process in order.

## Monte Carlo

There's multiple ways to do a Monte Carlo test. The standard way is shuffling the order, but here I added a 20% chance to simply skip any individual trade. Since there'll always be more trade signals than trades we can actively take (our funds aren't infinite, that's why we're here, after all), that means that each Monte Carlo run is going to be very different. By running it a large number of times, then comparing them against each other, we get some certainty that our results aren't because of extraordinary good/bad luck.

![Band Breakout Strategy](/images/BBO_DGX.png)
