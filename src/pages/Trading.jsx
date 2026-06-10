// src/pages/Trading.jsx

import { Link } from "react-router-dom";

import styles from "./Trading.module.css"; // Bring back the module layout helper

const tradingProjects = [
  {
    title: "Abandoned_Earnings_Fetcher",
    subtitle: "Scraping the SEC",
    techStack: ["Python", "Selenium", "Pandas", "BeautifulSoup"],
    description:
      "A script I wrote to get earnings data directly from the SEC. There's a reason why it's abandoned.",
    slug: "Abandoned_Earnings_Fetcher",
  },
  {
    title: "Data_Analysis",
    subtitle: "Data into 3D Charts",
    techStack: ["Python", "Pandas", "Matplotlib", "KFold", "Seaborn"],
    description:
      "Too many moving parts and too much data lead to a need to analyze it a bit more efficiently",
    slug: "Data_Analysis",
  },
  {
    title: "Pinescript Utilities",
    subtitle: "Handy Stuff for Tradingviewers",
    techStack: ["Pinescript"],
    description: "A small Repo containing some of my Pinescript Tools",
    slug: "Pinescript_Utilities",
  },
  {
    title: "Pinescript FOX",
    subtitle: "Codifying someone else's strategy",
    techStack: ["Pinescript"],
    description:
      "A script I wrote after I saw a video of a strategy that looked amazing",
    slug: "FOX",
  },
  {
    title: "Pine / Python Volume Burst",
    subtitle: "",
    techStack: [""],
    description: "",
    slug: "",
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
            <p>Feel free to give them a read! </p>
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
