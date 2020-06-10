namespace Fluke.UI.Frontend



module Client =
    let inline handleClientMessage (message: SharedState.SharedServerMessage) (state: UIState.State) =
        match message with
        | () -> state, None

//    let listen () =
//        Client.listen<UIState.State, SharedState.SharedServerMessage, SharedState.SharedClientMessage>
//            UIState.State.Default
//            MainView.lazyView
//            handleClientMessage
//            false

//    listen ()


