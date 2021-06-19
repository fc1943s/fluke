module.exports = {
  testEnvironment: 'jsdom',
  // preset: 'ts-jest/presets/js-with-babel',
  verbose: true,
  testMatch: ["**/*.test.fs.js"],
  transform: {
    '\\.js$': ['babel-jest', { configFile: './_babel.config.json' }]
  },
  setupFilesAfterEnv: ["./jest-setup.ts"],
  moduleNameMapper: {
    "\\.(css|less|scss|sss|styl)$": "<rootDir>/node_modules/jest-css-modules"
  },
  "transformIgnorePatterns": [
    "node_modules/(?!react-color)"
  ]
  // transform: {'.*?\.fs\.js': "babel-jest"},
  // maxConcurrency: 1,
};
