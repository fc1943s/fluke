module.exports = {
    testEnvironment: 'jest-environment-jsdom',
    // testEnvironment: 'jest-environment-node',
    // preset: 'ts-jest/presets/js-with-babel',
    verbose: true,
    testMatch: ["**/*.test.fs.js"],
    // transform: {'.*?\.fs\.js': "babel-jest"},
    // maxConcurrency: 1,
};
