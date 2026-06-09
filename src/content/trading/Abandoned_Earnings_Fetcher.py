from selenium import webdriver
from selenium.webdriver.common.by import By
# from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.ui import Select
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time
from bs4 import BeautifulSoup
import pandas as pd
import re
import dateparser
from datetime import datetime  # Import it correctly
from collections import Counter
import random


# Specify paths
chrome_user_data_dir = r'C:\Users\jdv97\AppData\Local\Google\Chrome\User Data'
chrome_profile = 'Default'
download_dir = r'D:\ChromeDownloads'
chrome_driver_path = r'E:\1Coding\Chromedriver\chromedriver.exe'



def convert_to_date(date_str):
    """
    Helper Function
    Converts 'Month Day, Year' string to a datetime object."""
    try:
        return datetime.strptime(date_str, "%B %d, %Y")
    except ValueError:
        return None  # Return None if conversion fails



def initialize_driver(chrome_user_data_dir, chrome_profile, download_dir):
    """
    Initialize the WebDriver with specified options using Selenium's automatic driver manager.
    """
    options = webdriver.ChromeOptions()
    options.add_argument(f"user-data-dir={chrome_user_data_dir}")
    options.add_argument(f"profile-directory={chrome_profile}")

    # Crucial arguments for modern Chrome + Selenium stability
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument("--remote-debugging-port=9222")

    # Set up Chrome preferences to handle downloads
    prefs = {
        "download.default_directory": download_dir,
        "download.prompt_for_download": False,
        "download.directory_upgrade": True,
        "safebrowsing.enabled": True,
        "safebrowsing.disable_download_protection": True
    }
    options.add_experimental_option("prefs", prefs)

    # Selenium will automatically fetch the correct driver version for your Chrome application
    driver = webdriver.Chrome()
    return driver

SEC_URL = "https://www.sec.gov/edgar/search/"





def get_8k_filings_selenium(ticker):
    driver.get(SEC_URL)

    # Wait until the search input is clickable
    search_box = WebDriverWait(driver, 10).until(
        EC.element_to_be_clickable((By.ID, "entity-short-form"))
    )

    # Enter the ticker in the search box
    search_box.send_keys(ticker)
    search_box.submit()  # Submit the form
    time.sleep(3)

    # Wait until the second input field is clickable and enter the ticker again
    full_form_box = WebDriverWait(driver, 10).until(
        EC.element_to_be_clickable((By.ID, "entity-full-form"))
    )
    full_form_box.send_keys(ticker)
    time.sleep(3)

    print("0")

    # Select the full company name by pressing the arrow down key and Enter
    full_form_box.send_keys(Keys.ARROW_DOWN)
    time.sleep(2)
    full_form_box.send_keys(Keys.RETURN)
    time.sleep(2)

    print("1")

    # Wait until the date range dropdown is clickable
    date_range_dropdown = WebDriverWait(driver, 10).until(
        EC.element_to_be_clickable((By.ID, "date-range-select"))
    )

    # Select the 'All' option using the value attribute
    select = Select(date_range_dropdown)
    select.select_by_value("all")
    time.sleep(1)

    print("2")

    # First, click the correct dropdown menu to reveal form type filters
    filter_menu_button = WebDriverWait(driver, 10).until(
        EC.element_to_be_clickable((By.ID, "show-filing-types"))
    )
    filter_menu_button.click()
    time.sleep(1)  # Allow animation to complete

    print("3")

    # Now, select the 8-K option inside the expanded dropdown
    form_filter = WebDriverWait(driver, 10).until(
        EC.element_to_be_clickable((By.ID, "fcb158"))
    )
    form_filter.click()
    time.sleep(2)

    # Now, select the 8-K option inside the expanded dropdown
    form_filter = WebDriverWait(driver, 10).until(
        EC.element_to_be_clickable((By.ID, "custom_forms_set"))
    )
    form_filter.click()
    time.sleep(2)


