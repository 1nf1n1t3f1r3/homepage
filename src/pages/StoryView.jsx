// src/pages/StoryView.jsx
import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import styles from "./StoryView.module.css";

function StoryView() {
  const { storyId } = useParams();
  const [markdownContent, setMarkdownContent] = useState("");
  const [codeContent, setCodeContent] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // 1. Map both file extensions in the directory as raw text strings
    const mdModules = import.meta.glob("../content/trading/*.md", {
      query: "?raw",
      eager: true,
    });
    const codeModules = import.meta.glob("../content/trading/*.py", {
      query: "?raw",
      eager: true,
    });

    // 2. Generate target lookup keys
    const mdPath = `../content/trading/${storyId}.md`;
    const codePath = `../content/trading/${storyId}.py`;

    // 3. Extract the text payloads
    if (mdModules[mdPath]) {
      setMarkdownContent(mdModules[mdPath].default);
    }

    if (codeModules[codePath]) {
      setCodeContent(codeModules[codePath].default);
    }

    setLoading(false);
  }, [storyId]);

  if (loading)
    return (
      <div className={styles.storyContainer}>
        <p>Loading chronicle...</p>
      </div>
    );
  if (!markdownContent) {
    return (
      <div className={styles.storyContainer}>
        <h2>Story not found</h2>
        <Link to="/trading">← Back to Trading Hub</Link>
      </div>
    );
  }

  return (
    <main className={styles.fullBleedCanvas}>
      <div className={styles.storyContainer}>
        <div className={styles.splitLayout}>
          {/* Left Column: The Narrative Chronicle */}
          <section className={styles.narrativeColumn}>
            <article className={styles.readingCard}>
              <ReactMarkdown>{markdownContent}</ReactMarkdown>
            </article>
          </section>

          {/* Right Column: Code Blueprint (Sticky Container) */}
          <section className={styles.codeColumn}>
            {codeContent ? (
              <div className={styles.codeFrame}>
                <div className={styles.codeFrameHeader}>
                  <span>{storyId}.py</span>
                </div>
                <pre>
                  <code>{codeContent}</code>
                </pre>
              </div>
            ) : (
              <div className={styles.noCodeBox}>
                <p>
                  No associated code engine script file found for this entry.
                </p>
              </div>
            )}
          </section>
        </div>
        <Link to="/trading" className={styles.backBtn}>
          ← Back to Trading Hub
        </Link>
      </div>
    </main>
  );
}

export default StoryView;
