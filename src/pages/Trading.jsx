// src/pages/Trading.jsx

import React, { useState } from "react";
import ReadmeModal from "../components/GitHubReadmeModal";
import { Link, useLocation } from "react-router-dom";

import styles from "./Trading.module.css"; // Bring back the module layout helper

const tradingProjects = [
  {
    title: "Simple Input File Test",
    subtitle: "Tradingview Trade Tester",
    techStack: ["Python", "Tradingview", "Pandas"],
    description: "The oldest trading file I could still find on my system. ",
    githubUrl: "https://github.com/1nf1n1t3f1r3/masque_de_fer",
    repo: "1nf1n1t3f1r3/masque_de_fer",
    image: "/images/masque_de_fer-preview.png",
  },
  {
    title: "Placeholder",
    subtitle: "",
    techStack: [""],
    description: "",
    liveUrl: "",
    githubUrl: "",
    repo: "",
    image: "",
  },
  {
    title: "Placeholder",
    subtitle: "",
    techStack: [""],
    description: "",
    liveUrl: "",
    githubUrl: "",
    repo: "",
    image: "",
  },
];

function Trading() {
  // Track which project is actively being viewed in the modal
  const [activeProject, setActiveProject] = useState(null);

  const location = useLocation();

  // Helper to add an 'active' class to the current page link
  const isActive = (path) =>
    location.pathname === path ? "nav-link active" : "nav-link";

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
                {project.techStack.map((tech) => (
                  <span key={tech} className="techBadge">
                    {tech}
                  </span>
                ))}
              </div>

              <p className="projectDescription">{project.description}</p>

              {project.isFreeTier && (
                <div className="freeTierWarning">
                  ☕ Hosted on a free tier. Please allow a minute for the server
                  to wake up on your first click!
                </div>
              )}

              <div className="cardActions">
                <a
                  href={project.githubUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btnPrimary"
                >
                  Read some Thoughts
                </a>
              </div>
            </article>
          ))}
        </div>
      </div>

      <ReadmeModal
        key={activeProject?.repo || "empty"}
        isOpen={!!activeProject}
        repo={activeProject?.repo}
        projectImage={activeProject?.image}
        onClose={() => setActiveProject(null)}
      />
    </main>
  );
}

export default Trading;
