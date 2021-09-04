module.exports = {
  testEnvironment: 'jsdom',
  // preset: 'ts-jest/presets/js-with-babel',
  "preset": "ts-jest",
  verbose: true,
  testMatch: ["**/*.test.fs.js"],
  testPathIgnorePatterns: [".fable"],
  transform: {
    '\\.js$': ['babel-jest', { configFile: './_babel.config.json' }]
  },
  setupFilesAfterEnv: ["./jest-setup.ts"],
  moduleNameMapper: {
    "\\.(css|less|scss|sss|styl)$": "<rootDir>/node_modules/jest-css-modules"
  },
  "transformIgnorePatterns": [
    // "(?!(react-color|react-vim-wasm|vim-wasm))"
  ],
  // maxConcurrency: 1,
};
