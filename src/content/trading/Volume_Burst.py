# pipeline.py
import os
import json
import pandas as pd
import numpy as np
import yfinance as yf
from datetime import datetime, timedelta, date
import pandas_market_calendars as mcal

# YF Daily Cache
storage_folder = r"D:\Trading_Data\ticker_data_cache_unadjusted"
metadata_file = os.path.join(storage_folder, "ticker_metadata.json")
os.makedirs(storage_folder, exist_ok=True)

# 5M Cache
LOCAL_CACHE_FOLDER = r"D:\Trading_Data\ticker_data_cache_5M_TV"

# ------------------------------
# CONFIG / PARAMETERS
# ------------------------------

PARAMS = {
    # Files
    "INPUT_CSV": r"E:\1Coding\Projects\PycharmProjects\Trade_Management_from_Entry_Date\russell_1000_2024.csv",
    "OUTPUT_CSV": r"E:\1Coding\Projects\PycharmProjects\Trade_Management_from_Entry_Date\VB_R1000_6M2025.csv",

    # ATR Settings for Daily and Intraday
    "ATR_PERIOD": 10,

    # Daily
    "VOLUME_LOOKBACK": 5,
    "VOLUME_MULT": 3,

    # Liquidity Filter
    "DAILY_LIQUIDITY_LOOKBACK_WINDOW": 30,
    "DAILY_MIN_AVG_VOLUME": 1000000,
    "DAILY_MIN_PRICE": 10,

    # Daily Filter based on if the Daily is the same color bar
    "DAILY_COLOR_FILTER": True,

    # Daily Filter Based on having a High/Low over the Previous Day's High/Low
    "FILTER_HIGH_LOW": False,

    # Daily Filter based on having a Gap. If Upward Gap, no Shorts and Vice Versa
    "GAP_COLOR_FILTER": True,

    # ATR range filter
    "DAILY_USE_ATR_RANGE": True,
    "DAILY_ATR_MULT": 2,  # require daily range > 1.0 × ATR

    # Close location filter
    "DAILY_USE_CLV": True,
    "DAILY_CLV_LONG": 0.75,  # long burst: close must be above 60% of range
    "DAILY_CLV_SHORT": 0.25,  # short burst: close must be below 40%

    # 5Min
    "MAX_ENTRY_BARS": 10,
    "EMA_PERIOD": 20,

    "ROLLING_HIGH_LOW_LOOKBACK": 3,
    "STOP_ATR_MULT": 0.75,
    "ATR_TRAIL_DECAY": 0.9,
    "TARGET_R": 1,

    # Open Bar Filter
    "OPEN_BAR_COLOR_FILTER": True,

    # EMA Filter
    "EMA_FILTER": True,
    "EMA_FILTER_DIST": 3,

    # Daily Close versus Intraday Entry Candidate Filter
    "DAILY_USE_ENTRY_DIST": True,   # enable ATR-distance entry filter
    "DAILY_ENTRY_DIST": 3,    #  entry must be at least 1 ATR above daily close
}
nyse = mcal.get_calendar("NYSE")

