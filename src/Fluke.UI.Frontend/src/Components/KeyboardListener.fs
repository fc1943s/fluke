namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.Core.JsInterop
open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fable.Core
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open System
open Fluke.Shared.Domain.UserInteraction


module CtrlListener =
    [<ReactComponent>]
    let CtrlListener () =
        Listener.useKeyPress
            [|
                "Control"
            |]
            (fun getter setter e ->
                promise {
                    let ctrlPressed = Store.value getter Atoms.ctrlPressed

                    if e.ctrlKey <> ctrlPressed then
                        Store.set setter Atoms.ctrlPressed e.ctrlKey
                })

        nothing


module ShiftListener =
    [<ReactComponent>]
    let ShiftListener () =
        Listener.useKeyPress
            [|
                "Shift"
            |]
            (fun getter setter e ->
                promise {
                    let shiftPressed = Store.value getter Atoms.shiftPressed

                    if e.shiftKey <> shiftPressed then
                        Store.set setter Atoms.shiftPressed e.shiftKey
                })

        Listener.useKeyPress
            [|
                "I"
                "H"
                "P"
                "B"
            |]
            (fun _ setter e ->
                promise {
                    let setView value = Store.set setter Atoms.view value

                    match e.ctrlKey, e.altKey, e.key with
                    | true, true, "I" ->
                        JS.log (fun () -> "RouterObserver.onKeyDown() View.Information")
                        setView View.View.Information
                    | true, true, "H" -> setView View.View.HabitTracker
                    | true, true, "P" -> setView View.View.Priority
                    | true, true, "B" -> setView View.View.BulletJournal
                    | _ -> ()
                })


        nothing


module SelectionListener =
    [<ReactComponent>]
    let SelectionListener () =
        Listener.useKeyPress
            [|
                "Escape"
                "R"
            |]
            (fun getter setter e ->
                promise {
                    let cellSelectionMap = Store.value getter Selectors.Session.cellSelectionMap

                    if e.key = "Escape" && e.``type`` = "keydown" then
                        if not cellSelectionMap.IsEmpty then
                            cellSelectionMap
                            |> Map.keys
                            |> Seq.iter (fun taskId -> Store.set setter (Atoms.Task.selectionSet taskId) Set.empty)

                    if e.key = "R" && e.``type`` = "keydown" then
                        if not cellSelectionMap.IsEmpty then
                            let newMap =
                                if cellSelectionMap.Count = 1 then
                                    cellSelectionMap
                                    |> Map.toList
                                    |> List.map
                                        (fun (taskId, dates) ->
                                            let date =
                                                dates
                                                |> Seq.item (Random().Next (0, dates.Count - 1))

                                            taskId, Set.singleton date)
                                    |> Map.ofSeq
                                else
                                    let key =
                                        cellSelectionMap
                                        |> Map.keys
                                        |> Seq.item (Random().Next (0, cellSelectionMap.Count - 1))

                                    Map.singleton key cellSelectionMap.[key]

                            newMap
                            |> Map.iter
                                (fun taskId dates ->
                                    Store.set setter (Atoms.Task.selectionSet taskId) (dates |> Set.map DateId))
                })

        nothing


module PasteListener =
    type Browser.Types.Clipboard with
        [<Emit "$0.read()">]
        member _.read () = jsNative

//    const blobToBase64 = (blob) => {
//  return new Promise((resolve) => {
//    const reader = new FileReader();
//    reader.readAsDataURL(blob);
//    reader.onloadend = function () {
//      resolve(reader.result);
//    };
//  });
//};
    let blobToBase64 blob =
        Fable.SimpleHttp.FileReader.readBlobAsText blob|>Async.StartAsPromise
//        Promise.create
//            (fun res err ->
//                let str = Fable.SimpleHttp.FileReader.readBlobAsText blob|>Async.StartAsPromise
//                res (str|>Async.StartAsPromise)
//
////                reader.readAsDataURL blob
////                reader.onload <- fun () -> res reader.result
//                )


    [<ReactComponent>]
    let PasteListener () =

        //      React.useEffect(() => {
//        if (!navigator.clipboard) {
//          setError({
//            message: 'Clipboard API not available',
//            link: 'https://caniuse.com/clipboard'
//          });
//          return;
//        }
//
//        function handlePasteEvent(event) {
//          handleClipboard();
//          event.preventDefault();
//        }
//
//        document.addEventListener('paste', handlePasteEvent);
//
//        return () => {
//          document.removeEventListener('paste', handlePasteEvent);
//        };
//      }, []);

        let toast = Chakra.useToast ()

        let handlePasteEvent =
            Store.useCallback (
                (fun _ _ (event: Browser.Types.Event) ->
                    promise {
                        match Browser.Navigator.navigator.clipboard with
                        | Some clipboard ->
                            let! read = clipboard.read ()
                            printfn $"read={read}"

                            Browser.Dom.window?read <- read

                            let! blobs =
                                read
                                |> Array.collect
                                    (fun (clipboardItem: {| types: string []
                                                            getType: string -> JS.Promise<Blob> |}) ->
                                        clipboardItem.types
                                        |> Array.filter (fun t -> t.StartsWith "image/")
                                        |> Array.map (fun x -> clipboardItem.getType (unbox x)))
                                |> Promise.Parallel

                            let imageObjectUrls =
                                blobs |> Array.map Browser.Url.URL.createObjectURL

                            printfn $"blobs={blobs} urls={blobs |> Array.map Browser.Url.URL.createObjectURL}"

                            let! str = blobToBase64 blobs.[0]

                            printfn $"img! result={str}"

                            Browser.Dom.window?blobs <- blobs

                        | None -> toast (fun x -> x.description <- "Clipboard not available")

                        event.preventDefault ()
                    }),
                [|
                    box toast
                |]
            )

        let handle =
            React.useMemo (
                (fun () -> handlePasteEvent >> Promise.start),
                [|
                    box handlePasteEvent
                |]
            )

        React.useDisposableEffect (
            (fun disposed ->
                if not disposed then
                    Browser.Dom.document.addEventListener ("paste", handle)
                else
                    Browser.Dom.document.removeEventListener ("paste", handle)),
            [|
                box handle
            |]
        )

        Listener.useKeyPress
            [|
                "V"
            |]
            (fun _ _setter e -> promise { if e.ctrlKey then () })

        nothing
