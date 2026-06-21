// src/pages/Unity.jsx

import { Link } from "react-router-dom";

import styles from "./Unity.module.css"; // Bring back the module layout helper

const unityProjects = [
  {
    title: "Story: Why I didn't complete anything",
    subtitle: "Start here",
    techStack: ["C#", "Unity"],
    description:
      "The reason is pretty straight-forward. Read more about my musings here",
    slug: "Epilogue",
  },
  {
    title: "Entity Physics",
    subtitle: "Custom Physics Engine",
    techStack: ["C#", "Unity"],
    description:
      " As far as looking back on code goes, I don't think it's that bad. Maybe, if I decide to try my hand on 2D Side-Scrolling again I can re-use it.",
    slug: "EntityPhysics",
  },
  {
    title: "Inventory System",
    subtitle: "The Inventory Management system",
    techStack: ["C#", "Unity", "JSON"],
    description: "Every game needs an Inventory system.",
    slug: "Inventory",
  },
  {
    title: "Multiplayer Abyss",
    subtitle: "The Abyss",
    techStack: ["C#", "Unity", "Mirror"],
    description: "Get me out. Please.",
    slug: "Multiplayer",
  },
];

function Unity() {
  return (
    <main className={styles.fullBleedCanvas}>
      <div className="gridContainer">
        <div className={`pageHeaderBanner ${styles.odinHeaderTheme}`}>
          <div>
            <h2>Unity Stories</h2>
            <p>
              A selection of scripts I've written for some game projects I'm
              definitely going to finish some day.
            </p>
            <p>Trust me!</p>
          </div>
        </div>

        <div className="projectsGrid">
          {unityProjects.map((project) => (
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
                <Link to={`/unity/${project.slug}`} className="btn btnPrimary">
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

export default Unity;