# ------------------------------
# DAILY SIGNAL GENERATOR
# ------------------------------
def fetch_stock_data_update_8(
    df,
    ticker_metadata,
    start_date_extender=1825,
    initialization_extender=325,
    end_date_extender=30,
    delisting_threshold=30,
    validate_exchange=False,
    metadata_validation=False,  # New flag to toggle the YF metadata block
):
    """
    Fetches and updates stock data for a given set of tickers. The function processes an input DataFrame containing tickers and their associated metadata, downloads or updates stock data as needed, validates metadata, and handles missing data by extending the data range where necessary.

    ### Parameters:
    - df (pandas.DataFrame): A DataFrame containing stock tickers and associated metadata (Company Name, Exchange, and Dates). The DataFrame should have columns named 'Ticker', 'Company_Name', 'Exchange', 'Date', 'Date2', and 'Date3'.
    - ticker_metadata (dict): A dictionary containing metadata for each ticker (e.g., earliest available date, company name, exchange).
    - start_date_extender (int, optional): The number of days to extend the start date for fetching stock data. Default is 1825 (5 years).
    - end_date_extender (int, optional): The number of days to extend the end date for fetching stock data. Default is 1 day.
    - delisting_threshold (int, optional): The number of consecutive missing days in stock data before considering a ticker as delisted. Default is 30 days.
    - validate_exchange (bool, optional): A flag indicating whether to strictly validate the exchange from the metadata. Default is False.

    ### Returns:
    - ticker_data_cache (dict): A dictionary where the keys are tickers and the values are DataFrames containing the fetched stock data for the specified date range.

    ### Behavior:
    1. The function first checks whether a cached file for the ticker exists. If it does, the function attempts to read it and validate its contents.
    2. If the cached file is invalid (missing required data or containing all nulls), it is deleted, and the function moves on to process the next ticker.
    3. The function then validates the metadata for the ticker (e.g., company name, exchange). If the metadata is invalid or mismatches the input data, it logs the issue and skips processing that ticker.
    4. If there are missing data gaps, it fetches the missing data from Yahoo Finance.
    5. The function ensures that no duplicate entries exist in the cached data and trims the data to the requested date range.
    6. If no cached file exists, the function validates the metadata and downloads the full stock data for the ticker.

    ### Error Handling:
    - If errors occur during metadata fetching or data processing (e.g., missing columns, download failures), the function logs the issues and skips the problematic ticker.
    - The function generates a CSV file listing any metadata-related issues for review.

    ### Notes:
    - The function can be computationally intensive if a large number of tickers need to be processed.
    """

    required_columns = ["Ticker", "Company_Name", "Exchange", "Date", "Date2", "Date3"]

    # Check if DataFrame has a multi-index and reset it
    if isinstance(df.index, pd.MultiIndex) or isinstance(df.index, pd.Index):
        print("Resetting DataFrame index to bring index columns back into the DataFrame...")
        df = df.reset_index()

    # Standardize column names: strip whitespace and remove non-breaking spaces
    df.columns = (
        df.columns
        .str.strip()
        .str.replace(r'\s+', '_', regex=True)
        .str.replace(u'\xa0', '', regex=False)
    )

    # Debugging: Print current column names
    print("Sanitized columns in input DataFrame:", list(df.columns))
    print("Required columns:", required_columns)

    # Check for missing columns
    missing_columns = [col for col in required_columns if col not in df.columns]
    if missing_columns:
        raise ValueError(
            f"Input file is missing the following required columns: {missing_columns}. "
            f"Columns in the input file: {list(df.columns)}."
        )

    # Ensure required columns are in standardized case
    df.rename(columns={col.lower(): col for col in required_columns if col.lower() in df.columns}, inplace=True)

    df['Ticker'] = df['Ticker'].str.upper()
    ticker_data_cache = {}
    issues = []  # To record metadata mismatches
    unique_tickers = df['Ticker'].unique()

    for ticker in unique_tickers:
        print(f"\nProcessing ticker: {ticker}")
        ticker = ticker.strip()
        file_path = os.path.join(storage_folder, f"{ticker}.csv")

        df['Date'] = pd.to_datetime(df['Date'])
        df['Date2'] = pd.to_datetime(df['Date2'], errors='coerce')
        df['Date3'] = pd.to_datetime(df['Date3'], errors='coerce')

        ticker_rows = df[df['Ticker'] == ticker]
        start_date = ticker_rows['Date'].min()
        end_date = ticker_rows[['Date2', 'Date3']].max().max()

        if pd.isna(end_date):
            print(f"Warning: end_date is NaT for ticker {ticker}. Using start date +1 as fallback.")
            end_date = start_date + timedelta(1)

        start_date_extended_minus_initializer = start_date - timedelta(days=start_date_extender)
        start_date_extended = start_date_extended_minus_initializer - timedelta(days=initialization_extender)
        end_date_extended = end_date + timedelta(days=end_date_extender)

        # Limit to earliest available date based on stored ticker metadata
        if ticker in ticker_metadata and 'Earliest_Date' in ticker_metadata[ticker]:
            earliest_date = datetime.strptime(ticker_metadata[ticker]['Earliest_Date'], '%Y-%m-%d')
            if start_date_extended < earliest_date:
                print(f"Adjusting start_date_extended for {ticker} to metadata earliest date: {earliest_date}")
                start_date_extended = earliest_date

        # Limit to last available date based on current datetime's yesterday
        today = datetime.now().date()
        yesterday = today - timedelta(days=1)
        if end_date_extended.date() > yesterday:  # Convert end_date_extended to datetime.date for comparison
            print(f"Adjusting end_date_extended for {ticker} to yesterday: {yesterday}")
            end_date_extended = pd.Timestamp(yesterday)  # Ensure end_date_extended remains a Timestamp

        start_date_extended_str = start_date_extended.strftime('%Y-%m-%d')
        end_date_extended_str = end_date_extended.strftime('%Y-%m-%d')
        print(f"Fetching data for {ticker} from {start_date_extended_str} to {end_date_extended_str}")

        try:
            if os.path.exists(file_path):
                try:
                    df_cached = pd.read_csv(file_path, index_col="Date", parse_dates=True)

                    essential_columns = {'Open', 'High', 'Low', 'Close'}
                    if df_cached.empty or df_cached.shape[0] == 0 or not essential_columns.issubset(df_cached.columns)  or df_cached['Open'].isnull().all():
                        print(f"Cached file for {ticker} is invalid. Clearing cache.")
                        os.remove(file_path)
                        continue

                    # Convert to timezone native
                    if df_cached.index.tz is not None:
                        df_cached.index = df_cached.index.tz_localize(None)
                    df_cached.sort_index(inplace=True)

                    # Toggle-controlled metadata validation block
                    if metadata_validation:
                        try:
                            ticker_info = yf.Ticker(ticker).info
                            company_name = ticker_info.get("longName", "Unknown")
                            exchange = ticker_info.get("Exchange", "Unknown")
                        except Exception as e:
                            print(f"Error retrieving metadata for {ticker}: {e}")
                            issues.append({
                                "ticker": ticker,
                                "reason": "Metadata fetch error",
                                "error": str(e)
                            })
                            continue

                        if ticker not in ticker_metadata:
                            print(f"Metadata missing for {ticker}. Skipping ticker.")
                            issues.append({
                                "ticker": ticker,
                                "reason": "Metadata missing",
                                "Company_Name": company_name,
                                "Exchange": exchange
                            })
                            continue
                        else:
                            stored_name = ticker_metadata[ticker].get("Company_Name")
                            stored_exchange = ticker_metadata[ticker].get("Exchange")

                            if company_name != stored_name or (validate_exchange and exchange != stored_exchange):
                                print(f"Metadata mismatch for {ticker}. Expected {stored_name} ({stored_exchange}), got {company_name} ({exchange}).")
                                issues.append({
                                    "Ticker": ticker,
                                    "Expected_Name": stored_name,
                                    "Expected_Exchange": stored_exchange,
                                    "Actual_Name": company_name,
                                    "Actual_Exchange": exchange
                                })
                                continue

                    trading_days = nyse.valid_days(start_date=start_date_extended_str,
                                                   end_date=end_date_extended_str).tz_localize(None)
                    missing_dates = pd.DatetimeIndex(trading_days).difference(df_cached.index)

                    if not missing_dates.empty:
                        print(f"Detected gaps in cached data for {ticker}. Missing dates: {missing_dates}")
                        missing_start = missing_dates.min()
                        missing_end = missing_dates.max()

                        if (date.today() - missing_end.date()).days <= 7:
                            missing_start = missing_start - timedelta(days=2)
                            missing_end = date.today() + timedelta(days=1)

                        missing_start_str = missing_start.strftime('%Y-%m-%d')
                        missing_end_str = missing_end.strftime('%Y-%m-%d')
                        print(f"Fetching data from {missing_start_str} to {missing_end_str} for {ticker}")

                        df_new = yf.download(ticker, start=missing_start_str, end=None, actions=True,
                                             auto_adjust=False, multi_level_index=False)

                        if not df_new.empty:
                            missing_streak_days = pd.DatetimeIndex(trading_days).difference(df_new.index)
                            streak_dates = missing_streak_days[missing_streak_days > df_new.index.max()]

                            if len(streak_dates) >= delisting_threshold:
                                delisting_date = streak_dates.min() - timedelta(days=1)
                                print(f"{ticker} appears to be delisted as of {delisting_date}. Adjusting end_date.")
                                missing_end = delisting_date

                            df_new = df_new.loc[missing_start:missing_end]
                            df_cached = pd.concat([df_cached, df_new]).sort_index()
                            df_cached = df_cached[~df_cached.index.duplicated(keep='first')]
                            df_cached.to_csv(file_path)
                        else:
                            print(f"No new data found for {ticker} from {missing_start_str} to {missing_end_str}")
                            issues.append({
                                "ticker": ticker,
                                "reason": "Date mismatch",
                                "missing_start": missing_start_str,
                                "missing_end": missing_end_str
                            })
                    else:
                        print(f"Full range of data is already accounted for in the cache for {ticker}.")

                except (ValueError, KeyError, pd.errors.ParserError) as e:
                    print(f"Error processing cached file for {ticker}: {e}")
                    continue

            else:  # if not os.path.exists(file_path):
                print(f"Validating metadata for full download of {ticker}")
                try:
                    ticker_info = yf.Ticker(ticker).info
                    company_name = ticker_info.get("longName", "Unknown")
                    exchange = ticker_info.get("Exchange", "Unknown")

                    # Validate against input file (strict validation)
                    input_rows = ticker_rows.iloc[0]
                    input_name = input_rows.get("Company_Name", "Unknown")
                    input_exchange = input_rows.get("Exchange", "Unknown")

                    if company_name != input_name or (validate_exchange and exchange != input_exchange):
                        print(f"Input file mismatch for {ticker}. Expected {input_name} ({input_exchange}), got {company_name} ({exchange}).")
                        issues.append({
                            "ticker": ticker,
                            "reason": "Input file mismatch",
                            "expected_name": input_name,
                            "expected_exchange": input_exchange,
                            "actual_name": company_name,
                            "actual_exchange": exchange
                        })
                        continue

                    print(f"Metadata for {ticker} validated successfully.")
                    metadata_valid = True

                except Exception as e:
                    print(f"Error retrieving metadata for {ticker}: {e}")
                    issues.append({
                        "ticker": ticker,
                        "reason": "Metadata fetch error during validation",
                        "error": str(e)
                    })
                    continue

                if metadata_valid:
                    print(f"Downloading full data range for {ticker}")
                    df_cached = yf.download(ticker, start=None, end=None, actions=True,
                                            auto_adjust=False, multi_level_index=False)
                    if not df_cached.empty:
                        earliest_date = df_cached.index.min()
                        ticker_metadata[ticker] = {
                            "Earliest_Date": earliest_date.strftime('%Y-%m-%d'),
                            "Company_Name": company_name,
                            "Exchange": exchange
                        }

                    df_cached = df_cached[~df_cached.index.duplicated(keep='first')]
                    df_cached.to_csv(file_path)

            # Drop duplicates in cache for consistency
            df_cached = df_cached[~df_cached.index.duplicated(keep='first')]

            # Trim data to requested range for return, without altering the cache
            df_result = df_cached.loc[start_date_extended_str:end_date_extended_str].copy()

            # Add the new variable as a column to the result dataframe
            df_result['start_date_extended_minus_initializer'] = start_date_extended_minus_initializer

            ticker_data_cache[ticker] = df_result

        except Exception as e:
            print(f"Unexpected error for {ticker}: {e}")
            continue

    # Write issues to CSV for review if any mismatches occurred
    if issues:
        issues_df = pd.DataFrame(issues)
        issues_file_path = os.path.join(storage_folder, "metadata_issues.csv")
        issues_df.to_csv(issues_file_path, index=False)
        print(f"Metadata issues recorded in {issues_file_path}")

    return ticker_data_cache


