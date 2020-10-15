namespace Fluke.UI.Frontend.Hooks

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared

module SoundPlayer =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let hook =
        React.memo (fun (input: {| Username: Username |}) ->
            let oldActiveSessions = React.useRef []
            let (Minute sessionLength) = Recoil.useValue (Recoil.Atoms.User.sessionLength input.Username)

            let (Minute sessionBreakLength) = Recoil.useValue (Recoil.Atoms.User.sessionBreakLength input.Username)

            let activeSessions = Recoil.useValue (Recoil.Selectors.Session.activeSessions input.Username)

            React.useEffect
                ((fun () ->
                    oldActiveSessions.current
                    |> List.map (fun (Model.ActiveSession (oldTaskName, (Minute oldDuration), _, _)) ->
                        let newSession =
                            activeSessions
                            |> List.tryFind (fun (Model.ActiveSession (taskName, (Minute duration), _, _)) ->
                                taskName = oldTaskName
                                && duration = oldDuration + 1.)

                        match newSession with
                        | Some (Model.ActiveSession (_, (Minute newDuration), _, _)) when oldDuration = -1.
                                                                                               && newDuration = 0. ->
                            TempAudio.playTick
                        | Some (Model.ActiveSession (_, newDuration, totalDuration, _)) when newDuration = totalDuration ->
                            TempAudio.playDing
                        | None ->
                            if oldDuration = sessionLength
                               + sessionBreakLength
                               - 1. then
                                TempAudio.playDing
                            else
                                id
                        | _ -> id)
                    |> List.iter (fun x -> x ())

                    oldActiveSessions.current <- activeSessions),
                 [|
                     sessionLength :> obj
                     sessionBreakLength :> obj
                     activeSessions :> obj
                 |])

            nothing)
