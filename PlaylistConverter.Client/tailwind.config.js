/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
    "./Pages/**/*.{razor,html,cshtml}",
    "./Layout/**/*.{razor,html,cshtml}",
    "./Shared/**/*.{razor,html,cshtml}"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}