def collect_8k_links(driver):
    """Extracts all 8-K filing links from the current page and handles pagination."""
    all_filings = []

    while True:
        # Parse current page with BeautifulSoup
        soup = BeautifulSoup(driver.page_source, "html.parser")

        # Find all 8-K report links
        for row in soup.find_all("tr"):  # Loop through all table rows
            link_tag = row.find("a", href=True)  # Find the hyperlink in the row
            date_tag = row.find_all("td")  # Find all <td> elements (date should be here)

            if link_tag and link_tag["href"] != "#0" and "8-K" in link_tag.text:
                filing_link = link_tag["href"] # + "https://www.sec.gov"
                date_tag = row.find("td")  # Ensure we extract the first <td> (filing date)
                filing_date = date_tag.text.strip() if date_tag else "Unknown Date"

                all_filings.append((filing_date, filing_link))

        print(f"Collected {len(all_filings)} filings so far...")

        # Check for "Next Page" button
        try:
            next_button = driver.find_element(By.XPATH, "//a[@class='page-link'][@data-value='nextPage']")
            if "disabled" in next_button.get_attribute("class"):
                print("No more pages found.")
                break  # No more pages left
            next_button.click()
            time.sleep(1)  # Allow new page to load
        except:
            print("Next Page button not found. Exiting pagination.")
            break  # No next page found

    print(all_filings)

    return all_filings



HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36",
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
    "Accept-Language": "en-US,en;q=0.9",
    "Accept-Encoding": "gzip, deflate, br",
    "Connection": "keep-alive",
    "Upgrade-Insecure-Requests": "1",
    "TE": "Trailers",
    "Referer": "https://www.sec.gov/edgar/search/",
}