# --- Filter Functions ---
def liquidity_filter(df: pd.DataFrame, params: dict) -> pd.Series:
    lookback = params.get("DAILY_LIQUIDITY_LOOKBACK_WINDOW")
    min_avg_vol = params.get("DAILY_MIN_AVG_VOLUME")
    min_price = params.get("DAILY_MIN_PRICE")

    avg_vol = df["Volume"].rolling(lookback).mean()
    avg_close = df["Close"].rolling(lookback).mean()

    return (avg_vol >= min_avg_vol) & (avg_close >= min_price)


def high_low_filter(df: pd.DataFrame, params: dict) -> pd.Series:
    prev_high = df["High"].shift(1)
    prev_low = df["Low"].shift(1)
    green = df["Close"] > df["Open"]
    red = df["Close"] < df["Open"]

    return (green & (df["High"] > prev_high)) | (red & (df["Low"] < prev_low))


def atr_range_filter(df: pd.DataFrame, params: dict) -> pd.Series:
    atr_mult = params.get("DAILY_ATR_MULT")
    return (df["High"] - df["Low"]) > atr_mult * df["ATR_Daily"]


def clv_filter(df: pd.DataFrame, params: dict) -> pd.Series:
    clv = (df["Close"] - df["Low"]) / (df["High"] - df["Low"]).replace(0, np.nan)
    long_filter = (df["Close"] > df["Open"]) & (clv > params.get("DAILY_CLV_LONG"))
    short_filter = (df["Close"] < df["Open"]) & (clv < params.get("DAILY_CLV_SHORT"))
    return long_filter | short_filter


