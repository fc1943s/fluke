const path = require('path');
const fs = require('fs');
const babelInclude = require('@dealmore/craco-plugin-babel-include');

module.exports = {
  // devServer: {
  //   writeToDisk: true
  // },
  plugins: [
    {
      plugin: babelInclude,
      options: {
        include: [
          path.resolve('./src'),
          fs.realpathSync('./public'),
        ],
      },
    },
  ],
};
