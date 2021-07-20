const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');

let mainWindow;

process.env.NODE_ENV = 'production';

const port = 33929;

let listen = false;

app.commandLine.appendSwitch('disable-features', 'SpareRendererForSitePerProcess,WebRtcHideLocalIpsWithMdns');
if (process.env.FLUKE_USER_DATA_PATH) {
  app.setPath('userData', process.env.FLUKE_USER_DATA_PATH || path.resolve(`${app.getAppPath()}/../../userData`));
}

function forceExit() {
  mainWindow = null;
  app.quit();
}

async function createWindow() {
  (async () => {
    if (!listen) {
      listen = true;
      const serveApp = require('https-localhost')();
      serveApp.serve(`${app.getAppPath()}/cra_output`, port)
      console.log('userData', app.getPath('userData'));
    }
  })().then();

  // await app.whenReady();

  mainWindow = new BrowserWindow({
    width: 900,
    height: 680,
    frame: false,
    // show: false,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      enableRemoteModule: false,
      preload: path.join(__dirname, 'electron-preload.js')
    }
  });

  mainWindow.loadURL(`https://localhost:${port}`);

  mainWindow.once('ready-to-show', () => mainWindow.show());
  mainWindow.on('closed', forceExit);
}

app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    forceExit();
  }
});

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow().then();
  }
});

ipcMain.on('close', (evt, arg) => {
  console.log('closing', evt, arg);
  forceExit();
})

ipcMain.on('minimize', (evt, arg) => {
  mainWindow.minimize();
})