def volume_burst_finder(input_csv, ticker_metadata, params):
    """
    Detects volume bursts for tickers listed in an input CSV.

    Parameters
    ----------
    input_csv : str
        Path to CSV with columns [Ticker, Date, Date2].
    ticker_metadata : dict
        Metadata dictionary passed to fetch_stock_data_update_8.
    volLookback : int
        Lookback window for average volume.
    volMultiplier : float
        Volume multiplier threshold.

    Returns
    -------
    dict
        Dictionary of {ticker: DataFrame}, each containing the burst signals.
    """
    # Get Params
    volLookback = params.get("VOLUME_LOOKBACK")
    volMultiplier = params.get("VOLUME_MULT")
    atr_period = params.get("ATR_PERIOD")

    # Load tickers and dates
    df = pd.read_csv(input_csv)
    df["Date"] = pd.to_datetime(df["Date"])
    df["Date2"] = pd.to_datetime(df["Date2"], errors="coerce")

    # Fetch stock data
    ticker_data_cache = fetch_stock_data_update_8(df, ticker_metadata)

    results = {}

    for ticker, data in ticker_data_cache.items():
        if "Volume" not in data.columns:
            continue  # skip if no volume

        # --- Add daily ATR RMA---
        data = data.copy()
        data["ATR_Daily"] = compute_atr_rma(data, period=atr_period)

        # Add daily bar direction
        data["Daily_Green"] = data["Close"] > data["Open"]
        data["Daily_Red"] = data["Close"] < data["Open"]

        # Compute avgVol with shift (use previous day’s SMA)
        avgVol = data["Volume"].rolling(volLookback).mean().shift(1)

        # Ratio vs threshold
        volRatio = data["Volume"] / avgVol
        burst = (volRatio > volMultiplier) & liquidity_filter(data, params)

        if params.get("FILTER_HIGH_LOW", False):
            burst &= high_low_filter(data, params)

        if params.get("DAILY_USE_ATR_RANGE", False):
            burst &= atr_range_filter(data, params)

        if params.get("DAILY_USE_CLV", False):
            burst &= clv_filter(data, params)

        # Add signals back into df
        data["avgVol"] = avgVol
        data["volRatio"] = volRatio
        data["burst"] = burst

        # Restrict to requested [Date, Date2] interval
        start_date = df.loc[df["Ticker"] == ticker, "Date"].min()
        end_date = df.loc[df["Ticker"] == ticker, "Date2"].max()
        if pd.isna(end_date):
            end_date = data.index.max()

        data = data.loc[start_date:end_date]

        results[ticker] = data

    return results

# Daily call from Main
def daily_signal_generator(params, ticker_metadata):
    input_csv = params["INPUT_CSV"]

    print(f"Running daily signal generation from {input_csv}...")
    results = volume_burst_finder(input_csv, ticker_metadata, params)

    # Flatten into DataFrame of hits
    burst_rows = []
    for ticker, df in results.items():
        hits = df[df["burst"]].copy()
        if hits.empty:
            continue

        for dt, row in hits.iterrows():
            burst_rows.append({
                "Ticker": ticker,
                "Date": dt.strftime("%Y-%m-%d"),
                "Daily_Close": row["Close"],  # <-- add this
                "Volume": row["Volume"],
                "AvgVolume": row["avgVol"],
                "VolRatio": row["volRatio"],
                "Range": row["High"] - row["Low"],
                "ATR_Daily": row.get("ATR_Daily", np.nan),
                "CLV": (row["Close"] - row["Low"]) / ((row["High"] - row["Low"]) if (row["High"] != row["Low"]) else np.nan),
                "Daily_Green": row.get("Daily_Green", False),
                "Daily_Red": row.get("Daily_Red", False),
            })

    signals_df = pd.DataFrame(burst_rows)
    print(f"Found {len(signals_df)} daily burst signals.")

    # Optional: dump to CSV for inspection
    signals_df.to_csv("Volume_Burst_Full_Daily_Part_YF.csv", index=False)

    return signals_df


