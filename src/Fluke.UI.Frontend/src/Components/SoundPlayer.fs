namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Bindings

module SoundPlayer =
    open Model

    [<ReactComponent>]
    let SoundPlayer () =
        let oldActiveSessions = React.useRef []
        let sessionDuration = Store.useValue Atoms.User.sessionDuration
        let sessionBreakDuration = Store.useValue Atoms.User.sessionBreakDuration
        let activeSessions = Store.useValue Selectors.Session.activeSessions

        React.useEffect (
            (fun () ->
                oldActiveSessions.current
                |> List.map
                    (fun (TempUI.ActiveSession (oldTaskName, Minute oldDuration)) ->
                        let newSession =
                            activeSessions
                            |> List.tryFind
                                (fun (TempUI.ActiveSession (taskName, Minute duration)) ->
                                    taskName = oldTaskName
                                    && duration = oldDuration + 1)

                        match newSession with
                        | Some (TempUI.ActiveSession (_, Minute newDuration)) when oldDuration = -1 && newDuration = 0 ->
                            TempAudio.playTick
                        | Some (TempUI.ActiveSession (_, newDuration)) when
                            Minute.Value newDuration = Minute.Value sessionDuration -> TempAudio.playDing
                        | None ->
                            if oldDuration = (Minute.Value sessionDuration)
                                             + (Minute.Value sessionBreakDuration)
                                             - 1 then
                                TempAudio.playDing
                            else
                                id
                        | _ -> id)
                |> List.iter (fun x -> x ())

                oldActiveSessions.current <- activeSessions),
            [|
                box oldActiveSessions
                box sessionDuration
                box sessionBreakDuration
                box activeSessions
            |]
        )

        nothing
