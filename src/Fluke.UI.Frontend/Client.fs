namespace Fluke.UI.Frontend

open Suigetsu.UI.ElmishBridge.Frontend
open Elmish
open Elmish.React

module Temp =
    #if DEBUG
    open Elmish.HMR
    #endif
    let listen () =
        let init () = (), Cmd.none
        
        let update msg state =
            state, Cmd.none
            
        let viewWrapper =
            fun state dispatch ->
                HomePageComponent.``default``
                    ()
                       
        Program.mkProgram init update viewWrapper
        |> Program.withReactSynchronous "app"
        |> Program.run
        

module Client =
    let inline handleClientMessage (message: SharedState.SharedServerMessage) (state: UIState.State) =
        match message with
        | () -> state, None

    let listen () =
        Temp.listen ()
//        Client.listen<UIState.State, SharedState.SharedServerMessage, SharedState.SharedClientMessage>
//            UIState.State.Default
//            MainView.lazyView
//            handleClientMessage

    listen ()

