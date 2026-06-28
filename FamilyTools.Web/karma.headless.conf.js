// Temporary headless config for sandbox runs (no Chrome installed; uses CHROME_BIN=Edge).
const base = require('./karma.conf.js');
module.exports = function (config) {
  base(config);
  config.set({
    browsers: ['ChromeHeadless'],
    singleRun: true,
    autoWatch: false,
  });
};
