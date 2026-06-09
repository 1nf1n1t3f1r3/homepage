// src/pages/MyWebsites.jsx

import React, { useState } from "react";
import ReadmeModal from "../components/GitHubReadmeModal";

import styles from "./MyWebsites.module.css"; // Bring back the module layout helper

const websiteProjects = [
  {
    title: "Masque de Fer Fencing Club Website",
    subtitle: "Plain HTML Website",
    techStack: ["HTML", "CSS", "Javascript"],
    description:
      "A basic HTML+CSS+JS Website for my Fencing Club in Zwijndrecht. I deliberately kept it as simple as possible to allow non-devs to peak under the hood, or write for it. It does show how much work frameworks take off our hands; working with plain HTML doesn't necessarily make it easier. Good thing we can still write Partials in JS. It might be a candidate for a snappy React refactor some day.",
    liveUrl: "https://masquedefer.nl",
    githubUrl: "https://github.com/1nf1n1t3f1r3/masque_de_fer",
    repo: "1nf1n1t3f1r3/masque_de_fer",
    image: "/images/masque_de_fer-preview.png",
  },
  {
    title: "TTRPG Tools",
    subtitle: "Work in Progress",
    techStack: [""],
    description: "",
    liveUrl: "",
    githubUrl: "",
    repo: "",
    image: "",
  },
];

function MyWebsites() {
  // Track which project is actively being viewed in the modal
  const [activeProject, setActiveProject] = useState(null);

  return (
    <main className={styles.fullBleedCanvas}>
      {/* 2. This locks your text and cards into your 1200px centered grid */}
      <div className="gridContainer">
        <div className={`pageHeaderBanner ${styles.odinHeaderTheme}`}>
          <div>
            <h2>Website Showcase</h2>
            <p>Websites I've built and am pretty pleased with.</p>
            <p>All of these can be found live.</p>
          </div>
        </div>

        <div className="projectsGrid">
          {websiteProjects.map((project) => (
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
                  href={project.liveUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btnPrimary"
                >
                  Visit Website ↗
                </a>
                <a
                  href={project.githubUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btnPrimary"
                >
                  Github Repo ↗
                </a>
                <button
                  onClick={() => setActiveProject(project)}
                  className="btn btnSecondary"
                >
                  Github Readme
                </button>
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

export default MyWebsites;
