# Pinescript Utilities

[💻 View Full Source Code on GitHub](https://github.com/1nf1n1t3f1r3/Pinescript_Utilities)

A Repo containing some Pinescript Utility Scripts

The orange line is the ATR Stop. The blue labels are the Earnings. You can see at the bottom that Tradingview doesn't mark the first two here yet. The datestamp shows it's 2018 here. The two panels below the main chart are the Mac Spikes and the MACD-V

## ATR Stop

This is a little tool that constantly displays the ATR distance from the lows. You can use this to set a stop without having to do the measuring yourself. The lines are a bit distracting, so toggling them off and on again when setting a trade is the way I do it.

## Datestamp

A datestamp I adapted from another trader. I put it in the middle and cut out some of his fluff. It displays the Ticker, Date and Timeframe. helpful when backtesting and you want to take a picture of the chart.

## Earnings

You might already know this one... But it marks Earnings on the chart, going further back then Tradingview does by default. You can tell that the first 2 labels don't have the green [E] at the bottom of the panel

## MAC Spikes

This is something Adam H Grimes came up with. It detects spikes of volatility in a ticker. This is a good way to compare apples to apples

## MACD-V (AHG)

This is a modified MACD that uses Adam H Grimes' settings, and also changes the bounds of the MACD to be relative, rather than absolute. This way it's much more helpful to compare across tickers. I also added the little grey line that slowly descend from the highs; they indicate when the MACD makes a new relative high.

## PED

A tool to display potential entry points. The white line indicates where you might enter a trade, with a Stop and Target on the orange/blue lines. It's not intended to enter wherever they're drawn on the chart, but to be used as a consistent entry technique at the tail-end of some strategy.

![SEC Scraper Spreadsheet Output](/images/Pinescript_Utilities_BA.png)

## Sneaky Bonus: PED Python

If you can write Pine.. Why stop there? Python's much better for large-scale data analysis ...

You might intuit this is one of my earlier works. It takes the raw format of the PED and translates that directly to Python. Totally repetitive code. Makes the size of the script absolutely explode. It has kind of a nostalgic vibe to it, doesn't it?

```
import pandas as pd
import yfinance as yf
import ta
# import numpy as np

# Read the input CSV file
input_data = pd.read_csv('ML21.csv')

all_data=[]

def safe_to_datetime(date_str):
    try:
        return pd.to_datetime(date_str)
    except ValueError:
        return None

# Iterate over each row in the input data
for index, row in input_data.iterrows():
    ticker = row['Ticker']
    direction = row['Direction']
    start_date = safe_to_datetime(row['Date'])
    underway_date = safe_to_datetime(row['Date2']) if pd.notna(row['Date2']) else pd.Timestamp.today()
    end_date = safe_to_datetime(row['Date3']) if pd.notna(row['Date3']) else pd.Timestamp.today()

    try:
        # Fetch historical data from Yahoo Finance for the given ticker
        data = yf.download(ticker, start=start_date - pd.Timedelta(days=21), end=end_date)

        # Add the 'Ticker' and 'Direction' columns to the left side of the DataFrame
        data.insert(0, 'Ticker', ticker)
        data.insert(1, 'Direction', direction)

        # Calculate EMA
        ema_length = 20
        data['ema'] = ta.trend.ema_indicator(data['Close'], window=ema_length)

        # Calculate ATR
        atr_length = 10
        data['atr'] = ta.volatility.AverageTrueRange(high=data['High'], low=data['Low'], close=data['Close'], window=atr_length).average_true_range()

        # Define pivot conditions and other calculations (similar to your original script)
        data['pivotLongFilterHLHL'] = (
            (data['High'] > data['ema']) &
            (data['High'].shift(1) > data['ema'].shift(1)) &
            (data['High'].shift(2) > data['ema'].shift(2)) &
            (data['Close'].shift(3) > data['ema'].shift(3))
        )

        data['HLHL'] = (
            data['pivotLongFilterHLHL'] &
            (data['High'] <= data['High'].shift(1)) &
            (data['High'].shift(1) > data['High'].shift(2)) &
            (data['High'].shift(2) < data['High'].shift(3)) &
            (data['High'].shift(1) <= data['High'].shift(3))
        )

        data['pivotLongFilter2'] = (
                (data['High'] > data['ema']) &
                (data['High'].shift(1) > data['ema'].shift(1)) &
                (data['Close'].shift(2) > data['ema'].shift(2))  # Final close condition
        )

        data['pvh2'] = (
                data['pivotLongFilter2'] &
                (data['High'] <= data['High'].shift(1)) &
                (data['High'].shift(1) < data['High'].shift(2)) &
                (data['High'].shift(2) >= data['High'].shift(3))
        )

        # Define additional conditions
        data['pivotLongFilter3'] = (
                (data['High'] > data['ema']) &
                (data['High'].shift(1) > data['ema'].shift(1)) &
                (data['High'].shift(2) > data['ema'].shift(2)) &
                (data['Close'].shift(3) > data['ema'].shift(3))  # Final close condition
        )

        data['pvh3'] = (
                data['pivotLongFilter3'] &
                (data['High'] <= data['High'].shift(1)) &
                (data['High'].shift(1) <= data['High'].shift(2)) &
                (data['High'].shift(2) < data['High'].shift(3)) &
                (data['High'].shift(3) >= data['High'].shift(4))
        )

        data['pivotLongFilter4'] = (
                (data['High'] > data['ema']) &
                (data['High'].shift(1) > data['ema'].shift(1)) &
                (data['High'].shift(2) > data['ema'].shift(2)) &
                (data['Close'].shift(3) > data['ema'].shift(3))
        )

        data['pvh4'] = (
                data['pivotLongFilter4'] &
                (data['High'] <= data['High'].shift(1)) &
                (data['High'].shift(1) <= data['High'].shift(2)) &
                (data['High'].shift(2) <= data['High'].shift(3)) &
                (data['High'].shift(3) < data['High'].shift(4)) &
                (data['High'].shift(4) >= data['High'].shift(5))
        )

        data['pivotLongFilter5'] = (
                (data['High'] > data['ema']) &
                (data['High'].shift(1) > data['ema'].shift(1)) &
                (data['High'].shift(2) > data['ema'].shift(2)) &
                (data['High'].shift(3) > data['ema'].shift(3)) &
                (data['High'].shift(4) > data['ema'].shift(4)) &
                (data['Close'].shift(5) > data['ema'].shift(5))
        )

        data['pvh5'] = (
                data['pivotLongFilter5'] &
                (data['High'] <= data['High'].shift(1)) &
                (data['High'].shift(1) <= data['High'].shift(2)) &
                (data['High'].shift(2) <= data['High'].shift(3)) &
                (data['High'].shift(3) <= data['High'].shift(4)) &
                (data['High'].shift(4) < data['High'].shift(5)) &
                (data['High'].shift(5) >= data['High'].shift(6))
        )

        data['pivotLongFilter6'] = (
                (data['High'] > data['ema']) &
                (data['High'].shift(1) > data['ema'].shift(1)) &
                (data['High'].shift(2) > data['ema'].shift(2)) &
                (data['High'].shift(3) > data['ema'].shift(3)) &
                (data['High'].shift(4) > data['ema'].shift(4)) &
                (data['High'].shift(5) > data['ema'].shift(5)) &
                (data['Close'].shift(6) > data['ema'].shift(6))
        )

        data['pvh6'] = (
                data['pivotLongFilter6'] &
                (data['High'] <= data['High'].shift(1)) &
                (data['High'].shift(1) <= data['High'].shift(2)) &
                (data['High'].shift(2) <= data['High'].shift(3)) &
                (data['High'].shift(3) <= data['High'].shift(4)) &
                (data['High'].shift(4) <= data['High'].shift(5)) &
                (data['High'].shift(5) < data['High'].shift(6)) &
                (data['High'].shift(6) >= data['High'].shift(7))
        )

        data['pivotLongFilter7'] = (
                (data['High'] > data['ema']) &
                (data['High'].shift(1) > data['ema'].shift(1)) &
                (data['High'].shift(2) > data['ema'].shift(2)) &
                (data['High'].shift(3) > data['ema'].shift(3)) &
                (data['High'].shift(4) > data['ema'].shift(4)) &
                (data['High'].shift(5) > data['ema'].shift(5)) &
                (data['High'].shift(6) > data['ema'].shift(6)) &
                (data['Close'].shift(7) > data['ema'].shift(7))
        )

        data['pvh7'] = (
                data['pivotLongFilter7'] &
                (data['High'] <= data['High'].shift(1)) &
                (data['High'].shift(1) <= data['High'].shift(2)) &
                (data['High'].shift(2) <= data['High'].shift(3)) &
                (data['High'].shift(3) <= data['High'].shift(4)) &
                (data['High'].shift(4) <= data['High'].shift(5)) &
                (data['High'].shift(5) <= data['High'].shift(6)) &
                (data['High'].shift(6) < data['High'].shift(7)) &
                (data['High'].shift(7) >= data['High'].shift(8))
        )

        # Define pivot conditions for shorts
        data['pivotShortFilterLHLH'] = (
                (data['Low'] < data['ema']) &
                (data['Low'].shift(1) < data['ema'].shift(1)) &
                (data['Low'].shift(2) < data['ema'].shift(2)) &
                (data['Close'].shift(3) < data['ema'].shift(3))
        )

        data['LHLH'] = (
                data['pivotShortFilterLHLH'] &
                (data['Low'] >= data['Low'].shift(1)) &
                (data['Low'].shift(1) < data['Low'].shift(2)) &
                (data['Low'].shift(2) > data['Low'].shift(3)) &
                (data['Low'].shift(1) >= data['Low'].shift(3))
        )

        # Calculate pivot short filters explicitly
        data['pivotShortFilter2'] = (
                (data['Low'] < data['ema']) &
                (data['Low'].shift(1) < data['ema'].shift(1)) &
                (data['Low'].shift(2) < data['ema'].shift(2)) &
                (data['Close'].shift(2) < data['ema'].shift(2))
        )

        data['pvl2'] = (
                data['pivotShortFilter2'] &
                (data['Low'] >= data['Low'].shift(1)) &
                (data['Low'].shift(1) > data['Low'].shift(2)) &
                (data['Low'].shift(2) <= data['Low'].shift(3))
        )

        data['pivotShortFilter3'] = (
                (data['Low'] < data['ema']) &
                (data['Low'].shift(1) < data['ema'].shift(1)) &
                (data['Low'].shift(2) < data['ema'].shift(2)) &
                (data['Low'].shift(3) < data['ema'].shift(3)) &
                (data['Close'].shift(3) < data['ema'].shift(3))
        )

        data['pvl3'] = (
                data['pivotShortFilter3'] &
                (data['Low'] >= data['Low'].shift(1)) &
                (data['Low'].shift(1) >= data['Low'].shift(2)) &
                (data['Low'].shift(2) > data['Low'].shift(3)) &
                (data['Low'].shift(3) <= data['Low'].shift(4))
        )

        data['pivotShortFilter4'] = (
                (data['Low'] < data['ema']) &
                (data['Low'].shift(1) < data['ema'].shift(1)) &
                (data['Low'].shift(2) < data['ema'].shift(2)) &
                (data['Low'].shift(3) < data['ema'].shift(3)) &
                (data['Low'].shift(4) < data['ema'].shift(4)) &
                (data['Close'].shift(4) < data['ema'].shift(4))
        )

        data['pvl4'] = (
                data['pivotShortFilter4'] &
                (data['Low'] >= data['Low'].shift(1)) &
                (data['Low'].shift(1) >= data['Low'].shift(2)) &
                (data['Low'].shift(2) >= data['Low'].shift(3)) &
                (data['Low'].shift(3) > data['Low'].shift(4)) &
                (data['Low'].shift(4) <= data['Low'].shift(5))
        )

        data['pivotShortFilter5'] = (
                (data['Low'] < data['ema']) &
                (data['Low'].shift(1) < data['ema'].shift(1)) &
                (data['Low'].shift(2) < data['ema'].shift(2)) &
                (data['Low'].shift(3) < data['ema'].shift(3)) &
                (data['Low'].shift(4) < data['ema'].shift(4)) &
                (data['Close'].shift(5) < data['ema'].shift(5))
        )

        data['pvl5'] = (
                data['pivotShortFilter5'] &
                (data['Low'] >= data['Low'].shift(1)) &
                (data['Low'].shift(1) >= data['Low'].shift(2)) &
                (data['Low'].shift(2) >= data['Low'].shift(3)) &
                (data['Low'].shift(3) >= data['Low'].shift(4)) &
                (data['Low'].shift(4) > data['Low'].shift(5)) &
                (data['Low'].shift(5) <= data['Low'].shift(6))
        )

        data['pivotShortFilter6'] = (
                (data['Low'] < data['ema']) &
                (data['Low'].shift(1) < data['ema'].shift(1)) &
                (data['Low'].shift(2) < data['ema'].shift(2)) &
                (data['Low'].shift(3) < data['ema'].shift(3)) &
                (data['Low'].shift(4) < data['ema'].shift(4)) &
                (data['Low'].shift(5) < data['ema'].shift(5)) &
                (data['Close'].shift(6) < data['ema'].shift(6))
        )

        data['pvl6'] = (
                data['pivotShortFilter6'] &
                (data['Low'] >= data['Low'].shift(1)) &
                (data['Low'].shift(1) >= data['Low'].shift(2)) &
                (data['Low'].shift(2) >= data['Low'].shift(3)) &
                (data['Low'].shift(3) >= data['Low'].shift(4)) &
                (data['Low'].shift(4) >= data['Low'].shift(5)) &
                (data['Low'].shift(5) > data['Low'].shift(6)) &
                (data['Low'].shift(6) <= data['Low'].shift(7))
        )

        data['pivotShortFilter7'] = (
                (data['Low'] < data['ema']) &
                (data['Low'].shift(1) < data['ema'].shift(1)) &
                (data['Low'].shift(2) < data['ema'].shift(2)) &
                (data['Low'].shift(3) < data['ema'].shift(3)) &
                (data['Low'].shift(4) < data['ema'].shift(4)) &
                (data['Low'].shift(5) < data['ema'].shift(5)) &
                (data['Low'].shift(6) < data['ema'].shift(6)) &
                (data['Close'].shift(7) < data['ema'].shift(7))
        )

        data['pvl7'] = (
                data['pivotShortFilter7'] &
                (data['Low'] >= data['Low'].shift(1)) &
                (data['Low'].shift(1) >= data['Low'].shift(2)) &
                (data['Low'].shift(2) >= data['Low'].shift(3)) &
                (data['Low'].shift(3) >= data['Low'].shift(4)) &
                (data['Low'].shift(4) >= data['Low'].shift(5)) &
                (data['Low'].shift(5) >= data['Low'].shift(6)) &
                (data['Low'].shift(6) > data['Low'].shift(7)) &
                (data['Low'].shift(7) <= data['Low'].shift(8))
        )

        # Constant Calculations for Entry Levels, Stops & Targets
        data['long_entry_price'] = data['High'] + 1
        data['short_entry_price'] = data['Low'] - 1
        lookback_window = 3
        stop_distance = 1
        data['lowest_low'] = data['Low'].rolling(window=lookback_window).min()
        data['highest_high'] = data['High'].rolling(window=lookback_window).max()
        data['stop_size'] = stop_distance * data['atr']
        data['long_trade_stop_price'] = data['lowest_low'] - data['stop_size']
        data['long_trade_profit_target'] = data['long_entry_price'] + (data['long_entry_price'] - data['long_trade_stop_price'])
        data['short_trade_stop_price'] = data['highest_high'] + data['stop_size']
        data['short_trade_profit_target'] = data['short_entry_price'] - (data['short_trade_stop_price'] - data['short_entry_price'])

        # Create markers for long entry prices
        data['long_entry_marker_HLHL'] = data.apply(lambda row: row['long_entry_price'] if row['HLHL'] else None, axis=1)
        data['long_entry_marker_pvh2'] = data.apply(lambda row: row['long_entry_price'] if row['pvh2'] else None, axis=1)
        data['long_entry_marker_pvh3'] = data.apply(lambda row: row['long_entry_price'] if row['pvh3'] else None, axis=1)
        data['long_entry_marker_pvh4'] = data.apply(lambda row: row['long_entry_price'] if row['pvh4'] else None, axis=1)
        data['long_entry_marker_pvh5'] = data.apply(lambda row: row['long_entry_price'] if row['pvh5'] else None, axis=1)
        data['long_entry_marker_pvh6'] = data.apply(lambda row: row['long_entry_price'] if row['pvh6'] else None, axis=1)
        data['long_entry_marker_pvh7'] = data.apply(lambda row: row['long_entry_price'] if row['pvh7'] else None, axis=1)
        # Create markers for short entry prices
        data['short_entry_marker_LHLH'] = data.apply(lambda row: row['short_entry_price'] if row['LHLH'] else None, axis=1)
        data['short_entry_marker_pvl2'] = data.apply(lambda row: row['short_entry_price'] if row['pvl2'] else None, axis=1)
        data['short_entry_marker_pvl3'] = data.apply(lambda row: row['short_entry_price'] if row['pvl3'] else None, axis=1)
        data['short_entry_marker_pvl4'] = data.apply(lambda row: row['short_entry_price'] if row['pvl4'] else None, axis=1)
        data['short_entry_marker_pvl5'] = data.apply(lambda row: row['short_entry_price'] if row['pvl5'] else None, axis=1)
        data['short_entry_marker_pvl6'] = data.apply(lambda row: row['short_entry_price'] if row['pvl6'] else None, axis=1)
        data['short_entry_marker_pvl7'] = data.apply(lambda row: row['short_entry_price'] if row['pvl7'] else None, axis=1)

        # Process the data based on the 'direction' field
        if direction == 'Short':
            # Filter and process short entries

            # LHLH
            data['LHLH_prev_low_condition'] = (
                    (data['LHLH'].shift(1)) &
                    (data['Low'] < data['Low'].shift(1))
            )
            data['new_low_marker_LHLH'] = data.apply(lambda row: row['Low'] if row['LHLH_prev_low_condition'] else None,
                                                     axis=1)
            entry_dates_LHLH = data.loc[data['new_low_marker_LHLH'].notna(), 'new_low_marker_LHLH'].index

            # pvl2
            data['pvl2_prev_low_condition'] = (
                    (data['pvl2'].shift(1)) &
                    (data['Low'] < data['Low'].shift(1))
            )
            data['new_low_marker_pvl2'] = data.apply(lambda row: row['Low'] if row['pvl2_prev_low_condition'] else None,
                                                     axis=1)
            entry_dates_pvl2 = data.loc[data['new_low_marker_pvl2'].notna(), 'new_low_marker_pvl2'].index

            # pvl3
            data['pvl3_prev_low_condition'] = (
                    (data['pvl3'].shift(1)) &
                    (data['Low'] < data['Low'].shift(1))
            )
            data['new_low_marker_pvl3'] = data.apply(lambda row: row['Low'] if row['pvl3_prev_low_condition'] else None,
                                                     axis=1)
            entry_dates_pvl3 = data.loc[data['new_low_marker_pvl3'].notna(), 'new_low_marker_pvl3'].index

            # pvl4
            data['pvl4_prev_low_condition'] = (
                    (data['pvl4'].shift(1)) &
                    (data['Low'] < data['Low'].shift(1))
            )
            data['new_low_marker_pvl4'] = data.apply(lambda row: row['Low'] if row['pvl4_prev_low_condition'] else None,
                                                     axis=1)
            entry_dates_pvl4 = data.loc[data['new_low_marker_pvl4'].notna(), 'new_low_marker_pvl4'].index

            # pvl5
            data['pvl5_prev_low_condition'] = (
                    (data['pvl5'].shift(1)) &
                    (data['Low'] < data['Low'].shift(1))
            )
            data['new_low_marker_pvl5'] = data.apply(lambda row: row['Low'] if row['pvl5_prev_low_condition'] else None,
                                                     axis=1)
            entry_dates_pvl5 = data.loc[data['new_low_marker_pvl5'].notna(), 'new_low_marker_pvl5'].index

            # pvl6
            data['pvl6_prev_low_condition'] = (
                    (data['pvl6'].shift(1)) &
                    (data['Low'] < data['Low'].shift(1))
            )
            data['new_low_marker_pvl6'] = data.apply(lambda row: row['Low'] if row['pvl6_prev_low_condition'] else None,
                                                     axis=1)
            entry_dates_pvl6 = data.loc[data['new_low_marker_pvl6'].notna(), 'new_low_marker_pvl6'].index

            # pvl7
            data['pvl7_prev_low_condition'] = (
                    (data['pvl7'].shift(1)) &
                    (data['Low'] < data['Low'].shift(1))
            )
            data['new_low_marker_pvl7'] = data.apply(lambda row: row['Low'] if row['pvl7_prev_low_condition'] else None,
                                                     axis=1)
            entry_dates_pvl7 = data.loc[data['new_low_marker_pvl7'].notna(), 'new_low_marker_pvl7'].index
            pass
        else:
            # Filter and process long entries
            # Define new conditions for each setup

            # HLHL
            data['HLHL_prev_high_condition'] = (
                    (data['HLHL'].shift(1)) &
                    (data['High'] > data['High'].shift(1))
            )
            data['new_high_marker_HLHL'] = data.apply(lambda row: row['High'] if row['HLHL_prev_high_condition'] else None,
                                                      axis=1)
            entry_dates_HLHL = data.loc[data['new_high_marker_HLHL'].notna(), 'new_high_marker_HLHL'].index

            # pvh2
            data['pvh2_prev_high_condition'] = (
                    (data['pvh2'].shift(1)) &
                    (data['High'] > data['High'].shift(1))
            )

            data['new_high_marker_pvh2'] = data.apply(lambda row: row['High'] if row['pvh2_prev_high_condition'] else None,
                                                      axis=1)
            entry_dates_pvh2 = data.loc[data['new_high_marker_pvh2'].notna(), 'new_high_marker_pvh2'].index

            # pvh3
            data['pvh3_prev_high_condition'] = (
                    (data['pvh3'].shift(1)) &
                    (data['High'] > data['High'].shift(1))
            )
            data['new_high_marker_pvh3'] = data.apply(lambda row: row['High'] if row['pvh3_prev_high_condition'] else None,
                                                      axis=1)
            entry_dates_pvh3 = data.loc[data['new_high_marker_pvh3'].notna(), 'new_high_marker_pvh3'].index

            # pvh4
            data['pvh4_prev_high_condition'] = (
                    (data['pvh4'].shift(1)) &
                    (data['High'] > data['High'].shift(1))
            )
            data['new_high_marker_pvh4'] = data.apply(lambda row: row['High'] if row['pvh4_prev_high_condition'] else None,
                                                      axis=1)
            entry_dates_pvh4 = data.loc[data['new_high_marker_pvh4'].notna(), 'new_high_marker_pvh4'].index

            # pvh5
            data['pvh5_prev_high_condition'] = (
                    (data['pvh5'].shift(1)) &
                    (data['High'] > data['High'].shift(1))
            )
            data['new_high_marker_pvh5'] = data.apply(lambda row: row['High'] if row['pvh5_prev_high_condition'] else None,
                                                      axis=1)
            entry_dates_pvh5 = data.loc[data['new_high_marker_pvh5'].notna(), 'new_high_marker_pvh5'].index

            # pvh6
            data['pvh6_prev_high_condition'] = (
                    (data['pvh6'].shift(1)) &
                    (data['High'] > data['High'].shift(1))
            )
            data['new_high_marker_pvh6'] = data.apply(lambda row: row['High'] if row['pvh6_prev_high_condition'] else None,
                                                      axis=1)
            entry_dates_pvh6 = data.loc[data['new_high_marker_pvh6'].notna(), 'new_high_marker_pvh6'].index

            # pvh7
            data['pvh7_prev_high_condition'] = (
                    (data['pvh7'].shift(1)) &
                    (data['High'] > data['High'].shift(1))
            )
            data['new_high_marker_pvh7'] = data.apply(lambda row: row['High'] if row['pvh7_prev_high_condition'] else None,
                                                      axis=1)
            entry_dates_pvh7 = data.loc[data['new_high_marker_pvh7'].notna(), 'new_high_marker_pvh7'].index
            pass

        # Display or save results for each ticker and date range
        print(f"Results for {ticker} from {start_date} to {end_date}")


        # Let's try to trim the csv


        # data = data.drop(['Open', 'High', 'Low', 'Close', 'Adj Close', 'Volume', 'ema', 'atr',
        #                   'pivotLongFilterHLHL', 'pivotLongFilter2', 'pivotLongFilter3', 'pivotLongFilter4',
        #                   'pivotLongFilter4', 'pivotLongFilter5', 'pivotLongFilter6', 'pivotLongFilter7',
        #                   'pivotShortFilterLHLH', 'pivotShortFilter2', 'pivotShortFilter3', 'pivotShortFilter4',
        #                   'pivotShortFilter5', 'pivotShortFilter6', 'pivotShortFilter7',
        #                   'long_entry_price', 'short_entry_price', 'lowest_low', 'highest_high', 'stop_size',
        #                   'long_trade_stop_price', 'long_trade_profit_target', 'short_trade_stop_price',
        #                   'short_trade_profit_target',
        #                   'HLHL', 'pvh2', 'pvh3', 'pvh4', 'pvh5', 'pvh6', 'pvh7',
        #                   'LHLH', 'pvl2', 'pvl3', 'pvl4', 'pvl5', 'pvl6', 'pvl7',
        #                   'long_entry_marker_HLHL', 'long_entry_marker_pvh2', 'long_entry_marker_pvh3',
        #                   'long_entry_marker_pvh4', 'long_entry_marker_pvh5', 'long_entry_marker_pvh6',
        #                   'long_entry_marker_pvh7',
        #                   'short_entry_marker_LHLH', 'short_entry_marker_pvl2', 'short_entry_marker_pvl3',
        #                   'short_entry_marker_pvl4', 'short_entry_marker_pvl5', 'short_entry_marker_pvl6',
        #                   'short_entry_marker_pvl7'
        #
        #                   ], axis=1)

        columns_to_keep = ['Date', 'Ticker', 'Direction',
                           'new_high_marker_HLHL', 'new_high_marker_pvh2', 'new_high_marker_pvh3', 'new_high_marker_pvh4',
                           'new_high_marker_pvh5', 'new_high_marker_pvh6', 'new_high_marker_pvh7',
                           'new_low_marker_LHLH', 'new_low_marker_pvl2', 'new_low_marker_pvl3',
                           'new_low_marker_pvl4','new_low_marker_pvl5', 'new_low_marker_pvl6', 'new_low_marker_pvl7'
                           ]

        # Get the columns that are present in the DataFrame
        existing_columns_to_keep = [col for col in columns_to_keep if col in data.columns]
        data = data[existing_columns_to_keep]

        # Set the Ticker / Direction / Date Order.
        data = data.reset_index()


        direction_col = data.pop('Direction')
        data.insert(0, 'Direction', direction_col)
        ticker_col = data.pop('Ticker')
        data.insert(0, 'Ticker', ticker_col)

        # Add all other Columns together and clean up the Sum column
        columns_to_not_reorder = ['Ticker', 'Direction', 'Date'] # Ticker isn't in here because it's an index or something

        columns_to_sum = [col for col in data.columns if col not in columns_to_not_reorder]
        for col in columns_to_sum:
            data[col] = pd.to_numeric(data[col], errors='coerce')


        print(data[columns_to_sum].dtypes)

        data['Sum'] = data[columns_to_sum].sum(axis=1)
        data.drop(columns=columns_to_sum, inplace=True)
        data = data[columns_to_not_reorder + ['Sum']]
        data = data[data['Sum'] != 0]

        # Append
        all_data.append(data)

    except Exception as e:
        print(f"An error occurred while processing ticker {ticker}: {e}")
        continue  # Skip to the next ticker

# Concat all DFs into one
combined_df=pd.concat(all_data, ignore_index=False)
combined_df.to_csv('ML21_entry_dates_cb.csv'
                   '', index=False)


```
