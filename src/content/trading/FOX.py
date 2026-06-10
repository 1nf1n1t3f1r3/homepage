// This source code is subject to the terms of the Mozilla Public License 2.0 at https://mozilla.org/MPL/2.0/
// © Comatus

// Codified and Modified from YouTube:
// Author: 'TradePro'
// Title:  '76% Win Rate Highly Profitable Trading Strategy Proven 100 Trades - 3 EMA + Stochastic RSI + ATR' 
// Link:    https://www.youtube.com/watch?v=7NM7bR2mL7U

// Strategy consists of 3 EMAs which need to be in alignment.
// The StochRSI cross gives the entry signal
// SL at 3 ATR. PT at 2 ATR
// Enter at close of bar

// Changed entry and SL PT rules for personal use

// Trade Pro's EMA settings are Lengths 8, 14, 50, 
// Trade Pro's ATR is Length 14 


//@version=6
strategy('FOX Strat', overlay = true, process_orders_on_close = true, margin_long = 100, margin_short = 100, use_bar_magnifier = true)

//Flat Detector for Strat Ver. 
isFlat() =>
    strategy.position_size == 0

//indicator(title="Stochastic RSI", shorttitle="Stoch RSI", format=format.price, precision=2, timeframe="", timeframe_gaps=true)
smoothK = input.int(3, 'K', minval = 1, group = 'Stochastic RSI Settings')
smoothD = input.int(3, 'D', minval = 1, group = 'Stochastic RSI Settings')
lengthRSI = input.int(14, 'RSI Length', minval = 1, group = 'Stochastic RSI Settings')
lengthStoch = input.int(14, 'Stochastic Length', minval = 1, group = 'Stochastic RSI Settings')
src = input(close, title = 'RSI Source', group = 'Stochastic RSI Settings')
rsi1 = ta.rsi(src, lengthRSI)
k = ta.sma(ta.stoch(rsi1, rsi1, rsi1, lengthStoch), smoothK)
d = ta.sma(k, smoothD)

//rsiStoch Crossover
crossoverDown = ta.crossunder(k, d)
crossoverUp = ta.crossover(k, d)

//ema
emaLength = input.int(title = 'EMA Length1', defval = 10, minval = 1, group = 'EMA Settings')
ema = ta.ema(close, emaLength)
plot(ema, title = 'ema', color = color.purple)
emaLength2 = input.int(title = 'EMA Length2', defval = 20, minval = 1, group = 'EMA Settings')
ema2 = ta.ema(close, emaLength2)
plot(ema2, title = 'ema', color = color.green)
emaLength3 = input.int(title = 'EMA Length3', defval = 50, minval = 1, group = 'EMA Settings')
ema3 = ta.ema(close, emaLength3)
plot(ema3, title = 'ema', color = color.yellow)

//Ema Reqs
candleBeyondEma = input.bool(title = 'Require Entry Level to be over the EMA', defval = true)
overEmaReq = high > ema
underEmaReq = low < ema
overEma = candleBeyondEma == true ? overEmaReq : true
underEma = candleBeyondEma == true ? underEmaReq : true

emaLong = ema > ema2 and ema2 > ema3 and overEma
emaShort = ema < ema2 and ema2 < ema3 and underEma

//ATR
atrLength = input.int(title = 'Atr Length', defval = 10, step = 1, group = 'ATR Settings')
atr = ta.atr(atrLength)
atrStops = input(true, title = 'Check to use ATR Stops. Ticks otherwise', group = 'Stop Settings')

//Stop and Target Inputs
riskReward = input.float(title = 'Risk/Reward', group = 'Stop Settings', defval = 1, step = .1)
stopDistance = input.float(title = 'Stop Distance', group = 'Stop Settings', defval = 1.25, step = .25)
stopUnit = atrStops == true ? atr : syminfo.mintick

structureTotal = 4
//structureToggle = input.bool(title = "Toggle extra Lookback Length for Stop Distance", group="Stop Settings", defval=true)

//Enable Longs/Shorts
enableLongs = input.bool(title = 'Enable Longs', group = 'Long/Short', defval = true)
enableShorts = input.bool(title = 'Enable Shorts', group = 'Long/Short', defval = true)

//Testing Pullback Structure
pbStructureDisabler = input.bool(title = 'Disable PB Structure Requirement', defval = true, group = 'Candle Structure Setting', tooltip="Toggle off to Require a simple Candle Pullback Structure, in addition to the EMA + Stoch RSI")
longPbStructure = high > high[1] and high[1] < high[2] and high[2] < high[3]
shortPbStructure = low < low[1] and low[1] > low[2] and low[2] > low[3]

longPbStructureEmaReq = high[1] > ema[1] and high[2] > ema[2] and high[3] > ema[3]
shortPbStructureEmaReq = low[1] < ema[1] and low[2] < ema[2] and low[3] < ema[3]

