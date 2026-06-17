// src/pages/Home.jsx
import React from "react";
import styles from "./Home.module.css";
import { Link } from "react-router-dom";

function Home() {
  return (
    <main className="page-content">
      <div className="gridContainer">
        <div className="pageHeaderBanner">
          <h2>Janus de Vries</h2>
          <p>Full-Stack Developer | Trader | Fencer | Dungeon Master</p>
          <p>Currently looking for a Job in Web Development</p>
        </div>
        <h3 className={styles.sectionTitle}>Explore My Works</h3>
        {/* Portal Section */}
        <section className={styles.portalSection}>
          <div className="projectsGrid">
            {/* Trading Stories */}
            <div
              className="projectCard"
              style={{
                "--card-bg-override": "var(--bg-card)",
                "--card-border-override": "var(--card-trading-border)",
              }}
            >
              <div className="cardHeader">
                <h3>Trading Chronicles</h3>
                <span
                  className="projectSubtitle"
                  style={{ color: "var(--accent-trading)" }}
                >
                  Python, Pinescript & Automation
                </span>
              </div>
              <p className="projectDescription">
                Deep dives into trading strategies, data analysis, Pinescript
                and scraping SEC EDGAR records.
              </p>
              <div className="cardActions">
                <Link to="/trading" className="btn btnPrimary">
                  Read Trading Stories →
                </Link>
              </div>
            </div>

            {/* Web Showcases */}
            <div
              className="projectCard"
              style={{
                "--card-bg-override": "var(--bg-card)",
                "--card-border-override": "var(--card-mdf-border)",
              }}
            >
              <div className="cardHeader">
                <h3>Web Showcases</h3>
                <span
                  className="projectSubtitle"
                  style={{ color: "var(--card-mdf-border)" }}
                >
                  Production Sites
                </span>
              </div>
              <p className="projectDescription">
                Live websites, including fun things for my fencing club, D&D
                Campaign, and, of course, this one{" "}
              </p>
              <div className="cardActions">
                <Link to="/mywebsites" className="btn btnPrimary">
                  Visit Websites →
                </Link>
              </div>
            </div>

            {/* Odin Projects */}
            <div
              className="projectCard"
              style={{
                "--card-bg-override": "var(--bg-card)",
                "--card-border-override": "var(--accent-odin)",
              }}
            >
              <div className="cardHeader">
                <h3>Odin Hub</h3>
                <span className="projectSubtitle">Full-Stack Sandboxes</span>
              </div>
              <p className="projectDescription">
                A collection of captone Odin Project applications, like
                Odinbook, Where's Waldo and Pokemon Memory.
              </p>
              <div className="cardActions">
                <Link to="/odin" className="btn btnPrimary">
                  View Odin Apps →
                </Link>
              </div>
            </div>

            <div
              className="projectCard"
              style={{
                "--card-bg-override": "#1c1b22",
                "--card-border-override": "#6a5acd",
              }}
            >
              <div className="cardHeader">
                <h3>Unity Chronicles</h3>
                <span className="projectSubtitle" style={{ color: "#9370db" }}>
                  C# & Game Mechanics
                </span>
              </div>
              <p className="projectDescription">
                Post-mortems and technical breakdowns of interactive
                environments, structural game loops, C# state machines, and
                physics optimizations.
              </p>
              <div className="cardActions">
                <Link to="/unity" className="btn btnPrimary">
                  Explore Game Dev →
                </Link>
              </div>
            </div>
          </div>
        </section>

        {/* 2. TWO-COLUMN SPLIT RESUME GRID */}
        <h3 className={styles.sectionTitle}>About Me</h3>
        <div className={styles.resumeLayout}>
          {/* LEFT SIDEBAR: Personal Details & Core Tech Stack */}
          <section className={styles.sidebarColumn}>
            <div className={styles.profileSection}>
              <h3>About Me</h3>
              <p className={styles.bioText}>
                I build clean, high-performance web interfaces and robust
                automation networks. Whether it's deploying single-page
                applications or setting up programmatic scraping pipelines, I
                like solving the hard synchronization problems behind the
                scenes.
              </p>
            </div>

            <div className={styles.stackSection}>
              <h3>Technical Arsenal</h3>
              <div className="techBadgeContainer">
                <span className="techBadge">Git & GitHub</span>

                <span className="techBadge">HTML5</span>
                <span className="techBadge">CSS3</span>
                <span className="techBadge">JavaScript (ES6+)</span>
                <span className="techBadge">Node.js</span>

                <span className="techBadge">React</span>
                <span className="techBadge">Vite</span>

                <span className="techBadge">Python</span>
                <span className="techBadge">C#</span>

                <span className="techBadge">Whatever's Next!</span>
              </div>
            </div>
          </section>

          {/* RIGHT COLUMN: Engineering Focal Points */}
          <section className={styles.mainChronicleColumn}>
            <div className={styles.chronicleCard}>
              <h3>Engineering Core & Focus Areas</h3>

              {/* Focal Point 1: Data Infrastructure */}
              <div className={styles.timelineItem}>
                <div className={styles.timelineHeader}>
                  <h4>Data Pipelines & Financial Automation</h4>
                  <span className={styles.timelineDate}>Python / Node.js</span>
                </div>
                <p>
                  Architected custom automated scrapers to parse raw fundamental
                  financial data straight from the SEC EDGAR network. Engineered
                  secure, low-latency webhook systems and execution handlers to
                  pipe live signals directly into broker APIs.
                </p>
              </div>

              {/* Focal Point 2: Modern Web Architecture */}
              <div className={styles.timelineItem}>
                <div className={styles.timelineHeader}>
                  <h4>Full-Stack Component Systems</h4>
                  <span className={styles.timelineDate}>
                    React / JavaScript
                  </span>
                </div>
                <p>
                  Building optimized, responsive single-page applications
                  focused on data isolation and component lifecycle management.
                  Experienced in implementing state-driven UI layouts, custom
                  asynchronous modal architectures, and fixing deep
                  browser-rendering glitches using advanced React hooks like{" "}
                  <code>useLayoutEffect</code>.
                </p>
              </div>

              {/* Focal Point 3: Static Site Pipelines */}
              <div className={styles.timelineItem}>
                <div className={styles.timelineHeader}>
                  <h4>Static Site Engineering & Markdown Parsers</h4>
                  <span className={styles.timelineDate}>
                    Quartz / Production
                  </span>
                </div>
                <p>
                  Deployed production-ready association platforms alongside
                  complex, interconnected static knowledge networks. Built local
                  file interceptors using Vite glob loaders to read raw Markdown
                  files client-side, bypassing GitHub API public rate limits to
                  cleanly render content dynamically through{" "}
                  <code>rehype-raw</code> abstract syntax trees.
                </p>
              </div>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}

export default Home;
