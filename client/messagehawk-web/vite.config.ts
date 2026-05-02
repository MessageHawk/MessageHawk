import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "https://localhost:7079",
        changeOrigin: true,
        secure: false,
      },
      "/health": {
        target: "https://localhost:7079",
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    outDir: "../../src/MessageHawk.Api/wwwroot",
    emptyOutDir: true,
  },
});