# ------------------------------
# INTRADAY BACKTESTER
# ------------------------------
DATA_CACHE = {}  # global in-memory cache for 5-min bars

# ------------------------------
# LOCAL 5M CSV CACHE FETCHER
# ------------------------------

LOCAL_DATA_CACHE = {}  # in-memory cache for CSV 5m bars

def fetch_5m_data_localcache(ticker: str) -> pd.DataFrame:
    """
    Fetch cached 5-min bars for a ticker from local CSV cache.
    Example file: D:\\Trading_Data\\ticker_data_cache_5m\\AAPL.csv
    """
    ticker = ticker.upper()
    file_path = os.path.join(LOCAL_CACHE_FOLDER, f"{ticker}.csv")
    if not os.path.exists(file_path):
        raise ValueError(f"No cached CSV file found for ticker {ticker} in {LOCAL_CACHE_FOLDER}")

    if ticker in LOCAL_DATA_CACHE:
        df = LOCAL_DATA_CACHE[ticker]
    else:
        df = pd.read_csv(file_path)

        # Normalize datetime and column casing
        if "Datetime" not in df.columns:
            if "date" in df.columns:
                df = df.rename(columns={"date": "Datetime"})
            elif "timestamp" in df.columns:
                df = df.rename(columns={"timestamp": "Datetime"})

        df["Datetime"] = pd.to_datetime(df["Datetime"], errors="coerce")
        df["Date"] = df["Datetime"].dt.date
        df["Time"] = df["Datetime"].dt.strftime("%H:%M:%S")

        # Normalize OHLCV casing
        df = df.rename(columns={
            "open": "Open",
            "high": "High",
            "low": "Low",
            "close": "Close",
            "volume": "Volume"
        })

        # Reorder and keep only expected columns
        df = df[["Datetime", "Date", "Time", "Open", "High", "Low", "Close", "Volume"]]

        LOCAL_DATA_CACHE[ticker] = df

    return df.copy()


def compute_ema(series: pd.Series, period: int) -> pd.Series:
    return series.ewm(span=period, adjust=False).mean()


def compute_atr_rma(df: pd.DataFrame, period: int) -> pd.Series:
    high_low = df["High"] - df["Low"]
    high_close = np.abs(df["High"] - df["Close"].shift())
    low_close = np.abs(df["Low"] - df["Close"].shift())
    tr = high_low.combine(high_close, np.maximum).combine(low_close, np.maximum)
    return tr.ewm(alpha=1/period, adjust=False).mean()


def build_indicator_dataframe(df: pd.DataFrame, params: dict) -> pd.DataFrame:
    """
    Build intraday indicators and rolling lows/highs for a day.
    Params is a dict containing:
        - ATR_PERIOD
        - EMA_PERIOD
        - ROLLING_HIGH_LOW_LOOKBACK
    """
    df = df.copy()
    atr_period = params.get("ATR_PERIOD")
    ema_period = params.get("EMA_PERIOD")
    rolling_high_low_lookback = params.get("ROLLING_HIGH_LOW_LOOKBACK")

    df["ATR"] = compute_atr_rma(df, atr_period)
    df["EMA"] = compute_ema(df["Close"], ema_period)

    # Compute rolling highs/lows per day, using only previous bars
    df["Rolling_Low"] = (
        df.groupby("Date")["Low"]
        .transform(lambda x: x.rolling(rolling_high_low_lookback, min_periods=1).min())
        .shift(1)
    )

    df["Rolling_High"] = (
        df.groupby("Date")["High"]
        .transform(lambda x: x.rolling(rolling_high_low_lookback, min_periods=1).max())
        .shift(1)
    )

    return df


def get_next_trading_day_from_df(df: pd.DataFrame, signal_date: pd.Timestamp) -> str:
    all_dates = sorted(df["Date"].unique())
    after = [d for d in all_dates if d > signal_date]
    if not after:
        raise ValueError(f"No valid trading day found after {signal_date}")
    return after[0].strftime("%Y-%m-%d")



