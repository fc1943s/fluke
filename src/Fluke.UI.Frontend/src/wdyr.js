import React from 'react';

const store = require('./Bindings/Store.fs');

if (process.env.NODE_ENV === 'development') {
  const whyDidYouRender = require('@welldone-software/why-did-you-render');
  whyDidYouRender(React, {
    include: [/.*?/],
    trackAllPureComponents: true,
    trackExtraHooks: [
      [store, 'Store_useCallback'],
      [store, 'Store_useAtomFieldOptions'],
      [store, 'Store_useStateOption']
    ]
  });
}
