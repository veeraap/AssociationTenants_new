﻿module.exports = {
    theme: {
        extend: {}
    },
    content: ["./**/*.razor", "./wwwroot/index.html"],
    plugins: [
        require('@tailwindcss/forms'),
    ]
}