/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      colors: {
        primary: 'var(--primary-color)',
        secondary: 'var(--secondary-color)',
        accent: 'var(--accent-color)',
        background: 'var(--background-color)',
        navbar: 'var(--navbar-color)',
        'navbar-border': 'var(--navbar-border-color)',
        contrast: 'var(--contrast-color)'
      },
      fontFamily: {
        heading: ['var(--heading-font)'],
        body: ['var(--body-font)']
      }
    },
  },
  plugins: [],
}
