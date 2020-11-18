const path = require('path');
const fs = require('fs');
const { addReactRefresh } = require('customize-cra-react-refresh');
const { override, babelInclude, removeModuleScopePlugin } = require('customize-cra');

module.exports = (config, env) => Object.assign(
  config,
  override(
    babelInclude([
      path.resolve('./src'),
      fs.realpathSync('./public'),
    ]),
    addReactRefresh(),
    removeModuleScopePlugin(),
  )(config, env),
);
