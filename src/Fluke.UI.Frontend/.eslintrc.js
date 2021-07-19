module.exports = {
  env: {
    browser: true,
    es6: true,
    node: true,
    jest: true,
  },
  extends: [
    'plugin:react/recommended',
    'plugin:react-hooks/recommended',
    'plugin:jest-dom/recommended',
    'plugin:@typescript-eslint/eslint-recommended',
  ],
  globals: {
    Atomics: 'readonly',
    SharedArrayBuffer: 'readonly',
  },
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaFeatures: {
      jsx: true,
    },
    ecmaVersion: 12,
    sourceType: 'module',
  },
  plugins: [
    'react',
    'react-hooks',
    '@typescript-eslint',
    'jest-dom',
  ],
  rules: {
    // 'import/no-extraneous-dependencies': 'off',
    // 'import/extensions': ['error', 'ignorePackages', {
    //   js: 'never',
    //   jsx: 'never',
    //   ts: 'never',
    //   tsx: 'never',
    //   json: 'never',
    // }],
    'react/jsx-filename-extension': [1, { extensions: ['.js', '.jsx', '.ts', '.tsx'] }],
    'react-hooks/exhaustive-deps': ['warn', { additionalHooks: '(.*?useRecoilCallback.*?|.*?useCallback.*?|.*?useDisposableEffect.*?|.*?useEffect.*?|.*?useMemo.*?)' }],
    'react-hooks/rules-of-hooks': 'warn',

    'react/display-name': 'off',
  },
  settings: {
    'import/resolver': {
      node: {
        extensions: ['.js', '.jsx', '.ts', '.tsx'],
      },
    },
  },
  // ignorePatterns: ['**/*.fs.js', '**/*.fs.ts'],
};
