# Scraping the SEC

This is a script I wrote in order to find earnings dates. I couldn't find them consistently on Yahoo Finance, and Tradingview also doesn't display earnings dates when you go back too far. So... I figured I'd get them from the source at the SEC. That was a completely unhinged venture. It's been a while since I actually wrote this thing and I wasn't too big on documenting anything I did back then. Feel free to read all about it, or skip to the bottom for a better solution.

## Earnings

When trading stocks, you invariably run into the issue of Earnings Reports being released when you're interested in a trade. The question is, how do you handle those? The risk with these is that the company might report something that's unexpected, in which case the stock might respond with extreme volatility. This can, of course, be in your favour, but it also could go against you, where you enter a position, earnings are released overnight, and the stock is now trading far beyond your Stop Loss (Where you wanted to exit the trade, if it didn't go your way), leading to a bigger-than-expected loss.

## The Solution?

Well, there's two ways to handle these reports. The first is that you simply ignore them (or don't know they exist at all), and you enter or maintain a position in front of earnings. The argument in favour of this approach is that, there's a reason the setup looks good. Earnings are likely to lead to a more volatile outcome, which means, if the trade is already good anyway, then a more volatile outcome, where the outcome is more likely than not to be in your favour, just means bigger profits! The second approach is that you close your position before earnings are released, or don't enter one at all, if earnings are coming up soon. The reason is simple; if you're a serious trader, carefully weighing risks and balancing trades so that individual wins and losses don't have an outsized effect on the bottom line, why risk that by playing Report Roulette?

## Scraping Script

In order to decide which approach is better, we would first need to know when these earnings reports were actually dropped. Then look at our live and/or backtested trades in order to see if there's a skew in any direction. Data on earnings isn't always too easy to come by in a clean format (at least, that's what I thought, more on that later). However, they all need to share that information with the SEC, so... That's where we'll find it for sure! Then we just need to go and get it, right? That brings us to the scraping.

### get_8k_filings_selenium & collect-8k_links

I wrote a script that controls the browser in order to do that. It takes a .csv input file, then goes opens up Google Chrome with Selenium to go the website of the SEC. When there, it navigates the website by finding HTML elements then 'clicking' them. It 'types' the tickers of the input file, then selects the ticker with the Arrow Keys and Return button. At this point, it's looking at all the SEC reports of one company (say, AAPL). But, companies release a lot of reports, so we still need to get the right ones. The so-called 8-K reports are the ones to search for, so it goes to the filter menu on the SEC site and filters only by them. Then it uses the Python library BeautifulSoup to start fetching all of those.

### get_earnings_data_selenium_soup

So, now we're looking at reports. But, who has the time to read these things. We spend our time on more reasonable things, like re-inventing wheels and writing hacky code. So, this is where the script looks through the page with the html.parser and looks at the text, it'll find recurring phrases, like 'Q1' and 'first quarter'; not all reports use the same exact phrasing, which is particularly great when it comes to deciding dates. It takes a lot of hacking around to try to find the right one, since there's more than one date per report. Still, when this function is done, it should return a ticker, report type, URL, and a list of dates, making a guess as to which one is the actual report date. For example, the financial quarter ends on March 31, so that's not the earnings date, but a date from a phrase like " ... for the quarter ending March 31."

### determine_confidence_level

A small hint, the confidence level should be zero. But, why stop there? In order to analyze the messy data that comes out of the previous function, I figured I'd write another function to determine a 'confidence level'. The idea was that it'd check all the dates and test them based on, aspects like how often they occur. Rather poor metrics, but I couldn't really come up with anything better.

### extract_earnings_sentences

This is the bit that actually extracts the text that is about the reporting date. It works with regex using terms like 'report|release|statement|earnings' etc. And it takes some extra context as well.

### sanity_check_earnings_dates

I wonder what sanity I was checking. Anwyay, this step is there to drop duplicate reports. If there's an issue, it the ticker gets flagged.

## Pinescript to the Rescue

So.. It turns out that Tradingview _does_ actually store the earnings data. It just doesn't display it. But, it's easy to ask it to with Pinescript. Then you can get this data in .csv format by Exporting it. The more you know.

```
//@version=5
indicator("Earnings Markers with Dates", overlay=true)

// Request earnings for the current stock
earnings = request.earnings(syminfo.tickerid, earnings.actual, gaps=barmerge.gaps_on, lookahead=barmerge.lookahead_on)

// Check if there's an earnings report on this bar
isEarningsDay = not na(earnings)

// Extract date components from the current bar
earningsDate = str.format("Earnings: {0}-{1}-{2}", year(time), month(time), dayofmonth(time))

// Add a label with the earnings date
if isEarningsDay
    label.new(bar_index, low, earningsDate, color=color.blue, textcolor=color.white, size=size.small)

// Plot earnings markers
plotshape(isEarningsDay, title='Is_Earnings_Day', location=location.belowbar, style=shape.triangleup, color=color.blue, size=size.small)

```