def detect_breakout_entries(
    df: pd.DataFrame,
    date_str: str,
    params: dict,
    daily_green: bool = None,
    daily_red: bool = None,
    daily_close: float = None   # <-- new
) -> pd.DataFrame:

    """
    Rolling breakout strategy with ATR/EMA filter.
    Fully safe for index slices: operates on the daily slice only.
    """



    # Params
    max_entry_bars = params.get("MAX_ENTRY_BARS")
    stop_atr_mult = params.get("STOP_ATR_MULT")

    target_r = params.get("TARGET_R")
    min_tick = 0.01  # or however you define tick size
    daily_color_filter = params.get("DAILY_COLOR_FILTER")
    gap_color_filter = params.get("GAP_COLOR_FILTER")
    open_bar_color_filter = params.get("OPEN_BAR_COLOR_FILTER")


    # Add Columns
    df_day = df.reset_index(drop=True).copy()  # ensure 0..N indexing
    df_day["Daily_Close"] = daily_close

    df_day["Entry_Long"] = np.nan
    df_day["Entry_Short"] = np.nan
    df_day["Entry_Dist_ATR"] = np.nan
    df_day["Stop_Long"] = np.nan
    df_day["Stop_Short"] = np.nan
    df_day["Target_Long"] = np.nan
    df_day["Target_Short"] = np.nan

    trade_taken = False
    long_cancelled = False
    short_cancelled = False

    # Open Bar Color Filter
    if open_bar_color_filter:
        if len(df_day) > 0:
            first_open_bar = df_day.iloc[0]
            first_open = first_open_bar["Open"]
            first_close = first_open_bar["Close"]

            if first_close > first_open:
                short_cancelled = True
            elif first_close < first_open:
                long_cancelled = True

    # Gap Color Filter
    if gap_color_filter:
        if daily_close is not None and len(df_day) > 0:
            first_open = df_day.iloc[0]["Open"]
            gap = first_open - daily_close

            if gap < 0:
                long_cancelled = True
            if gap > 0:
                short_cancelled = True

    # Daily Color Filter
    if daily_color_filter:
        if daily_green:  # only allow longs
            short_cancelled = True
        elif daily_red:  # only allow shorts
            long_cancelled = True
        else:  # flat bar, disable both
            long_cancelled = True
            short_cancelled = True

    for i in range(1, min(max_entry_bars + 1, len(df_day))):
        if trade_taken:
            break

        prev = df_day.iloc[i - 1]
        curr = df_day.iloc[i]

        prev_ema = prev["EMA"]
        prev_high = prev["High"]
        prev_low = prev["Low"]
        atr = prev["ATR"]

        # --- Long breakout ---
        if not long_cancelled and not trade_taken:
            if curr["High"] > prev_high:
                dist_atr = (prev_high - prev_ema) / atr
                dist_from_daily = abs(prev_high - daily_close) / atr if daily_close is not None else np.nan

                # EMA Filter
                if params.get("EMA_FILTER", False):
                    if dist_atr < params.get("EMA_FILTER_DIST"):  # cancel if too close to EMA
                        long_cancelled = True
                        continue  # skip to next bar

                # # --- Gap Filter ---
                # if params.get("DISABLE_ON_OPPOSITE_GAP", False):
                #     if curr["Open"] < daily_close:  # Gap down
                #         long_cancelled = True
                #         continue  # skip to next bar

                # Daily Close Versus Prev Intraday High (Candidate Entry Location) Filter
                if params.get("DAILY_USE_ENTRY_DIST", False):
                    if dist_from_daily < params.get("DAILY_ENTRY_DIST"):
                        print(f"dist_atr = {dist_atr}. prev_high = {prev_high} - prev_ema {prev_ema} / atr {atr}")
                        long_cancelled = True
                        continue  # skip to next bar

                # --- If all filters pass, take the trade ---
                stop_price = df_day.iloc[i]["Rolling_Low"] - atr * stop_atr_mult
                entry_price = max(prev_high + min_tick, curr["Open"])
                target_price = entry_price + (entry_price - stop_price) * target_r

                df_day.iat[i, df_day.columns.get_loc("Entry_Long")] = entry_price
                df_day.iat[i, df_day.columns.get_loc("Entry_Dist_ATR")] = dist_atr
                df_day.iat[i, df_day.columns.get_loc("Stop_Long")] = stop_price
                df_day.iat[i, df_day.columns.get_loc("Target_Long")] = target_price

                trade_taken = True

        # --- Short breakout ---
        if not short_cancelled:
            if curr["Low"] < prev_low:
                dist_atr = (prev_ema - prev_low) / atr
                dist_from_daily = abs(prev_low - daily_close) / atr if daily_close is not None else np.nan

                # EMA Filter
                if params.get("EMA_FILTER", False):
                    if dist_atr < params.get("EMA_FILTER_DIST"):  # cancel if too close to EMA
                        short_cancelled = True
                        continue  # skip to next bar

                # # --- Gap Filter ---
                # if params.get("DISABLE_ON_OPPOSITE_GAP", False):
                #     if curr["Open"] > daily_close:  # Gap up
                #         short_cancelled = True
                #         continue  # skip to next bar

                # Daily Close Versus Prev Intrday Low (Candidate Entry Location) Filter
                if params.get("DAILY_USE_ENTRY_DIST", False):
                    if dist_from_daily < params.get("DAILY_ENTRY_DIST"):
                        short_cancelled = True
                        continue  # skip to next bar

                # Take the trade
                stop_price = df_day.iloc[i]["Rolling_High"] + atr * stop_atr_mult
                entry_price = min(prev_low - min_tick, curr["Open"])
                target_price = entry_price - (stop_price - entry_price) * target_r

                df_day.iat[i, df_day.columns.get_loc("Entry_Short")] = entry_price
                df_day.iat[i, df_day.columns.get_loc("Entry_Dist_ATR")] = dist_atr
                df_day.iat[i, df_day.columns.get_loc("Stop_Short")] = stop_price
                df_day.iat[i, df_day.columns.get_loc("Target_Short")] = target_price

                trade_taken = True

    return df_day

