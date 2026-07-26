/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Reports/**/*.cshtml'
  ],
  mode: 'jit',
  theme: {
    extend: {
      fontFamily: {
        custom: ['Tahoma', 'sans-serif'],
      },
    },
  },
  plugins: [],
}

