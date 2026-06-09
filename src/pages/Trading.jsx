// src/pages/Trading.jsx

import React, { useState } from "react";
import ReadmeModal from "../components/GitHubReadmeModal";
import { Link, useLocation } from "react-router-dom";

import styles from "./Trading.module.css"; // Bring back the module layout helper

const tradingProjects = [
  {
    title: "Simple Input File Test",
    subtitle: "Scraping the SEC",
    techStack: ["Python", "Selenium", "Pandas", "BeautifulSoup"],
    description:
      "A script I wrote to get earnings data directly from the SEC. It wasn't exactly worth the effort.",
    slug: "Abandoned_Earnings_Fetcher", // 1. This matches your "src/content/trading/simple-input-test.md" filename exactly
  },
  {
    title: "Placeholder",
    subtitle: "",
    techStack: [""],
    description: "",
    liveUrl: "",
    slug: "",
  },
];

function Trading() {
  return (
    <main className={styles.fullBleedCanvas}>
      {/* 2. This locks your text and cards into your 1200px centered grid */}
      <div className="pageContainer">
        <div className={`pageHeaderBanner ${styles.odinHeaderTheme}`}>
          <div>
            <h2>Trading Projects</h2>
            <p>
              A small selection of scripts I've built for my Trading Ventures.
            </p>
            <p>These include some of the first scripts I've written. </p>
            <p>So... Don't expect a get rich quick scheme here!</p>
          </div>
        </div>

        <div className="projectsGrid">
          {tradingProjects.map((project) => (
            <article
              key={project.title}
              className={`projectCard ${styles.mdfCardTheme}`}
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
                  Read some Thoughts
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
