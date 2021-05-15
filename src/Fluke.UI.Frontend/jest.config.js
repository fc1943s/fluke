module.exports = {
  testEnvironment: 'jest-environment-jsdom',
  // preset: 'ts-jest/presets/js-with-babel',
  verbose: true,
  testMatch: ["**/*.test.fs.js"],
  restoreMocks: true,
  clearMocks: true,
  resetMocks: true,
  moduleNameMapper: {
    "\\.(css|less|scss|sss|styl)$": "<rootDir>/node_modules/jest-css-modules"
  }
  // transform: {'.*?\.fs\.js': "babel-jest"},
  // maxConcurrency: 1,
};
