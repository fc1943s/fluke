namespace FsUi.State.Selectors

open FsStore
open FsUi
open FsUi.Model
open FsUi.State


module rec Ui =
    let rec uiState =
        Store.readSelector
            FsUi.root
            (nameof uiState)
            (fun getter ->
                {
                    DarkMode = Store.value getter Atoms.Ui.darkMode
                    FontSize = Store.value getter Atoms.Ui.fontSize
                })
