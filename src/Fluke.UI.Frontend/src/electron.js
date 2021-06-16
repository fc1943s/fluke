const { app, BrowserWindow } = require('electron');
const path = require('path');

let mainWindow;

process.env.NODE_ENV = 'production';

const port = 33929;

let listen = false;

app.commandLine.appendSwitch('disable-features', 'SpareRendererForSitePerProcess,WebRtcHideLocalIpsWithMdns');
app.setPath('userData', process.env.FLUKE_USER_DATA_PATH || path.resolve(app.getAppPath() + '/../../userData'));

async function createWindow() {
  (async () => {
    if (!listen) {
      listen = true;
      const serveApp = require('https-localhost')();
      serveApp.serve(app.getAppPath() + '/cra_output', port)
      console.log('userData', app.getPath('userData'));
    }
  })().then();

  // await app.whenReady();

  mainWindow = new BrowserWindow({
    width: 900,
    height: 680,
    // show: false,
    // webPreferences: {
    //   nodeIntegration: true
    // }
  });

  mainWindow.loadURL("https://localhost:" + port);

  mainWindow.once('ready-to-show', () => mainWindow.show());
  mainWindow.on('closed', () => mainWindow = null);
}

app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow().then();
  }
});