def get_earnings_data_selenium_soup(driver, filing_url, ticker):
    """Extracts all possible earnings-related mentions from a filing using Selenium."""
    driver.get(filing_url)

    # Wait for page to load
    WebDriverWait(driver, 10).until(EC.presence_of_element_located((By.TAG_NAME, "body")))

    # Extract text
    soup = BeautifulSoup(driver.page_source, "html.parser")

    # Remove unnecessary elements
    for tag in soup(["script", "style", "ix:header", "ix:hidden"]):
        tag.extract()

    visible_text = soup.get_text(separator=" ", strip=True)

    # Find Report Type (Announce/Report/Disclose/Release)*
    match_report = re.search(r"(?P<earnings_type>announce|report|disclose|release|issue)", visible_text, re.IGNORECASE)
    report_type = match_report.group("earnings_type").upper() if match_report else "UNKNOWN"

    # Find All Earnings Mentions (Financial Results, Earnings Report, etc.)
    earnings_matches = re.findall(
        r"(financial\s+results|quarterly\s+results|earnings\s+(?:release|report|statement)?)",
        visible_text,
        re.IGNORECASE
    )

    # Find All Quarter Mentions
    quarter_matches = re.findall(r"(?:first|second|third|fourth)\s+quarter|(?:Q[1-4])", visible_text, re.IGNORECASE)

    # Normalize Quarter Terminology (Q1 → First Quarter, etc.)
    quarter_aliases = {
        "q1": "first quarter",
        "q2": "second quarter",
        "q3": "third quarter",
        "q4": "fourth quarter"
    }

    # Fix Unicode, Capitalization & Empty Quarter Matches
    normalized_quarters = [
        quarter_aliases.get(q.lower().replace("\xa0", " "), q.lower().replace("\xa0", " "))  # Normalize & Map
        for q in quarter_matches if q.strip()  # Ensure non-empty matches
    ]
    quarter_counts = Counter(normalized_quarters)  # Count occurrences

    # Find All Date Mentions
    date_matches = re.findall(
        r"\b(?:January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}\b",
        visible_text,
        re.IGNORECASE
    )

    # Fix Unicode & Capitalization Issues for Dates
    date_matches = [d.title().replace("\xa0", " ") for d in date_matches]  # Normalize non-breaking spaces
    date_counts = Counter(date_matches)  # Normalize non-breaking spaces

    # Common Quarter-End Dates
    quarter_end_dates = {"March 31", "June 30", "September 30", "December 31"}

    # Separate Quarter-End Dates from Other Dates
    dates_likely_quarter_related = [d for d in date_matches if any(q in d for q in quarter_end_dates)]
    dates_likely_non_quarter_related = [d for d in date_matches if d not in dates_likely_quarter_related]

    # Counting dates divided into quarter/non-quarter
    date_counts_quarter_related = Counter(dates_likely_quarter_related)
    date_counts_not_quarter_related = Counter(dates_likely_non_quarter_related)

    # Likely Earnings Release Date based on Count/Recency
    likely_earnings_release_date_based_on_count = date_counts_not_quarter_related.most_common(1)[0][0] if date_counts_not_quarter_related else "Unknown Most Common Release Date"
    likely_earnings_release_date_based_on_recency = max(dates_likely_non_quarter_related, key=lambda d: datetime.strptime(d,"%B %d, %Y")) if dates_likely_non_quarter_related else "Unknown Report Date"

    # Smart Selection Process (Avoid Crashes)
    # Extract the Most Common Quarter-End Date
    likely_quarter_based_on_counts = quarter_counts.most_common(1)[0][0] if quarter_counts else "Unknown Most Common Quarter"
    likely_quarter_date_based_on_recency = max(dates_likely_quarter_related, key=lambda d: datetime.strptime(d,"%B %d, %Y"))\
        if dates_likely_quarter_related else "Unknown Recency Quarter Date"
    likely_quarter_date_based_on_counts = date_counts_quarter_related.most_common(1)[0][0] if date_counts_quarter_related else "Unknown Most Common Quarter Date"

    print('likely_quarter_date_based_on_counts check update')
    print(likely_quarter_date_based_on_counts)

    # Pre-Filter: Only Process Valid Earnings Reports
    likely_earnings_report = report_type != "UNKNOWN" and earnings_matches and date_counts

    if not likely_earnings_report:
        print("-" * 50)
        print(f"🚫 Skipping non-earnings report: {filing_url}")
        print("-" * 50)
        return None  # Skip irrelevant reports completely

    # Final Matching of Earnings + Quarter + Date (Flexible Order)
    strict_match = None
    if likely_earnings_report:
        strict_pattern = (
            rf"({match_report.group('earnings_type')}.*?)?" if match_report else ""  # Optional Report Type
            rf"({'|'.join(earnings_matches)}).*?"  # Earnings Mention
            rf"({'|'.join(quarter_counts.keys())}).*?"  # Quarter Mention
            rf"({'|'.join(date_counts.keys())})"  # Date Mention
        )
        strict_match = re.search(strict_pattern, visible_text, re.IGNORECASE)

    # # Debugging: Print Everything Captured
    # print("-" * 50)
    # print(f"Filing: {filing_url}")
    # print(f"Report Type: {match_report.group('earnings_type') if match_report else 'Not Found'}")
    # print(f"Earnings Matches: {earnings_matches}")
    #
    # print(f"Quarter Matches: {quarter_counts}")
    # print(f"Date Matches: {date_counts}")
    #
    # print('Quarter matches/counts')
    # print(quarter_matches)
    # print(quarter_counts)
    #
    # print('Date matches/counts')
    # print(date_matches)
    # print(date_counts)
    #
    # print('Printing all Dates, divided as likely quarter / likely non-quarter dates')
    # print(dates_likely_quarter_related)
    # print(dates_likely_non_quarter_related)
    #
    # print('Likely quarter/earnings release dates:')
    # print(likely_quarter_date_based_on_recency)
    # print(likely_earnings_release_date_based_on_recency)
    #
    # print('Most commonly mentioned quarter and quarter date')
    # print(likely_quarter_based_on_counts)
    # print(likely_quarter_date_based_on_counts)
    # print("-" * 50)

    # Return Extracted Data
    extracted_data = {
        "ticker": ticker,
        "report_type": report_type,
        "filing_url": filing_url,
        "visible_text": visible_text,

        "earnings_matches": earnings_matches,

        "date_matches": date_matches,
        "date_counts": date_counts,

        "dates_likely_quarter_related": dates_likely_quarter_related,
        "dates_likely_non_quarter_related": dates_likely_non_quarter_related,

        "quarter_matches": quarter_matches,
        "quarter_counts": quarter_counts,

        "likely_quarter_based_on_counts": likely_quarter_based_on_counts,
        "likely_quarter_date_based_on_counts": likely_quarter_date_based_on_counts,
        "likely_quarter_date_based_on_recency": likely_quarter_date_based_on_recency,

        "likely_earnings_release_date_based_on_count" : likely_earnings_release_date_based_on_count,
        "likely_earnings_release_date_based_on_recency": likely_earnings_release_date_based_on_recency

    }

    return extracted_data


