namespace Fluke.FileSystem.Cli

open Suigetsu.CoreCLR

module Args =
    ()
        
module Main = 
    [<EntryPoint>]
    let main argv =
        fun () ->
            let _args = Startup.parseArgsIo argv
            ()
        |> Startup.withLogging false

