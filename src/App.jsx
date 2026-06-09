// src/App.jsx

import React from "react";
import { Routes, Route } from "react-router-dom";
import Header from "./components/Header";
import MyWebsites from "./pages/MyWebsites";
import OdinHub from "./pages/OdinHub";
import Trading from "./pages/Trading";
import StoryView from "./pages/StoryView"; // The single template page

// Tiny placeholder components for your pages (you can move these to separate files later!)
const Home = () => (
  <main className="page-content">
    <h2>Resume & About Me</h2>
    <p>Welcome to my portfolio...</p>
  </main>
);
// const OdinHubLink = () => (
//   <main className="page-content">
//     <h2>Odin Projects</h2>
//     <p>Check out my apps running on their own subdomains...</p>
//   </main>
// );
// const Trading = () => (
//   <main className="page-content">
//     <h2>Trading Scripts</h2>
//     <p>Pine Script and automation write-ups...</p>
//   </main>
// );
const UnityDev = () => (
  <main className="page-content">
    <h2>Unity & The Networking Wheel</h2>
    <p>The story of how I tried to rewrite multiplayer sync from scratch...</p>
  </main>
);
const DnD = () => (
  <main className="page-content">
    <h2>D&D Worldbuilding Tools</h2>
    <p>I'm a bit tired of having to come up with loot every dungeon</p>
  </main>
);
// const MasqueDeFer = () => (
//   <main className="page-content">
//     <h2>Masque de Fer Club Website</h2>
//     <p>The (Dutch) Website of Schermvereniging Masque de Fer, Zwijndrecht</p>
//   </main>
// );

function App() {
  return (
    <div className="app-container">
      {/* The header stays visible on every single page */}
      <Header />

      {/* React Router decides which component to render based on the URL */}
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/odin" element={<OdinHub />} />
        <Route path="/trading" element={<Trading />} />
        <Route path="/trading/:storyId" element={<StoryView />} />
        <Route path="/unity" element={<UnityDev />} />
        <Route path="/mywebsites" element={<MyWebsites />} />
      </Routes>
    </div>
  );
}

export default App;
