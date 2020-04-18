namespace Fluke.FileSystem.Cli

open Argu
open Suigetsu.Core
open Suigetsu.CoreCLR

module Args =
    ()
        
module Main = 
    [<EntryPoint>]
    let main argv =
        fun () ->
            let args = Startup.parseArgsIo argv
            ()
        |> Startup.withLogging false

