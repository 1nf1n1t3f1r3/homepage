// src/pages/StoryView.jsx
import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import styles from "./StoryView.module.css";

// 1. Snag the eagerly bundled modules out here at the top file level (only once!)
const mdModules = import.meta.glob("../content/trading/*.md", {
  query: "?raw",
  eager: true,
});
const codeModules = import.meta.glob("../content/trading/*.py", {
  query: "?raw",
  eager: true,
});

function StoryView() {
  const { storyId } = useParams();

  // 2. Keep state ONLY for dynamic, user-driven actions (like the modal overlay)
  const [activeModalImg, setActiveModalImg] = useState(null);

  // 3. PURE DATA DERIVATION: Zero useEffects, zero extra re-renders
  const mdPath = `../content/trading/${storyId}.md`;
  const codePath = `../content/trading/${storyId}.py`;

  const markdownContent = mdModules[mdPath]?.default || null;
  const codeContent = codeModules[codePath]?.default || null;

  // Intercept the default markdown image render loop
  const markdownRenderComponents = {
    img: ({ src, alt }) => (
      <img
        src={src}
        alt={alt}
        className={styles.zoomableMarkdownImg}
        onClick={() => setActiveModalImg({ src, alt })}
      />
    ),
  };

  // 4. Bail out immediately if the path is invalid
  if (!markdownContent) {
    return (
      <div className="pageContainer">
        <h2>Story not found</h2>
        <Link to="/trading">← Back to Trading Hub</Link>
      </div>
    );
  }

  return (
    <main className={styles.fullBleedCanvas}>
      <div className={styles.storyContainer}>
        <div className={styles.splitLayout}>
          {/* Left Column: Story */}
          <section className={styles.narrativeColumn}>
            <article className={styles.readingCard}>
              <ReactMarkdown components={markdownRenderComponents}>
                {markdownContent}
              </ReactMarkdown>
            </article>
          </section>

          {/* Right Column: Code Window */}
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
                <p>No associated code script file found.</p>
              </div>
            )}
          </section>
        </div>

        <Link to="/trading" className={styles.backBtn}>
          ← Back to Trading Hub
        </Link>
      </div>

      {/* DYNAMIC BACKDROP LIGHTBOX ZOOM OVERLAY */}
      {activeModalImg && (
        <div
          className={styles.lightboxOverlay}
          onClick={() => setActiveModalImg(null)}
        >
          <div className={styles.lightboxCloseBtn}>&times;</div>
          <img
            src={activeModalImg.src}
            alt={activeModalImg.alt}
            className={styles.lightboxActiveStage}
            onClick={(e) => e.stopPropagation()}
          />
        </div>
      )}
    </main>
  );
}

export default StoryView;
