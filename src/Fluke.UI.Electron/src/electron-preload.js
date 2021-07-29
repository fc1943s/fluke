const {
  contextBridge,
  ipcRenderer
} = require("electron");

contextBridge.exposeInMainWorld(
  "electronApi", {
    send: ipcRenderer.send
  }
);
