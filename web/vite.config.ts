import react from "@vitejs/plugin-react-swc";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": "http://localhost:5080",
      "/health": "http://localhost:5080",
    },
  },
  test: {
    exclude: ["**/node_modules/**", "**/dist/**", "e2e/**", "web/e2e/**"],
    environment: "jsdom",
    environmentOptions: {
      jsdom: { url: "http://localhost/" },
    },
    setupFiles: "./src/test-setup.ts",
    css: true,
    restoreMocks: true,
  },
});
