const path = require('path');
const webpack = require('webpack');
const fs = require('fs');
// const { addReactRefresh } = require('customize-cra-react-refresh');
const {
  override,
  babelInclude,
  removeModuleScopePlugin,
  useBabelRc,
  getBabelLoader,
  addWebpackPlugin
} = require('customize-cra');

const wdyrFn = ((config) => {
  const options = getBabelLoader(config).options;

  const originalPreset = options.presets.find((preset) => preset[0].includes('babel-preset-react-app'));
  if (originalPreset) {
    Object.assign(originalPreset[1], {
      development: true,
      importSource: '@welldone-software/why-did-you-render',
    });
  }
  return config;
})

module.exports = (config, env) => Object.assign(
  config,
  override(
    useBabelRc(),
    babelInclude([
      path.resolve('./src'),
      fs.realpathSync('./public'),
    ]),
    addWebpackPlugin(
      new webpack.ContextReplacementPlugin(
        /gun/,
        (data) => {
          delete data.dependencies[0].critical;
          return data;
        },
      ),
    ),
    // addReactRefresh(),
    removeModuleScopePlugin(),
  )(config, env),
);