def determine_confidence_level(extracted_data):
    """Determines the confidence level for extracted earnings data."""

    # Get all the extracted_data
    ticker = extracted_data["ticker"]
    report_type = extracted_data["report_type"]
    filing_url = extracted_data["filing_url"]

    earnings_matches = extracted_data["earnings_matches"]

    date_matches = extracted_data["date_matches"]
    date_counts = extracted_data["date_counts"]

    dates_likely_quarter_related = extracted_data["dates_likely_quarter_related"]
    dates_likely_non_quarter_related = extracted_data["dates_likely_non_quarter_related"]

    quarter_matches = extracted_data["quarter_matches"]
    quarter_counts = extracted_data["quarter_counts"]

    # Extract quarter-related data
    likely_quarter_based_on_counts = extracted_data["likely_quarter_based_on_counts"]
    likely_quarter_date_based_on_counts = extracted_data["likely_quarter_date_based_on_counts"]
    likely_quarter_date_based_on_recency = extracted_data["likely_quarter_date_based_on_recency"]

    likely_earnings_release_date_based_on_recency = extracted_data["likely_earnings_release_date_based_on_recency"]
    likely_earnings_release_date_based_on_count = extracted_data["likely_earnings_release_date_based_on_count"]

    # Tinkering with values for more advanced comparisons

    # Mapping Quarters to Dates in order to equalize these variables:
    # likely_quarter_based_on_counts, likely_quarter_date_based_on_counts, likely_quarter_date_based_on_recency
    quarter_to_date_mapping = {
        "first quarter": "March 31",
        "second quarter": "June 30",
        "third quarter": "September 30",
        "fourth quarter": "December 31"
    }

    print("likely_quarter_based_on_counts, likely_quarter_date_based_on_counts, likely_quarter_date_based_on_recency:")
    print(repr(likely_quarter_based_on_counts))  # Use repr() to reveal hidden characters
    print(repr(likely_quarter_date_based_on_counts))
    print(repr(likely_quarter_date_based_on_recency))

    likely_quarter_date_based_on_recency = likely_quarter_date_based_on_recency.lower().strip().replace("\n", " ")

    likely_quarter_based_on_counts = likely_quarter_based_on_counts.lower().strip().replace("\n", " ")

    if likely_quarter_based_on_counts in quarter_to_date_mapping:
        likely_quarter_based_on_counts = quarter_to_date_mapping[likely_quarter_based_on_counts]
    else:
        likely_quarter_based_on_counts = "Unknown"

    print('likely_quarter_based_on_counts after dictionary')
    print(likely_quarter_based_on_counts)


    # Split variables into its date and its year.
    # Ensure the date contains ", " before splitting
    if ", " in likely_quarter_date_based_on_counts:
        likely_quarter_date_based_on_counts_md, likely_quarter_date_based_on_counts_year = likely_quarter_date_based_on_counts.rsplit(
            ", ", 1)
    else:
        likely_quarter_date_based_on_counts_md, likely_quarter_date_based_on_counts_year = likely_quarter_date_based_on_counts, "0000"

    if ", " in likely_quarter_date_based_on_recency:
        likely_quarter_date_based_on_recency_md, likely_quarter_date_based_on_recency_year = likely_quarter_date_based_on_recency.rsplit(
            ", ", 1)
    else:
        likely_quarter_date_based_on_recency_md, likely_quarter_date_based_on_recency_year = likely_quarter_date_based_on_recency, "0000"

    # Convert years to integers for comparison
    int_likely_quarter_date_based_on_counts_year = int(likely_quarter_date_based_on_counts_year)
    int_likely_quarter_date_based_on_recency_year = int(likely_quarter_date_based_on_recency_year)

    print('Integer versions')
    print(int_likely_quarter_date_based_on_counts_year)
    print(int_likely_quarter_date_based_on_recency_year)



    # Step 1: Quarter Confidence Check: Full Match
    quarter_match = (
            likely_quarter_based_on_counts == likely_quarter_date_based_on_counts_md == likely_quarter_date_based_on_recency_md
            and likely_quarter_date_based_on_counts_year == likely_quarter_date_based_on_recency_year
    )

    # Step 1: Quarter Confidence Check: Match with Year Prognosis
    quarter_match_with_next_year_prognosis = (likely_quarter_based_on_counts == likely_quarter_date_based_on_counts_md and
                                 likely_quarter_date_based_on_counts_md == likely_quarter_date_based_on_recency_md
                                 and int_likely_quarter_date_based_on_counts_year +1 == int_likely_quarter_date_based_on_recency_year
                                 )

    # match_with_end_of_year_prognosis = (likely_quarter_based_on_counts == likely_quarter_date_based_on_counts_md and
    #                              likely_quarter_date_based_on_counts_md == "December 31"
    #                              and likely_quarter_date_based_on_counts_year == likely_quarter_date_based_on_recency_year
    #                              )

    quarter_partial_match = (
            likely_quarter_based_on_counts == likely_quarter_date_based_on_counts_md
            or likely_quarter_based_on_counts == likely_quarter_date_based_on_recency_md
            or likely_quarter_date_based_on_counts == likely_quarter_date_based_on_recency
    )
    # Initialize variables to prevent undefined issues
    most_likely_quarter = None
    release_date_confidence = "⚠️ Low Confidence / Uncertain"  # Default fallback

    # Identify the Best Quarter-End Date if Partial Match
    partial_match_values = []

    if likely_quarter_based_on_counts == likely_quarter_date_based_on_counts_md:
        partial_match_values.append(likely_quarter_based_on_counts)

    if likely_quarter_based_on_counts == likely_quarter_date_based_on_recency_md:
        partial_match_values.append(likely_quarter_based_on_counts)

    if likely_quarter_date_based_on_counts_md == likely_quarter_date_based_on_recency_md:
        partial_match_values.append(likely_quarter_date_based_on_counts_md)

    # Step 1: Quarter Confidence Check
    if quarter_match:
        quarter_confidence = "✅ 100% CONFIRMED (Quarter & Quarter-End Match)"
        most_likely_quarter = likely_quarter_date_based_on_counts
        # Ensure we store a valid quarter name, defaulting to "Unknown"
        final_quarter = likely_quarter_based_on_counts if likely_quarter_based_on_counts != "Unknown" else "Unknown"

    elif quarter_match_with_next_year_prognosis:
        quarter_confidence = "📊 High Confidence (Partial March likely explainable through next year prognosis)"
        most_likely_quarter = likely_quarter_date_based_on_counts
        final_quarter = likely_quarter_based_on_counts if likely_quarter_based_on_counts != "Unknown" else "Unknown" # Store confirmed quarter
    elif quarter_partial_match:
        quarter_confidence = "📊 Medium Confidence (Partial Match)"
        most_likely_quarter = likely_quarter_date_based_on_counts
        final_quarter = max(set(partial_match_values), key=partial_match_values.count) if partial_match_values else "Unknown Quarter"

    else:
        quarter_confidence = "⚠️ Low Confidence / Uncertain"
        final_quarter = "Unknown Quarter"  # Store confirmed quarter

    # Step 2: Earnings Release Date Confidence Check
    release_date_confidence = "⚠️ Low Confidence / Uncertain"  # Default fallback
    final_earnings_release_date = "Unknown"

    if most_likely_quarter:
        most_likely_quarter_date = convert_to_date(most_likely_quarter)
        recency_date = convert_to_date(likely_earnings_release_date_based_on_recency)
        count_date = convert_to_date(likely_earnings_release_date_based_on_count)

        print("Quarter End Date:", most_likely_quarter_date)
        print("Earnings Release (Recency):", recency_date)
        print("Earnings Release (Count):", count_date)

        if None not in (recency_date, count_date, most_likely_quarter_date):
            # High Confidence: Recency & Count match AND date is after quarter-end
            if recency_date == count_date and recency_date > most_likely_quarter_date:
                release_date_confidence = "✅ 100% CONFIRMED (Match between Recency, Count & Datetime)"
                final_earnings_release_date = likely_earnings_release_date_based_on_recency  # Store this as the confirmed date

            # Medium Confidence: Recency & Count match, but quarter-end alignment is unclear
            elif recency_date == count_date:
                release_date_confidence = "📊 Medium Confidence (Match between Recency and Count)"
                final_earnings_release_date = likely_earnings_release_date_based_on_recency  # Store best available match

            # Low Confidence: Recency & Count are different, but at least one is valid
            elif recency_date > most_likely_quarter_date or count_date > most_likely_quarter_date:
                release_date_confidence = "⚠️ Low Confidence (Release Date After Quarter, But No Match)"
                final_earnings_release_date = likely_earnings_release_date_based_on_recency if recency_date > most_likely_quarter_date else likely_earnings_release_date_based_on_count

    else:
        print("⚠️ No high-confidence quarter available for date comparison.")

    #  Return Final Structured Data
    structured_data = (
        ticker,
        report_type,
        filing_url,
        final_quarter,  
        quarter_confidence,
        final_earnings_release_date,  
        release_date_confidence
    )

    print(f"{quarter_confidence}: {structured_data}")
    print(f"{release_date_confidence}: {structured_data}")

    return structured_data



