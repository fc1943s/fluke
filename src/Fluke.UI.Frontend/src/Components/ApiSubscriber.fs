namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend


module ApiSubscriber =
    [<ReactComponent>]
    let ApiSubscriber () =
        let apiBaseUrl = Recoil.useValue Recoil.Atoms.apiBaseUrl
        let setApi = Recoil.useSetState Recoil.Atoms.api

        React.useEffect (
            (fun () -> setApi (Some (Sync.createApi apiBaseUrl))),
            [|
                box setApi
                box apiBaseUrl
            |]
        )

        nothing
