namespace Fluke.UI.Frontend

open Suigetsu.UI
open Suigetsu.UI.ElmishBridge.Frontend

module MainView =
    let lazyView (props: Client.MainViewProps<SharedState.SharedServerMessage, UIState.State>) =
        ()

//        let dispatch =
//            InternalUI.SharedServerMessage
//            >> Client.InternalServerMessage
//            >> props.ServerToClientDispatch
            
//        HomePageComponent.``default``
//            { Dispatch = dispatch
//              UIState = props.UIState
//              PrivateState = props.PrivateState }
