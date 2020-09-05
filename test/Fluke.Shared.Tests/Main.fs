namespace Fluke.Shared

open System.Diagnostics.CodeAnalysis
open Expecto


module Main =

    [<ExcludeFromCodeCoverage>]
    [<EntryPoint>]
    let main args =
        let tests = Tests.tests
        //            |> Test.filter " / " (List.exists (fun x -> x.Contains "schedule for tomorrow with PendingAfter"))
        runTestsWithArgs defaultConfig args tests
