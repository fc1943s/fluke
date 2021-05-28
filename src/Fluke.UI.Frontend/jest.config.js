module.exports = {
  testEnvironment: 'jsdom',
  // preset: 'ts-jest/presets/js-with-babel',
  verbose: true,
  testMatch: ["**/*.test.fs.js"],
  transform: {
    '\\.js$': ['babel-jest', { configFile: './_babel.config.json' }]
  },
  moduleNameMapper: {
    "\\.(css|less|scss|sss|styl)$": "<rootDir>/node_modules/jest-css-modules"
  }
  // transform: {'.*?\.fs\.js': "babel-jest"},
  // maxConcurrency: 1,
};
