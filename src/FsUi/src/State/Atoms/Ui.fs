namespace FsUi.State.Atoms

open FsStore
open FsUi.Model

module rec Ui =
    let rec darkMode =
        Store.atomWithStorageSync (Model.collection, $"{nameof Ui}/{nameof darkMode}", UiState.Default.DarkMode)

    let rec fontSize =
        Store.atomWithStorageSync (Model.collection, $"{nameof Ui}/{nameof fontSize}", UiState.Default.FontSize)