def backtest_entry_inplace(
    df: pd.DataFrame,
    stop_atr_mult: float = 1.0,
    atr_trail_decay: float = 0.9,
    tie_breaker: str = "stop_first"
) -> pd.DataFrame:
    """
    Backtest a single trade in-place using precomputed Rolling_Low/High.
    Expects df to already contain:
      - 'Entry_Long' / 'Entry_Short'
      - 'Stop_Long' / 'Stop_Short'
      - 'Target_Long' / 'Target_Short'
      - 'Rolling_Low' / 'Rolling_High'
      - 'ATR'
    Adds columns:
      - 'Trailing_Stop'
      - 'Trade_R'
    """

    if "Rolling_Low" not in df.columns or "Rolling_High" not in df.columns:
        raise ValueError("Missing Rolling_Low / Rolling_High. Run build_indicator_dataframe first.")

    df = df.copy()
    df["Trailing_Stop"] = np.nan
    df["Trade_R"] = np.nan

    # --- Determine entry ---
    entry_long = df["Entry_Long"].dropna() if "Entry_Long" in df.columns else pd.Series(dtype=float)
    entry_short = df["Entry_Short"].dropna() if "Entry_Short" in df.columns else pd.Series(dtype=float)

    if not entry_long.empty:
        direction = "Long"
        entry_idx = entry_long.index[0]
        entry_price = entry_long.iloc[0]
        initial_stop = df.loc[entry_idx, "Stop_Long"]
        target = df.loc[entry_idx, "Target_Long"]
    elif not entry_short.empty:
        direction = "Short"
        entry_idx = entry_short.index[0]
        entry_price = entry_short.iloc[0]
        initial_stop = df.loc[entry_idx, "Stop_Short"]
        target = df.loc[entry_idx, "Target_Short"]
    else:
        return df  # no entry

    risk = abs(entry_price - initial_stop)
    if risk == 0:
        return df

    trailing_stop = initial_stop
    atr_mult = float(stop_atr_mult)
    bars = df.loc[entry_idx:].copy()
    n_bars = len(bars)
    exited = False

    for pos in range(n_bars):
        idx = bars.index[pos]
        bar = bars.iloc[pos]
        high, low = bar["High"], bar["Low"]

        # ATR from previous bar
        atr_for_calc = bars["ATR"].iloc[pos - 1] if pos > 0 else bar["ATR"]

        # Candidate trailing stop from precomputed Rolling_Low/High
        if direction == "Long":
            # candidate_stop = bar["Rolling_Low"] - (atr_for_calc * atr_mult)
            candidate_stop = low - (atr_for_calc * atr_mult)
            trailing_stop = max(trailing_stop, candidate_stop)
        else:
            # candidate_stop = bar["Rolling_High"] + (atr_for_calc * atr_mult)
            candidate_stop = high + (atr_for_calc * atr_mult)
            trailing_stop = min(trailing_stop, candidate_stop)


        df.loc[idx, "Trailing_Stop"] = trailing_stop

        # Exit checks
        hit_stop = (low <= trailing_stop) if direction == "Long" else (high >= trailing_stop)
        hit_target = (high >= target) if direction == "Long" else (low <= target)

        if hit_stop and hit_target:
            exit_price = target if tie_breaker == "target_first" else trailing_stop
            exited = True
        elif hit_target:
            exit_price = target
            exited = True
        elif hit_stop:
            exit_price = trailing_stop
            exited = True

        if exited:
            r = (exit_price - entry_price) / risk if direction == "Long" else (entry_price - exit_price) / risk
            df.loc[idx, "Trade_R"] = r
            df.loc[idx, "Exit_Price"] = exit_price  
            df.loc[idx:, "Trailing_Stop"] = trailing_stop
            break

        # Decay ATR multiplier
        atr_mult *= atr_trail_decay

    # If no exit triggered, close on last bar
    if not exited:
        last_idx = bars.index[-1]
        last_close = bars["Close"].iloc[-1]
        r = (last_close - entry_price) / risk if direction == "Long" else (entry_price - last_close) / risk
        df.loc[last_idx, "Trade_R"] = r
        df.loc[last_idx, "Exit_Price"] = last_close # <- Add this?
        df.loc[last_idx, "Trailing_Stop"] = trailing_stop

    return df




