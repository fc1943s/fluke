const path = require("path");

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

module.exports = {
  entry: [
    resolve("./Fluke.UI.Electron.Renderer.fsproj"),
  ],
  output: {
    filename: "renderer.js"
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader"
        }
      }
    ]
  }
}
