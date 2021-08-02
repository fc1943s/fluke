namespace FsUi.State.Atoms

open FsStore
open FsUi.Model
open FsUi


module rec Ui =
    let rec darkMode = Store.atomWithStorageSync FsUi.root (nameof darkMode) UiState.Default.DarkMode
    let rec fontSize = Store.atomWithStorageSync FsUi.root (nameof fontSize) UiState.Default.FontSize