def condense_trade_rows(df: pd.DataFrame) -> pd.DataFrame:
    """
    Condense bar-by-bar df into one row per trade.
    Handles same-bar entry+exit trades safely.
    Keeps entry info from the entry bar, and final Trailing_Stop, Trade_R, Exit_Time, Direction, Entry/Stop/Target.
    Logs a CSV if anything goes wrong.
    """
    df = df.copy()

    try:
        # Keep only rows with Entry or Trade_R
        mask = pd.Series(False, index=df.index)
        for col in ("Entry_Long", "Entry_Short", "Trade_R"):
            if col in df.columns:
                mask |= df[col].notna()
        df_trades = df[mask].copy()
        if df_trades.empty:
            return df_trades

        # Rename Time → Entry_Time
        df_trades = df_trades.rename(columns={"Time": "Entry_Time"})

        # Ensure Exit_Time column exists
        df_trades["Exit_Time"] = pd.Series(dtype="string")

        # Identify entry row
        entry_rows = df_trades[(df_trades["Entry_Long"].notna()) | (df_trades["Entry_Short"].notna())]
        if entry_rows.empty:
            return pd.DataFrame()
        entry_idx = entry_rows.index[0]

        # Identify exit row (Trade_R filled)
        exit_rows = df_trades[df_trades["Trade_R"].notna()]
        if not exit_rows.empty:
            exit_idx = exit_rows.index[0]

            # Assign exit values to the entry row
            for col in ["Exit_Time", "Trade_R", "Trailing_Stop", "Exit_Price"]:
                if col in df_trades.columns:
                    df_trades.loc[entry_idx, col] = df_trades.loc[exit_idx, col]

        # Only keep the enriched entry row
        result = df_trades.loc[[entry_idx]].copy()

        # Add Direction + unified price/stop/target
        if pd.notna(result["Entry_Long"].iloc[0]):
            result["Direction"] = "Long"
            result["Entry_Price"] = result["Entry_Long"]
            result["Stop"] = result["Stop_Long"]
            result["Target"] = result["Target_Long"]
        else:
            result["Direction"] = "Short"
            result["Entry_Price"] = result["Entry_Short"]
            result["Stop"] = result["Stop_Short"]
            result["Target"] = result["Target_Short"]

        # Drop old columns (entry/stop/target long/short)
        drop_cols = [
            "Entry_Long", "Entry_Short",
            "Stop_Long", "Stop_Short",
            "Target_Long", "Target_Short",
        ]
        result = result.drop(columns=[c for c in drop_cols if c in result.columns])

        # Reorder columns
        preferred = [
            "Ticker", "Datetime", "Date", "Entry_Time", "Exit_Time", "Direction",
            "Entry_Price", "Exit_Price", "Stop", "Target", "Trade_R", "Trailing_Stop"
        ]
        others = [c for c in result.columns if c not in preferred]
        result = result[preferred + others]

        return result

    except Exception as e:
        ticker = df.get("Ticker", ["UNKNOWN"])[0] if "Ticker" in df.columns else "UNKNOWN"
        print(f"⚠️ condense_trade_rows failed for {ticker}")
        raise  # re-raise the original exception

def drop_raw_columns(df: pd.DataFrame) -> pd.DataFrame:
    """
    Drop raw market data and indicator columns for a cleaner trade log.
    Enable/disable as needed.
    """
    drop_cols = [
        "Open", "High", "Low", "Close", "Volume",
        "ATR", "EMA", "Entry_Dist_ATR"
    ]
    return df.drop(columns=[c for c in drop_cols if c in df.columns])


# Intraday Call from Main
def backtest_signals(signals_df, params):
    frames = []
    for ticker, group in signals_df.groupby("Ticker"):
        try:
            df_raw = fetch_5m_data_localcache(ticker)
            df_ind = build_indicator_dataframe(df_raw, params)

            for date_str, daily_green, daily_red, daily_close in zip(group["Date"], group["Daily_Green"], group["Daily_Red"], group["Daily_Close"]):
                signal_date = pd.to_datetime(date_str).date()
                try:
                    next_day = get_next_trading_day_from_df(df_ind, signal_date)
                except ValueError:
                    continue

                # Slice next-day bars
                target_mask = df_ind["Date"] == pd.to_datetime(next_day).date()
                target_bars = df_ind.loc[target_mask].copy().reset_index(drop=True)
                if target_bars.empty:
                    continue

                target_bars["Ticker"] = ticker

                # Run intraday detection + backtest
                daily_close_value = group.loc[group["Date"] == date_str, "Daily_Close"].values[0]
                target_bars = detect_breakout_entries(
                    target_bars,
                    date_str=date_str,  # <-- pass the date of the signal here
                    params=params,
                    daily_green=daily_green,
                    daily_red=daily_red,
                    daily_close=daily_close_value
                )

                target_bars = backtest_entry_inplace(
                    target_bars,
                    stop_atr_mult=params["STOP_ATR_MULT"],
                    atr_trail_decay=params["ATR_TRAIL_DECAY"]
                )

                # Comment out for Debugging Purposes
                target_bars = condense_trade_rows(target_bars)
                target_bars = drop_raw_columns(target_bars)

                if not target_bars.empty:
                    frames.append(target_bars)
        except Exception as e:
            print(f"Failed {ticker}: {e}")

    if frames:
        trades_df = pd.concat(frames, ignore_index=True)
    else:
        trades_df = pd.DataFrame()
    return trades_df

# ------------------------------
# MAIN RUNNER
# ------------------------------
if __name__ == "__main__":
    metadata_file = os.path.join(storage_folder, "ticker_metadata.json")
    if os.path.exists(metadata_file):
        with open(metadata_file, "r") as f:
            ticker_metadata = json.load(f)
    else:
        ticker_metadata = {}

    # 1️⃣ Daily burst signals
    signals_df = daily_signal_generator(PARAMS, ticker_metadata)

    # 2️⃣ Intraday backtest
    trades_df = backtest_signals(signals_df, PARAMS)

    # 3️⃣ Save results
    if not trades_df.empty:
        trades_df.to_csv(PARAMS["OUTPUT_CSV"], index=False)
        print(f"✅ Backtest results written to {PARAMS['OUTPUT_CSV']}")
    else:
        print("⚠️ No trades detected in backtest.")


# import os
# import time
#
# # Define the delay in seconds * minutes * hours
# delay = .25 * 60 * 60
#
# # Wait for the specified delay
# time.sleep(delay)
#
# # Execute the shutdown command
# os.system("shutdown /s /f /t 0")
