namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop


module Vim =
    let private Vim: obj -> obj = import "Vim" "react-vim-wasm"

    let render
        (input: {| OnVimCreated: obj -> unit
                   Props: UI.IChakraProps -> unit
                   Fallback: unit -> ReactElement |})
        =
        match emitJsExpr () "typeof SharedArrayBuffer" with
        | "function" ->
            UI.box
                (fun x -> input.Props x)
                [
                    ReactBindings.React.createElement (
                        Vim,
                        {|
                            style = {| width = "100%"; height = "100%" |}
                            cmdArgs =
                                [|
                                    "-c set guifont=\"Roboto Condensed:h13\"; set background=white"
                                |]
                            onVimCreated = input.OnVimCreated
                            files =
                                {|
                                    file1 = "/home/web_user/.vim/file1"
                                |}
                            persistentDirs =
                                [|
                                    "/home/web_user/.vim"
                                |]
                            onError = fun e -> printfn $"wasm error {e}"
                            worker = "static/js/vim-wasm/vim.js"
                        |},
                        []
                    )
                ]
        | _ -> input.Fallback ()

    ()


//    import * as React from 'react';
//    import { useCallback } from 'react';
//    import { Vim } from 'react-vim-wasm';
//
//    const onVimExit = useCallback(s => alert(`Vim exited with status ${s}`), []);
//    const onFileExport = useCallback((f, buf) => console.log('file exported:', f, buf), []);
//    const onError = useCallback(e => alert(`Error! ${e.message}`), []);
//
//    <Vim
//        worker="/path/to/vim-wasm/vim.js"
//        onVimExit={onVimExit}
//        onFileExport={onFileExport}
//        readClipboard={navigator.clipboard && navigator.clipboard.readText}
//        onWriteClipboard={navigator.clipboard && navigator.clipboard.writeText}
//        onError={onError}
//    />
