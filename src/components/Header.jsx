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
        <Link to="/odin" className={isActive("/odin")}>
          Odin Projects
        </Link>

        <Link to="/trading" className={isActive("/trading")}>
          Trading
        </Link>
        <Link to="/unity" className={isActive("/unity")}>
          Unity Dev
        </Link>
        <Link to="/dnd" className={isActive("/unity")}>
          D&D
        </Link>
        <Link to="/masquedefer" className={isActive("/unity")}>
          Masque de Fer
        </Link>
      </nav>
    </header>
  );
}

export default Header;
