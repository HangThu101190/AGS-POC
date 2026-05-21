import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
    // ag-grid-react must share the same React instance as the app (avoids useContext null)
    dedupe: ["react", "react-dom"],
  },
  optimizeDeps: {
    // Do not inline React inside ag-grid-react's prebundle
    exclude: ["ag-grid-react"],
    include: ["ag-grid-community"],
  },
  server: {
    port: 5174,
    proxy: {
      "/api": {
        target: "http://localhost:5024",
        changeOrigin: true,
      },
      "/health": {
        target: "http://localhost:5024",
        changeOrigin: true,
      },
      "/ready": {
        target: "http://localhost:5024",
        changeOrigin: true,
      },
      "/hubs": {
        target: "http://localhost:5024",
        changeOrigin: true,
        ws: true,
      },
    },
  },
});
