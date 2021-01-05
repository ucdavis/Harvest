module.exports = {

  // workaround for CRA not supporting jest's reporters config...
  jest: config => {
    const newConfig = {
      ...config,
      reporters: [
        'default',
        [
          'jest-trx-results-processor',
          {
            outputFile: './output/jest-results.trx',
            defaultUserName: 'JestRunner'
          }
        ]
      ]
    }
    return newConfig;
  }
}