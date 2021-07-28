namespace FsUi

open FsCore.Model


module Model =
    let collection = Collection (nameof FsUi)

    type UiState =
        {
            DarkMode: bool
            FontSize: int
        }
        static member inline Default = { DarkMode = false; FontSize = 15 }
