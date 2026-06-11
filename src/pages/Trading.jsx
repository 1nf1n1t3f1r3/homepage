// src/pages/Trading.jsx

import { Link } from "react-router-dom";

import styles from "./Trading.module.css"; // Bring back the module layout helper

const tradingProjects = [
  {
    title: "Strategy: Band Breakout Strategies",
    subtitle: "Heavy Lifting with Simple Strategies",
    techStack: ["Python", "pandas", "numpy", "yfinance"],
    description:
      "Simple Long-Term Investing Stratgies with Bollingers and/or Keltners",
    slug: "Band_Breakout_Strategies",
  },
  {
    title: "Strategy: Pine / Python Volume Burst",
    subtitle: "Pine to Python",
    techStack: ["Pinescript", "Python", "pandas", "numpy", "yfinance"],
    description:
      "First, you write it in Pine to see if it looks like it makes sense. Then you decide you need moar data, so you turn to Python.",
    slug: "Volume_Burst",
  },
  {
    title: "Strategy: Pinescript FOX",
    subtitle: "Codifying someone else's strategy",
    techStack: ["Pinescript"],
    description:
      "A script I wrote after I saw a video of a strategy that looked amazing",
    slug: "FOX",
  },
  {
    title: "Utilities: Data Analysis",
    subtitle: "Wrangling Data into 3D Charts",
    techStack: ["Python", "pandas", "Matplotlib", "KFold", "Seaborn"],
    description:
      "Too many moving parts and too much data lead to a need to analyze it a bit more efficiently",
    slug: "Data_Analysis",
  },
  {
    title: "Utilities: Pinescript Tools",
    subtitle: "Handy Stuff for Tradingviewers",
    techStack: ["Pinescript"],
    description: "A Minirepo containing some of my Pinescript Tools",
    slug: "Pinescript_Utilities",
  },
  {
    title: "Utilities: Abandoned Earnings Fetcher",
    subtitle: "Scraping the SEC",
    techStack: ["Python", "Selenium", "pandas", "BeautifulSoup"],
    description:
      "A script I wrote to get earnings data directly from the SEC and the reason why it's abandoned.",
    slug: "Abandoned_Earnings_Fetcher",
  },
];

function Trading() {
  return (
    <main className={styles.fullBleedCanvas}>
      <div className="gridContainer">
        <div className={`pageHeaderBanner ${styles.odinHeaderTheme}`}>
          <div>
            <h2>Trading Stories</h2>
            <p>A selection of scripts I've written for my trading.</p>
            <p>With accompanying stories as to how they got there.</p>
            <p>None of these are intended as stand-alone financial advice!</p>
          </div>
        </div>

        <div className="projectsGrid">
          {tradingProjects.map((project) => (
            <article
              key={project.title}
              className={`projectCard ${styles.tradingCardTheme}`}
            >
              <div className="cardHeader">
                <h3>{project.title}</h3>
                <span className="projectSubtitle">{project.subtitle}</span>
              </div>

              <div className="techBadgeContainer">
                {project.techStack.map(
                  (tech) =>
                    /* Only render a badge if tech is not an empty string */
                    tech && (
                      <span key={tech} className="techBadge">
                        {tech}
                      </span>
                    ),
                )}
              </div>

              <p className="projectDescription">{project.description}</p>

              <div className="cardActions">
                <Link
                  to={`/trading/${project.slug}`}
                  className="btn btnPrimary"
                >
                  Tell me more!
                </Link>
              </div>
            </article>
          ))}
        </div>
      </div>
    </main>
  );
}

export default Trading;