def extract_earnings_sentences(text):
    """Extracts relevant sentences containing earnings, quarter, and date mentions with extra context."""

    # Core regex pattern to match earnings, quarters, and dates in the same sentence
    pattern = (
        r'([^.]*?'  # Start capturing from sentence start
        r'\b(financial results|earnings\s+(?:report|release|statement)|quarterly\s+results|issued\s+(?:a|an)\s+(?:press|news)\s+release\s+reporting)\b'  # Expanded earnings phrases
        r'.*?'  # Allow any characters in between
        r'(?:\b(?:January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}\b.*?)?'  # Allow Date First or Last
        r'\b(?:first|second|third|fourth|Q[1-4]|fiscal)?\s*(?:quarter\s+(?:ended)?)\b'  # Handle "quarter ended" and "fiscal quarter ended"
        r'.*?'  # Allow any characters in between
        r'(?:\b(?:January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}\b)?'  # Optional date again
        r'[^.]*\.)'  # Capture until the end of the sentence
    )

    matches = list(re.finditer(pattern, text, re.IGNORECASE))
    extracted_sentences = []

    # Quarter Aliases (for normalizing "Q1" → "First Quarter")
    quarter_aliases = {
        "q1": "first quarter",
        "q2": "second quarter",
        "q3": "third quarter",
        "q4": "fourth quarter"
    }

    # Standard Quarter-End Dates
    quarter_end_dates = {
        "first quarter": "March 31",
        "second quarter": "June 30",
        "third quarter": "September 30",
        "fourth quarter": "December 31"
    }


    reporting_date = None
    quarter_name = None
    quarter_end_date = None

    for match in matches:
        start, end = match.start(), match.end()  # Get match positions

        # Extract extra context (before & after)
        extra_chars = 250
        extended_start = max(0, start - extra_chars)
        extended_end = min(len(text), end + extra_chars)
        extended_sentence = text[extended_start:extended_end]
        extracted_sentences.append(extended_sentence)

    #  Step 1: Find All Dates in the Extracted Sentences
    date_pattern = r'\b(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)\.?\s+\d{1,2},\s+\d{4}\b'

    cleaned_text = " ".join(extracted_sentences).replace("\xa0", " ")  # Normalize non-breaking spaces
    date_matches = re.findall(date_pattern, cleaned_text)  # Search cleaned text

    # date_matches = re.findall(date_pattern, " ".join(extracted_sentences))  # Use old working method

    if date_matches:
        # Separate dates into Quarter-End vs. Reporting
        quarter_related_dates = [date for date in date_matches if any(q in date for q in quarter_end_dates.values())]
        filtered_dates = [date for date in date_matches if date not in quarter_related_dates]

        # Find the most recent reporting date
        reporting_date = max(filtered_dates, key=lambda d: d.split(", ")[-1]) if filtered_dates else "Unknown"

        # Find the most recent quarter-end date
        quarter_end_date = max(quarter_related_dates,
                               key=lambda d: d.split(", ")[-1]) if quarter_related_dates else "Unknown"

    # Step 2: Find Quarter & Quarter-End Date
    quarter_pattern = r'\b(first|second|third|fourth|Q[1-4])\s+quarter\b'
    quarter_match = re.search(quarter_pattern, " ".join(extracted_sentences), re.IGNORECASE)

    if quarter_match:
        quarter_name = quarter_match.group(1).lower()  # Get matched quarter
        quarter_name = quarter_aliases.get(quarter_name, quarter_name)  # Normalize "Q1" → "First Quarter"

    return {
        "sentences": extracted_sentences if extracted_sentences else None,
        "reporting_date": reporting_date if reporting_date else "Unknown",
        "quarter_name": quarter_name if quarter_name else "Unknown",
        "quarter_end_date": quarter_end_date if quarter_end_date else "Unknown"
    }


