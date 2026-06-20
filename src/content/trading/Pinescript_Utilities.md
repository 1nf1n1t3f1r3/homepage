# Pinescript Utilities

[💻 View Scripts on GitHub](https://github.com/1nf1n1t3f1r3/Pinescript_Utilities)

A collection of Pinescript Utility Scripts

## ATR Stop

This is a little tool that constantly displays the ATR distance from the lows. You can use this to set a stop without having to do the measuring yourself. The lines are a bit distracting, so toggling them off and on again when setting a trade is the way I use it.

## ATR Trailer Tightener

This is similar to the ATR stop, except that it requires manually setting the entry date. Then, form that day, it'll tighten the ATR Stop until it gets triggered by an interaction with a bar.

## Datestamp

A datestamp I adapted from another trader. I put it in the middle and cut out some of his fluff. It displays the Ticker, Date and Timeframe. helpful when backtesting and you want to take a picture of the chart.

## Earnings

You might already know this one... But it marks Earnings on the chart, going further back then Tradingview does by default. You can tell that the first 2 labels don't have the green [E] at the bottom of the panel

## MAC Spikes

This is something Adam H Grimes came up with. It detects spikes of volatility in a ticker. This is a good way to compare apples to apples

## MACD-V (AHG)

This is a modified MACD that uses Adam H Grimes' settings, and also changes the bounds of the MACD to be relative, rather than absolute. This way it's much more helpful to compare across tickers. I also added the little grey line that slowly descend from the highs; they indicate when the MACD makes a new relative high.

## Pivot Entry Detector

A tool to display potential entry points. The white line indicates where you might enter a trade, with a Stop and Target on the orange/blue lines. It's not intended to enter wherever they're drawn on the chart, but to be used as a consistent entry technique at the tail-end of some strategy.

## Sneaky Python Bonus: PED in Python

If you can write Pine.. Why stop there? Pine's limited to the Tradingview environment, but Python's much better for large-scale data analysis. Simply feed it a .csv file with dates you want to check and it'll spill out the date when this PED finds a signal. In this version I also cleaned up some of the repetetive code of the earlier Pinescript version.

## Example

![Pivot Entry Detector](/images/Pinescript_Utilities_BA.png)
All the tools are on this chart.

At the top right, the datestamp indicates the Ticker, Timeframe and Date we're looking at.

The red line in the middle is the Trailing ATR. It stops displaying when the line gets pierced by a candle.
The faint orange bands is the ATR stop. Notice how it's on the exact same level as the Trail.

All the little dashes in white cyan and orange indicate potential entry points, with accompanying Stops and Targets, according to the Pivot Entry Detector. Since the detector doesn't look at anything else, they show up everywhere, and entering them at every possibility would be a little insane.

The blue labels are the Earnings. Tradingview displays Earnings Events with the [E] Label at the bottom of the chart. Since we're looking at old data, Tradingview doesn't bother displaying them anymore if we go too far back, but the label still does. We're looking at the exact cutoff in the middle; 2 labels on the right are marked by Tradingview, the ones on the left are not.

The first panel below the main chart contains the MAC Spikes. They show big movements in price on a glance. The biggest spikes on the MS tend to correlate with some of the biggest candles.

The final panel is the MACD-V. The axis of this MACD is bounded from 200 to -200, which isn't the standard for MACD indicators. This makes it more useful in being able to compare across tickers. I also included the Green/Red background when it makes a new relative high on the MACD, though it comes at the cost of makign the indicator look a bit 'busy'.
