// src/components/Header.jsx

import React from "react";
import { Link, useLocation } from "react-router-dom";
import "./Header.css";

function Header() {
  const location = useLocation();

  // Helper to add an 'active' class to the current page link
  const isActive = (path) =>
    location.pathname === path ? "nav-link active" : "nav-link";

  return (
    <header className="global-header">
      <h1 className="header-logo">
        <Link to="/">Janus de Vries</Link>
      </h1>

      <nav className="header-nav">
        <Link to="/" className={isActive("/")}>
          Home & Resume
        </Link>
        <Link to="/mywebsites" className={isActive("/mywebsites")}>
          My Websites
        </Link>
        <Link to="/odin" className={isActive("/odin")}>
          Odin Websites
        </Link>
        <Link to="/trading" className={isActive("/trading")}>
          Trading Stories
        </Link>
        <Link to="/unity" className={isActive("/unity")}>
          Unity Stories
        </Link>
        {/* <Link to="/dnd" className={isActive("/dnd")}>
          D&D
        </Link> */}
      </nav>
    </header>
  );
}

export default Header;
