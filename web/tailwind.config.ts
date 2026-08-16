import type { Config } from "tailwindcss";

export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        ink: "var(--midnight-ink)",
        paper: "var(--cool-paper)",
        action: "var(--action-cobalt)",
        shared: "var(--shared-mint)",
        warning: "var(--warning-amber)",
        border: "var(--mist-border)",
        surface: "var(--surface)",
        danger: "var(--danger)",
      },
      fontFamily: {
        sans: ["Manrope Variable", "sans-serif"],
        editorial: ["Literata Variable", "serif"],
      },
    },
  },
  plugins: [],
} satisfies Config;