def sanity_check_earnings_dates(df):
    """Ensures there are exactly 4 earnings reports per year, identifies missing ones,
       and tracks missing reports starting from 2020."""

    #  Convert extracted dates to YYYY-MM-DD format
    df["Extracted Reporting Date"] = df["Extracted Reporting Date"].replace("Unknown", pd.NA)
    df["Formatted Reporting Date"] = pd.to_datetime(df["Extracted Reporting Date"], errors="coerce")


    # Drop duplicate reports based on formatted date
    df = df.drop_duplicates(subset=["Formatted Reporting Date"]).reset_index(drop=True)

    # Group by Year & Extract Available Quarters
    df["Year"] = df["Formatted Reporting Date"].dt.year
    df["Quarter From Date"] = df["Formatted Reporting Date"].dt.quarter  # Extract quarter from date

    expected_quarters = {1, 2, 3, 4}  # Every year should have 4 reports
    missing_reports = {}
    current_year = datetime.now().year
    issue_found = False  # Flag for missing reports since 2020

    for year, group in df.groupby("Year"):
        reported_quarters = set(group["Quarter From Date"].dropna().astype(int))  # Get reported quarters
        missing_quarters = expected_quarters - reported_quarters  # Find missing ones

        #  Ignore missing quarters for the current year (e.g., 2025)
        if year == current_year:
            missing_reports[year] = "🕒 Year in Progress"
        elif missing_quarters:
            missing_reports[year] = f"⚠️ Missing Quarters: {sorted(missing_quarters)}"
            #  Only flag issues if year >= 2020
            if year >= 2020:
                issue_found = True
        else:
            missing_reports[year] = "✅ All Reports Present"

    #  Map Missing Reports to DataFrame
    df["Sanity Check"] = df["Year"].map(missing_reports)

    return df, issue_found  # Return issue flag for CSV naming


