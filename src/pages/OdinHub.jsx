// src/pages/Odinhub.jsx

import React, { useState } from "react";
import ReadmeModal from "../components/GitHubReadmeModal";
import styles from "./OdinHub.module.css";

const odinProjects = [
  {
    title: "Odinbook",
    subtitle: "Full-Stack Social Media Platform",
    techStack: [
      "Ruby on Rails",
      "PostgreSQL",
      "Tailwind CSS",
      "Pagy",
      "Sendgrid",
    ],
    description:
      "A comprehensive clone of Facebook built as a capstone project for The Odin Project. Features user profiles, following, posts, likes and comments. Feel free to post something; I get pretty lonely in Odinbook-land.",
    liveUrl: "https://odinbook.janusdevries.nl",
    githubUrl: "https://github.com/1nf1n1t3f1r3/odinbook",
    repo: "1nf1n1t3f1r3/odinbook",
    image: "/images/odinbook-preview.png",
    isFreeTier: true,
  },
  {
    title: "Pokémon Memory Game",
    subtitle: "Dynamic State-Management Trainer",
    techStack: ["React", "PokeAPI"],
    description:
      "An interactive memory card game pulling live data from the PokeAPI. Features procedural generation based on custom ID entries or complete regional Pokedex selections from all generations, even though my own familiarity with them becomes very limited after #493: Arceus.",
    liveUrl: "https://pokemon.janusdevries.nl",
    githubUrl: "https://github.com/1nf1n1t3f1r3/odin_memory",
    repo: "1nf1n1t3f1r3/odin_memory",
    image: "/images/odin_memory-preview.png",
    isFreeTier: false,
  },
  {
    title: "Where's Waldo",
    subtitle: "Real-Time Photo Tagging Game",
    techStack: ["React", "Ruby on Rails", "PostgreSQL"],
    description:
      "A time-attack puzzle game where players find the titular Waldo (and friends) on a massive canvas. Quick detectives can enter their names on the high-score leaderboard. Coordinates are validated via the backend to prevent cheating. I hope nobody realizes they can just find the characters, press F5 to reload the page, and then quickly click them all to beat my top score. That'd be devastating.",
    liveUrl: "https://whereswaldo.janusdevries.nl",
    githubUrl: "https://github.com/1nf1n1t3f1r3/odin_wheres_waldo",
    repo: "1nf1n1t3f1r3/odin_wheres_waldo",
    image: "/images/odin_wheres_waldo-preview.png",
    isFreeTier: true,
  },
];

function OdinHub() {
  // Track which project is actively being viewed in the modal
  const [activeProject, setActiveProject] = useState(null);

  return (
    <main className={styles.odinHubContainer}>
      <header className={styles.pageHeader}>
        <h2>The Odin Project Showcase</h2>
        <p className={styles.pageIntro}>
          My favourite projects from The Odin Project curriculum.
        </p>
      </header>

      <div className={styles.projectsGrid}>
        {odinProjects.map((project) => (
          <article key={project.title} className={styles.projectCard}>
            <div className={styles.cardHeader}>
              <h3>{project.title}</h3>
              <span className={styles.projectSubtitle}>{project.subtitle}</span>
            </div>

            <div className={styles.techBadgeContainer}>
              {project.techStack.map((tech) => (
                <span key={tech} className={styles.techBadge}>
                  {tech}
                </span>
              ))}
            </div>

            <p className={styles.projectDescription}>{project.description}</p>

            {project.isFreeTier && (
              <div className={styles.freeTierWarning}>
                ☕ Hosted on a free tier. Please allow a minute for the server
                to wake up on your first click!
              </div>
            )}

            <div className={styles.cardActions}>
              <a
                href={project.liveUrl}
                target="_blank"
                rel="noopener noreferrer"
                className={`${styles.btn} ${styles.btnPrimary}`}
              >
                Launch App ↗
              </a>
              <a
                href={project.githubUrl}
                target="_blank"
                rel="noopener noreferrer"
                className={`${styles.btn} ${styles.btnSecondary}`}
              >
                Github Repo
              </a>
              {/* NEW: Button to trigger the in-app Readme menu */}
              <button
                onClick={() => setActiveProject(project)}
                className={`${styles.btn} ${styles.btnSecondary}`}
              >
                Github Readme
              </button>
            </div>
          </article>
        ))}
      </div>

      {/* The Modal Handler */}
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

export default OdinHub;
