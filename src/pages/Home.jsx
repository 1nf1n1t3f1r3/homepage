// src/pages/Home.jsx
import React from "react";
import styles from "./Home.module.css";
import { Link } from "react-router-dom";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faEarthEurope,
  faEnvelopeOpen,
} from "@fortawesome/free-solid-svg-icons";

import { faGithub, faLinkedin } from "@fortawesome/free-brands-svg-icons";

function Home() {
  return (
    <main className="page-content">
      <div className="gridContainer">
        {/* HERO BANNER HEADER */}
        <div className={styles.heroGreetingBlock}>
          <h1>Greetings, Traveller! I'm Janus.</h1>
          <p className={styles.heroSubheading}>
            I'm a full-stack web developer who thrives on breaking down complex
            problems into (mostly) clean, modular systems.
          </p>
          <p className={styles.heroStatusText}>
            Whether I'm engineering algorithmic trading pipelines, designing
            responsive web applications, or refactoring game state loops, I
            build software with intent. Currently looking for my next challenge
            in web development.
          </p>
          <br />
          <p className={styles.heroStatusText}>
            Press one of the buttons to check out some Websites, or scroll down
            to read my CV!
          </p>
        </div>
        <h3 className={styles.sectionTitle}>Explore My Works</h3>
        {/* Portal Section */}
        <section className={styles.portalSection}>
          <div className={styles.projectsGrid}>
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
                  Websites I've put online
                </span>
              </div>
              <p className="projectDescription">
                Live websites, including fun things like my fencing club, D&D
                Campaign and the one you're on right now!{" "}
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
                <span className="projectSubtitle">
                  Full-Stack Learning Projects
                </span>
              </div>
              <p className="projectDescription">
                A collection of capstone Odin Project applications, like
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
                Post-mortems and technical breakdowns of C# scripts I wrote to
                handle Physics, Inventory, Multiplayer and more.
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
              <h3>Elevator Pitch</h3>
              <p className={styles.bioText}>
                I build clean web interfaces, trading scripts and Unity
                projects. I deploy single-page applications or set up
                programmatic scraping pipelines. I like solving hard problems
                and copy-pasting the same code block in three different places
                in equal measure.
              </p>
            </div>

            {/* Contact Details Box */}
            <div className={styles.profileSection}>
              <h3>Contact Info</h3>
              <ul className={styles.contactList}>
                <li>
                  <a
                    href="https://www.google.com/maps/search/?api=1&query=Papendrecht,+Netherlands"
                    target="_blank"
                    rel="noreferrer"
                  >
                    <FontAwesomeIcon
                      icon={faEarthEurope}
                      className={styles.contactIcon}
                    />
                    <strong> Papendrecht, Netherlands</strong>
                  </a>
                </li>
                <li>
                  <a href="mailto:jdv97@outlook.com">
                    <strong>
                      {" "}
                      <FontAwesomeIcon
                        icon={faEnvelopeOpen}
                        className={styles.contactIcon}
                      />{" "}
                      Send an Email
                    </strong>
                  </a>
                </li>
                <li>
                  <a href="https://github.com/1nf1n1t3f1r3/">
                    {" "}
                    <strong>
                      {" "}
                      <FontAwesomeIcon
                        icon={faGithub}
                        className={styles.contactIcon}
                      />{" "}
                      Visit my Github
                    </strong>
                  </a>
                </li>
                {/* <li>
                  <FontAwesomeIcon
                    icon={faLinkedin}
                    className={styles.contactIcon}
                  />
                  <a href="https://github.com/1nf1n1t3f1r3/">
                    {" "}
                    <strong> Visit my LinkedIn</strong>
                  </a>
                </li> */}
              </ul>
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

          {/* RIGHT COLUMN: Curriculum Vitae Main Data */}
          <section className={styles.mainChronicleColumn}>
            <div className={styles.chronicleCard}>
              <div className={styles.cvHeaderBlock}>
                <h3>Curriculum Vitae</h3>
              </div>

              <p className={styles.bioText} style={{ marginBottom: "30px" }}>
                Analytical Full-Stack Developer with a strong foundation in Ruby
                on Rails, React, and Python. Combines a Master’s degree in
                Philosophy (focused on logic and complex problem-solving) with
                years of experience in data-driven retail trading and
                automation. Proven ability to build, deploy, and maintain robust
                web applications independently through rigorous self-directed
                training.
              </p>

              {/* Section: Experience */}
              <div className={styles.cvSection}>
                <h4 className={styles.cvSectionTitle}>Experience</h4>

                <div className={styles.cvItem}>
                  <div className={styles.cvItemHeader}>
                    <strong>Retail Trader</strong>
                    <span className={styles.cvDate}>2018 – Present</span>
                  </div>
                  <span className={styles.cvCompany}>Self-Employed</span>
                  <ul className={styles.cvBulletList}>
                    <li>
                      Analyzed market data and executed financial strategies,
                      transitioning from short-term trading to long-term
                      investing.
                    </li>
                    <li>
                      Developed automated custom technical indicators and
                      algorithmic trading scripts using Python and Pine Script
                      to optimize data analysis.
                    </li>
                  </ul>
                </div>
              </div>

              {/* Section: Education & Training */}
              <div className={styles.cvSection}>
                <h4 className={styles.cvSectionTitle}>Education & Training</h4>

                {/* The Odin Project */}
                <div className={styles.cvItem}>
                  <div className={styles.cvItemHeader}>
                    <strong>Full-Stack Ruby on Rails</strong>
                    <span className={styles.cvDate}>Sept 2025 – June 2026</span>
                  </div>
                  <span className={styles.cvCompany}>The Odin Project</span>
                  <p className={styles.cvDescription}>
                    An intensive, project-based curriculum covering computer
                    science fundamentals, Git workflows, test-driven development
                    (TDD), and deploying production-ready applications using
                    Ruby on Rails and React.
                  </p>
                </div>

                {/* Master's */}
                <div className={styles.cvItem}>
                  <div className={styles.cvItemHeader}>
                    <strong>M.A. Philosophy of Contemporary Challenges</strong>
                    <span className={styles.cvDate}>2020 – 2021</span>
                  </div>
                  <span className={styles.cvCompany}>Tilburg University</span>
                  <p className={styles.cvDescription}>
                    Focused on advanced logic, critical thinking, and ethics.
                    Completed 30 extra credits mainly from business ethics.
                    <br />
                    <em>
                      Thesis: "The Populist Prince" (Analysis of modern populism
                      through Machiavelli).
                    </em>
                  </p>
                </div>

                {/* Bachelor's */}
                <div className={styles.cvItem}>
                  <div className={styles.cvItemHeader}>
                    <strong>
                      B.A. Liberal Arts and Sciences (Humanities Major)
                    </strong>
                    <span className={styles.cvDate}>2015 – 2018</span>
                  </div>
                  <span className={styles.cvCompany}>Tilburg University</span>
                  <p className={styles.cvDescription}>
                    Multidisciplinary program emphasizing research
                    methodologies, structured argumentation, and system
                    analysis.
                  </p>
                </div>
              </div>

              <h4 className={styles.cvSectionTitle}>Download as PDF</h4>
              <a href="/downloads/CV.pdf" download className="btn btnSecondary">
                📄 Download PDF CV
              </a>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}

export default Home;
