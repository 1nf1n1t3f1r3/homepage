// src/pages/StoryView.jsx
import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import styles from "./StoryView.module.css";

function StoryView() {
  const { storyId } = useParams();
  const [markdownContent, setMarkdownContent] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // 1. Tell Vite to map the entire directory of markdown files eagerly as raw text strings
    const modules = import.meta.glob("../content/trading/*.md", {
      query: "?raw",
      eager: true,
    });

    // 2. Construct the exact path lookup key
    const targetPath = `../content/trading/${storyId}.md`;

    // 3. Look up the file in our pre-mapped modules
    const matchedModule = modules[targetPath];

    if (matchedModule) {
      setMarkdownContent(matchedModule.default);
    } else {
      console.error(`Story file not found at: ${targetPath}`);
    }

    setLoading(false);
  }, [storyId]);

  if (loading)
    return (
      <div className="pageContainer">
        <p>Loading chronicle...</p>
      </div>
    );
  if (!markdownContent) {
    return (
      <div className="pageContainer">
        <h2>Story not found</h2>
        <Link to="/trading">← Back to Trading Hub</Link>
      </div>
    );
  }

  return (
    <main className={styles.storyCanvas}>
      <div className="pageContainer">
        <Link to="/trading" className={styles.backBtn}>
          ← Back to Trading Hub
        </Link>
        <article className={styles.markdownWrapper}>
          <ReactMarkdown>{markdownContent}</ReactMarkdown>
        </article>
      </div>
    </main>
  );
}

export default StoryView;
