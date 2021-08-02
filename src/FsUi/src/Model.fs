namespace FsUi

open FsCore.Model


module FsUi =
    let root = StoreRoot (nameof FsUi)


module Model =
    type UiState =
        {
            DarkMode: bool
            FontSize: int
        }
        static member inline Default = { DarkMode = false; FontSize = 15 }
