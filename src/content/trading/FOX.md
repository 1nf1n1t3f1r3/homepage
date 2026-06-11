# FOX

It's a commonly said that a strategy doesn't need to be complicated to be good. This one certainly isn't complicated and seemed good. That made it a prime candidate for turning into code and doing a test myself. In the video where I saw it, the presenter does everything manually. It's a good way to do it, but if you want data, code's the answer.

It's a quite little script. I made some changes to it from the original to try some other things out as well. It still works the same, and all my tinkering can be toggled On/Off.

## Description:

This is a Modified & Codified Strategy from Youtube by TradePro:
youtube.com/watch?v=7NM7bR2mL7U

It uses 3 EMAs and a Stoch RSI Crossover. The EMAs need to be in alignment: FastEMA > MediumEMA > SlowEMA. Additionally, there needs to be a Crossover of the Stochastic Fast Line.

The settings of my version are slightly different from the original. I'll describe the original first:
EMAS: 8, 14 and 50
Stochastic RSI: Tradingview Default
ATR: Tradingview Default
Risk Reward: 1.5/1 (Meaning Stop is larger than the Profit Target)

My version uses 10, 20 and 50 EMAs, with a 10 Length ATR and different R/R settings. The code also stops unrealistic trades that would be taken on Gaps. Finally, it has the option to use a simple Candle Pullback Structure.

When using the Candle Pullback Structure, rhe code still uses the Close by default as a conservative entry. This makes the strategy not too impressive. Toggling it to using the Intra-Bar entry looks much better. It's likely to me that entering there with the PB Structure would be feasible, but it introduces some lookahead bias in the test, which can't be accounted for. Using the Close as default seems like the more honest default, but I'd recommend checking out the Intra-Bar approach.

It uses Zen's table for data display and allanster's Date Settings.

![FOX Pinescript](/images/FOX_AAPL.png)
