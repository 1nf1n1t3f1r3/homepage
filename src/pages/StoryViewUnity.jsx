// src/pages/StoryViewUnity.jsx
import { useState, useEffect, useLayoutEffect } from "react";
import { useParams, Link } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import styles from "./StoryViewUnity.module.css";

import Prism from "prismjs";
import "prismjs/components/prism-csharp";

const mdModules = import.meta.glob("../content/unity/*.md", {
  query: "?raw",
  eager: true,
});
const codeModules = import.meta.glob("../content/unity/*.cs", {
  query: "?raw",
  eager: true,
});

function StoryViewUnity() {
  const { storyId } = useParams();
  const [activeModalImg, setActiveModalImg] = useState(null);

  const mdPath = `../content/unity/${storyId}.md`;
  const codePath = `../content/unity/${storyId}.cs`;

  const markdownContent = mdModules[mdPath]?.default || null;
  const codeContent = codeModules[codePath]?.default || null;

  useLayoutEffect(() => {
    window.scrollTo(0, 0);
  }, [storyId]);

  // Prism Highlight Loop
  useEffect(() => {
    if (markdownContent || codeContent) {
      Prism.highlightAll();
    }
  }, [markdownContent, codeContent, storyId]);

  const markdownRenderComponents = {
    img: ({ src, alt }) => (
      <img
        src={src}
        alt={alt}
        className={styles.zoomableMarkdownImg}
        onClick={() => setActiveModalImg({ src, alt })}
      />
    ),
    code: ({ className, children }) => {
      const language = className
        ? className.replace("language-", "")
        : "python"; // Change default fallback here if needed
      return <code className={`language-${language}`}>{children}</code>;
    },
    a: ({ href, children }) => (
      <a href={href} target="_blank" rel="noopener noreferrer">
        {children}
      </a>
    ),
  };

  if (!markdownContent) {
    return (
      <main className={styles.fullBleedCanvas} style={{ minHeight: "100vh" }} />
    );
  }

  // 🚀 DYNAMIC LAYOUT SELECTOR
  // If codeContent is available, use splitLayout. Otherwise, switch to centerLayout.
  const layoutClassName = codeContent
    ? styles.splitLayout
    : styles.centerLayout;

  return (
    <main className={styles.fullBleedCanvas}>
      <div className={styles.storyContainer}>
        {/* Swaps classes structurally depending on asset file states */}
        <div className={layoutClassName}>
          {/* Narrative text column always renders */}
          <section className={styles.narrativeColumn}>
            <article className={styles.readingCard}>
              <ReactMarkdown components={markdownRenderComponents}>
                {markdownContent}
              </ReactMarkdown>
            </article>
            <Link to="/unity" className={styles.backBtn}>
              ← Back to Unity Hub
            </Link>
          </section>

          {/* ⚡ CONDITIONAL CODE WINDOW: Completely disappears if no file exists */}
          {codeContent && (
            <section className={styles.codeColumn}>
              <div className={styles.codeFrame}>
                <div className={styles.codeFrameHeader}>
                  <span>{storyId}.cs</span>
                </div>

                <pre className="language-python">
                  <code className="language-python">{codeContent}</code>
                </pre>
              </div>
            </section>
          )}
        </div>
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

export default StoryViewUnity;