# Input file with tickers
INPUT_FILE = "tickers.csv"
SUMMARY_FILE = "Earnings_Summary.csv"

# Load tickers from CSV
tickers_df = pd.read_csv(INPUT_FILE)
tickers = tickers_df["Ticker"].unique()  # Ensure unique tickers

# Track issues for reporting
issue_log = []

# Initialize Chrome Driver Before Loop
driver = initialize_driver(chrome_user_data_dir, chrome_profile, download_dir)

for ticker in tickers:
    print(f"🚀 Processing {ticker}...")

    start_time = time.time()

    try:
        # Step 1: Use Selenium to navigate to the 8-K Filings
        get_8k_filings_selenium(ticker)

        print(f"1: Got 8K Filings")

        # Step 2: Collect all 8-K filings links and dates
        all_filings = collect_8k_links(driver)
        print(f"2: Got all Filings")

        # Step 3: Extract structured earnings data for all filings
        extracted_data = [
            get_earnings_data_selenium_soup(driver, filing[1], ticker) for filing in all_filings
        ]

        # Filter out None values
        extracted_data = [data for data in extracted_data if data]
        print(f"3: Extracted Data")

        # Step 4: Determine confidence level for each extracted report
        structured_earnings_data = [determine_confidence_level(data) for data in extracted_data]

        # Step 5: Extract earnings sentences & integrate them into `structured_earnings_data`
        earnings_sentences_data = [extract_earnings_sentences(data["visible_text"]) for data in extracted_data]

        # Step 6: Merge extracted earnings sentence data with structured_earnings_data
        final_data = []
        for structured_entry, sentence_entry in zip(structured_earnings_data, earnings_sentences_data):
            final_data.append(structured_entry + (
                sentence_entry["sentences"],
                sentence_entry["reporting_date"],
                sentence_entry["quarter_name"],
                sentence_entry["quarter_end_date"]
            ))

        # Step 7: Convert final structured data into a Pandas DataFrame
        df = pd.DataFrame(
            final_data,
            columns=[
                "Ticker", "Earnings Type", "Filing Link", "Quarter", "Quarter Confidence",
                "Date", "Date Confidence",
                "Earnings Sentences", "Extracted Reporting Date",
                "Extracted Quarter", "Extracted Quarter-End Date"
            ]
        )

        # Step 8: Perform sanity check on earnings dates
        df, issue_found = sanity_check_earnings_dates(df)

        # Adjust CSV filename if reports are missing since 2020
        csv_filename = f"{ticker}_Final_Earnings{'_Issues' if issue_found else ''}.csv"

        # Save final results
        df.to_csv(csv_filename, index=False)

        # Track issues for summary
        issue_log.append([ticker, "⚠️ Issues Found" if issue_found else "✅ All Good"])

        end_time = time.time()
        print(f"✅ {ticker} earnings data saved as: {csv_filename} ({end_time - start_time:.2f} sec)")

    except Exception as e:
        print(f"❌ Error processing {ticker}: {e}")
        issue_log.append([ticker, "❌ Processing Error"])
        continue  # Move to the next ticker

# Close the Chrome Driver After Processing All Tickers
driver.quit()

# Save issue summary to CSV
summary_df = pd.DataFrame(issue_log, columns=["Ticker", "Status"])
summary_df.to_csv(SUMMARY_FILE, index=False)

print(f"Issue summary saved as: {SUMMARY_FILE}")





# import os
# import time
#
# # Define the delay in seconds (x hours)
# delay = .00005 * 60 * 60
#
# # Wait for the specified delay
# time.sleep(delay)
#
# # Execute the shutdown command
# os.system("shutdown /s /f /t 0")