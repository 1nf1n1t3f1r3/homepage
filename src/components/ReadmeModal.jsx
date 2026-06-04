// src/components/ReadmeModal.jsx

import React, { useState, useEffect } from "react";
import Markdown from "react-markdown";
import rehypeRaw from "rehype-raw";
import "./ReadmeModal.css";

function ReadmeModal({ repo, isOpen, onClose, projectImage }) {
  const [markdown, setMarkdown] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!isOpen) return;

    // Fetch the README using the public GitHub API
    fetch(`https://api.github.com/repos/${repo}/readme`, {
      headers: { Accept: "application/vnd.github.v3.raw" },
    })
      .then((res) => {
        if (!res.ok) throw new Error("Failed to fetch readme");
        return res.text();
      })
      .then((data) => {
        setMarkdown(data);
        setLoading(false);
      })
      .catch((err) => {
        console.error(err);
        setError(true);
        setLoading(false);
      });
  }, [repo, isOpen]);

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      {/* stopPropagation prevents clicking inside the modal from closing it */}
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close-btn" onClick={onClose}>
          ✕ Close
        </button>

        {/* Optional Project Showcase Image */}
        {projectImage && (
          <div className="modal-hero-image">
            <img src={projectImage} alt={`${repo} preview`} />
          </div>
        )}

        <div className="markdown-body">
          {loading && (
            <p className="modal-status">⏳ Fetching README from GitHub...</p>
          )}
          {error && (
            <p className="modal-status error">❌ Could not load README.</p>
          )}

          {/* 2. Add the rehypeRaw plugin here */}
          {!loading && !error && (
            <Markdown rehypePlugins={[rehypeRaw]}>{markdown}</Markdown>
          )}
        </div>
      </div>
    </div>
  );
}

export default ReadmeModal;
