import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import sitemap from "vite-plugin-sitemap"; // 1. Import the plugin

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    sitemap({
      // 2. Configure the plugin
      hostname: "https://janusdevries.nl",
      // Tell it about your React Router paths
      dynamicRoutes: ["/mywebsites", "/odin", "/trading", "/unity"],
    }),
  ],
});
