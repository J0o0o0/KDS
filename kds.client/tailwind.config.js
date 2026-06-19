/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // KDS urgency colors
        urgent: {
          DEFAULT: '#ef4444',  // red — late
          warn: '#f59e0b',     // amber — warning
          ok: '#10b981',       // green — on time
        }
      },
      fontFamily: {
        mono: ['"JetBrains Mono"', 'monospace'],  // for timers
      }
    },
  },
  plugins: [],
}
