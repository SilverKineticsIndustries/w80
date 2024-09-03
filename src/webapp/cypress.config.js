const { defineConfig } = require("cypress");
const shell = require("shelljs")

module.exports = defineConfig({
  fixturesFolder: '../../tests/n2n/fixtures',
  videoFolder: '../../tests/n2n/artifacts/video',
  downloadsFolder: '../../tests/n2n/artifacts/downloads',
  screenshotsFolder: '../../tests/n2n/artifacts/screenshots',
  defaultCommandTimeout: 10000, // milliseconds
  e2e: {
    supportFile: '../../tests/n2n/support/e2e.js',
    specPattern: '../../tests/n2n/tests/*.js',
    baseUrl: 'http://localhost:15002',
    timeout: 10000,
    video: false,
    videoCompression: true,
    setupNodeEvents(on, config) {
      on('task', {
        log(message) {
          console.log(message)
          return null
        },
        cloneDb() {
          shell.exec('../../db/util/clone_database.sh W80 W80_INIT');
          return null
        },
        restoreDb() {
          shell.exec('../../db/util/clone_database.sh W80_INIT W80');
          return null
        }
      })
    }
  }
});
