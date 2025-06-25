/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      colors: {
        primary: '#3B5F3B',
        secondary: '#A97449',
        accent: '#D5B57A',
        background: '#F9F6F1',
        contrast: '#2E2E2E'
      },
      fontFamily: {
        heading: ['"Playfair Display"', 'serif'],
        body: ['Lora', 'serif']
      }
    },
  },
  plugins: [],
}