longPbReq = longPbStructureEmaReq
shortPbReq = shortPbStructureEmaReq


// Defining Entries, Stops and Targets
// Setting to take the Trade Intra-Bar, only to be used when combined with the PB Structure Setting
// Using the 'Close' is default, because it's always feasible to take the trade there. 
// The Intra-Bar Entries are better, but we can't be certain the Stoch RSI and the EMAs would be aligned with the strategy when our conditions hit Intra-Bar, even though it's likely!
// Watch out for $$ Eyes
useIntraBarEntries = input.bool(title="Use Intra-Bar Entries", defval=false, group="Entry Rules", tooltip = "Only consider Enabling this with Pullback Structure Enabled as well. It's *likely* you'd be able to enter Intra-Bar with these rules, but not guaranteed, like the default 'Close' setting is. Toggling this on will show a much better, but possibly biased strategy, as there'll be instances where the EMA or the Stoch RSI isn't actually aligned yet when at the entry location. There's no way to account for that but watching the chart in real-time.  ")
entryLongPrice = useIntraBarEntries == true ? high[1] + 1 * syminfo.mintick : close
entryShortPrice = useIntraBarEntries == true ? low[1] -1 * syminfo.mintick : close

// //Entry Levels you'd use if you were watching intrabar. 

// entryLongPrice = high[1] + 1 * syminfo.mintick
// entryShortPrice = low[1] - 1 * syminfo.mintick



//Stops and Targets
stopSize = stopDistance * stopUnit
longTradeStopPrice = 0.0
longTradeTargetPrice = 0.0
shortTradeStopPrice = 0.0
shortTradeTargetPrice = 0.0
longEntryStopSize = entryLongPrice - ta.lowest(low, structureTotal) + stopSize
shortEntryStopSize = ta.highest(high, structureTotal) - entryShortPrice + stopSize

//Stops and Targets
longTradeStopOut = entryLongPrice - longEntryStopSize
longTradeTargetHit = entryLongPrice + longEntryStopSize * riskReward
shortTradeStopOut = entryShortPrice + shortEntryStopSize
shortTradeTargetHit = entryShortPrice - shortEntryStopSize * riskReward

//Gap Requirement
disableGaps = input.bool(defval = true, title = 'Enable to disable unrealistic gap entries.', group="Gap Rules", tooltip="For use with PB Structure and Intra-Bar Entry. Disables unfeasable Entries where the Open gapped past the desired entry price")
gapReqLong = disableGaps == true ? entryLongPrice >= open : true
gapReqShort = disableGaps == true ? entryShortPrice <= open : true

//Retracement Entry
longRTE = open > entryLongPrice and open < longTradeTargetPrice and low <= entryLongPrice
shortRTE = open < entryShortPrice and open > shortTradeTargetPrice and high >= entryShortPrice

entryLong = enableLongs and isFlat() and emaLong and crossoverUp and (longPbStructure or pbStructureDisabler) and (gapReqLong or longRTE)
entryShort = enableShorts and isFlat() and emaShort and crossoverDown and (shortPbStructure or pbStructureDisabler) and (gapReqShort or shortRTE)

if entryLong
    longTradeStopPrice := entryLongPrice - longEntryStopSize
    longTradeTargetPrice := entryLongPrice + longEntryStopSize * riskReward
    longTradeTargetPrice

//Shorts
if entryShort
    shortTradeStopPrice := entryShortPrice + shortEntryStopSize
    shortTradeTargetPrice := entryShortPrice - shortEntryStopSize * riskReward
    shortTradeTargetPrice

//Plotting Entries, Stops, Targets
plot(entryLong ? entryLongPrice : na, title = 'Long Entry Price', color = color.white, style = plot.style_linebr, linewidth = 1)
plot(entryLong ? longTradeStopOut : na, title = 'Long Stop Price', color = color.orange, style = plot.style_linebr, linewidth = 1)
plot(entryLong ? longTradeTargetHit : na, title = 'Long Target Price', color = color.aqua, style = plot.style_linebr, linewidth = 1)

plot(entryShort ? entryShortPrice : na, title = 'Short Entry Price', color = color.white, style = plot.style_linebr, linewidth = 1)
plot(entryShort ? shortTradeStopOut : na, title = 'Short Stop Price', color = color.orange, style = plot.style_linebr, linewidth = 1)
plot(entryShort ? shortTradeTargetHit : na, title = 'Short Target Price', color = color.aqua, style = plot.style_linebr, linewidth = 1)


