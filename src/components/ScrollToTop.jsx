// src/components/ScrollToTop.jsx
import { useEffect } from "react";
import { useLocation } from "react-router-dom";

function ScrollToTop() {
  const { pathname } = useLocation();

  useEffect(() => {
    // Imperatively reset the main browser window to the top-left corner
    window.scrollTo(0, 0);
  }, [pathname]); // This hook re-fires every single time the route path updates

  return null; // This component is a pure functional utility; it doesn't render any HTML
}

export default ScrollToTop;
