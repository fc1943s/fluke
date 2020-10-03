const path = require("path");

module.exports = {
    allFiles: true,
    entry: path.join(__dirname, "./Fluke.UI.Frontend.Tests.fsproj"),
    outDir: path.join(__dirname, "../../src/Fluke.UI.Frontend/dist/tests"),
    babel: {
        plugins: ["@babel/plugin-transform-modules-commonjs"],
        sourceMaps: "inline"
    }
};
