// src/pages/StoryView.jsx
import { useState, useEffect, useLayoutEffect } from "react";
import { useParams, Link } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import styles from "./StoryView.module.css";

import Prism from "prismjs";
import "prismjs/components/prism-python";

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
  const [activeModalImg, setActiveModalImg] = useState(null);

  const mdPath = `../content/trading/${storyId}.md`;
  const codePath = `../content/trading/${storyId}.py`;

  const markdownContent = mdModules[mdPath]?.default || null;
  const codeContent = codeModules[codePath]?.default || null;

  useLayoutEffect(() => {
    window.scrollTo(0, 0);
  }, [storyId]); // Fires instantly the second the storyId changes

  // Prism
  useEffect(() => {
    if (markdownContent || codeContent) {
      Prism.highlightAll();
    }
  }, [markdownContent, codeContent, storyId]);

  // Intercept the default markdown image and code render loops
  const markdownRenderComponents = {
    img: ({ src, alt }) => (
      <img
        src={src}
        alt={alt}
        className={styles.zoomableMarkdownImg}
        onClick={() => setActiveModalImg({ src, alt })}
      />
    ),
    // Ensures any code snippets inside the markdown markdown text column also get tokens applied
    code: ({ className, children }) => {
      const language = className
        ? className.replace("language-", "")
        : "python";
      return <code className={`language-${language}`}>{children}</code>;
    },

    // Open Links in new Tabs
    a: ({ href, children }) => {
      return (
        <a href={href} target="_blank" rel="noopener noreferrer">
          {children}
        </a>
      );
    },
  };

  if (!markdownContent || (codeContent && !codeModules[codePath])) {
    return (
      <main
        className={styles.fullBleedCanvas}
        style={{ minHeight: "100vh" }}
      ></main>
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
            <Link to="/trading" className={styles.backBtn}>
              ← Back to Trading Hub
            </Link>
          </section>

          {/* Right Column: Code Window */}
          <section className={styles.codeColumn}>
            {codeContent ? (
              <div className={styles.codeFrame}>
                <div className={styles.codeFrameHeader}>
                  <span>{storyId}.py</span>
                </div>
                {/* Prism target class "language-python" here */}
                <pre className="language-python">
                  <code className="language-python">{codeContent}</code>
                </pre>
              </div>
            ) : (
              <div className={styles.noCodeBox}>
                <p>No associated code script file found.</p>
              </div>
            )}
          </section>
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

export default StoryView;