//Thanks to allanster for the "How to Backtest Time Ranges" script
// === INPUT DATE RANGE ===
fromDay = input.int(defval = 1, title = 'From Day', inline = 'Date Settings', group = 'Date/Time Settings', minval = 1, maxval = 31)
throughDay = input.int(defval = 1, title = 'Through Day', inline = 'Date Settings', group = 'Date/Time Settings', minval = 1, maxval = 31)
fromMonth = input.int(defval = 2, title = 'From Month', inline = 'Date Settings', group = 'Date/Time Settings', minval = 1, maxval = 12)
throughMonth = input.int(defval = 1, title = 'Through Month', inline = 'Date Settings', group = 'Date/Time Settings', minval = 1, maxval = 12)
fromYear = input.int(defval = 2022, title = 'From Year', inline = 'Date Settings', group = 'Date/Time Settings', minval = 1970)
throughYear = input.int(defval = 2112, title = 'Through Year', inline = 'Date Settings', group = 'Date/Time Settings', minval = 1970)

// === INPUT TIME RANGE ===
entryTime = input.session('0835-1450', title = 'Entry Time', group = 'Date/Time Settings')
exitTime = input.session('0000-0000', title = 'Exit Time', group = 'Date/Time Settings')

// === DATE & TIME RANGE FUNCTIONS ===
isDate() => // create function "within window of dates"
    start = timestamp(fromYear, fromMonth, fromDay, 00, 00) // date start
    finish = timestamp(throughYear, throughMonth, throughDay, 23, 59) // date finish
    isDate = time >= start and time <= finish // current date is "within window of dates"
    isDate

isTime(_position) =>
    not na(time(timeframe.period, _position + ':1234567'))

//Adding them together, as well as a disabling tool
disableDay = input.bool(title = 'Disable Day Settings', group = 'Date/Time Settings', defval = true)
disableTime = input.bool(title = 'Disable Time Settings', group = 'Date/Time Settings', defval = true)
dateAndTimeRequirements = (isDate() or disableDay) and (isTime(entryTime) or disableTime)

////////////////////////////////////////////////////////////////////////////////

//Actual Entry/Exit Rules tucked in here
if entryLong and dateAndTimeRequirements
    strategy.entry('Long Entry', strategy.long)
    strategy.exit('TP/SL', 'Long Entry', stop = longTradeStopPrice, limit = longTradeTargetPrice)

if entryShort and dateAndTimeRequirements
    strategy.entry('Short Entry', strategy.short)
    strategy.exit('TP/SL', 'Short Entry', stop = shortTradeStopPrice, limit = shortTradeTargetPrice)

////////////////////////////////////////////////////////////////////////////////

plot(entryLong ? entryLongPrice : na, style = plot.style_linebr, color = color.white)
plot(entryLong ? longTradeStopPrice : na, style = plot.style_linebr, color = color.orange)
plot(entryLong ? longTradeTargetPrice : na, style = plot.style_linebr, color = color.blue)

plot(entryShort ? entryShortPrice : na, style = plot.style_linebr, color = color.white)
plot(entryShort ? shortTradeStopPrice : na, style = plot.style_linebr, color = color.orange)
plot(entryShort ? shortTradeTargetPrice : na, style = plot.style_linebr, color = color.blue)

//Strategy Evaluation. Thanks Zen https://www.tradingview.com/u/ZenAndTheArtOfTrading/
totalWins = 0
totalLoss = 0

if strategy.wintrades != strategy.wintrades[1]
    totalWins := totalWins + 1
    totalWins

if strategy.losstrades != strategy.losstrades[1]
    totalLoss := totalLoss + 1
    totalLoss

// Prepare stats table
drawTester = input.bool(title = 'Draw Backtester', defval = true, group = 'Table')

// Custom function to truncate (cut) excess decimal places
truncate(_number, _decimalPlaces) =>
    _factor = math.pow(10, _decimalPlaces)
    int(_number * _factor) / _factor

//Stats Table
var table testTable = table.new(position.top_right, 5, 2, border_width = 1)
f_fillCell(_table, _column, _row, _title, _value, _bgcolor, _txtcolor) =>
    _cellText = _title + '\n' + _value
    table.cell(_table, _column, _row, _cellText, bgcolor = _bgcolor, text_color = _txtcolor)

// Draw stats table
var bgcolor = color.new(color.black, 0)
if drawTester
    if barstate.islastconfirmedhistory
        // Update table
        f_fillCell(testTable, 0, 0, 'Total Trades:', str.tostring(strategy.closedtrades), bgcolor, color.white)
        f_fillCell(testTable, 0, 1, 'Wins', str.tostring(strategy.wintrades), bgcolor, color.white)
        f_fillCell(testTable, 1, 0, 'Losses', str.tostring(strategy.losstrades), bgcolor, color.white)
        f_fillCell(testTable, 1, 1, 'B/Es', str.tostring(strategy.eventrades), bgcolor, color.white)

