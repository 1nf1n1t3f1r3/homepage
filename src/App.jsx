// src/App.jsx

import React from "react";
import { Routes, Route } from "react-router-dom";
import ScrollToTop from "./components/ScrollToTop"; // 1. Import the utility component
import Header from "./components/Header";
import Home from "./pages/Home";
import MyWebsites from "./pages/MyWebsites";
import OdinHub from "./pages/OdinHub";
import Trading from "./pages/Trading";
import StoryView from "./pages/StoryView";
import Unity from "./pages/Unity";
import StoryViewUnity from "./pages/StoryViewUnity";

function App() {
  return (
    <div className="app-container">
      {/* Prevent Scrolling Issues */}
      <ScrollToTop />

      {/* The header stays visible on every single page */}
      <Header />

      {/* React Router decides which component to render based on the URL */}
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/odin" element={<OdinHub />} />
        <Route path="/trading" element={<Trading />} />
        <Route path="/trading/:storyId" element={<StoryView />} />
        <Route path="/unity" element={<Unity />} />
        <Route path="/unity/:storyId" element={<StoryViewUnity />} />
        <Route path="/mywebsites" element={<MyWebsites />} />
      </Routes>
    </div>
  );
}

export default App;
