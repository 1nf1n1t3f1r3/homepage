// src/pages/Home.jsx
import React from "react";
import styles from "./Home.module.css";
import { Link } from "react-router-dom";

function Home() {
  return (
    <main className="page-content">
      <div className="gridContainer">
        {/* 1. HERO BANNER HEADER */}
        <div className="pageHeaderBanner">
          <h2>Janus de Vries</h2>
          <p>Full-Stack Developer | Trader | Fencer | Dungeon Master</p>
        </div>

        {/* 2. TWO-COLUMN SPLIT RESUME GRID */}
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

          {/* RIGHT COLUMN: Professional Chronicle */}
          <section className={styles.mainChronicleColumn}>
            <div className={styles.chronicleCard}>
              <h3>Experience & Major Projects</h3>

              <div className={styles.timelineItem}>
                <div className={styles.timelineHeader}>
                  <h4>Algorithmic Trading & Automation</h4>
                  <span className={styles.timelineDate}>Present</span>
                </div>
                <p>
                  Engineered custom fundamental scraping pipelines parsing
                  financial filings directly from the SEC EDGAR network.
                  Architected secure webhook-driven order execution frameworks.
                </p>
              </div>

              <div className={styles.timelineItem}>
                <div className={styles.timelineHeader}>
                  <h4>Web Deployment Masterclass</h4>
                  <span className={styles.timelineDate}>Ongoing</span>
                </div>
                <p>
                  Maintained and launched custom responsive portal designs
                  including the Masque de Fer fencing association platform and
                  complex campaign wiki networks utilizing Obsidian and Quartz
                  deployment loops.
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
