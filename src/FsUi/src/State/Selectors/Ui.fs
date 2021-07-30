namespace FsUi.State.Selectors

open FsUi.State
open FsStore
open FsUi.Model


module rec Ui =
    let rec uiState =
        Store.readSelector
            $"{nameof Ui}/{nameof uiState}"
            (fun getter ->
                {
                    DarkMode = Store.value getter Atoms.Ui.darkMode
                    FontSize = Store.value getter Atoms.Ui.fontSize
                })
