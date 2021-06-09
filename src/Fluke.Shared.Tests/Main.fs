namespace Fluke.Shared

open System.Diagnostics.CodeAnalysis
open Expecto


module Main =

    [<ExcludeFromCodeCoverage>]
    [<EntryPoint>]
    let main args =
        let tests =
            Tests.tests
//            |> Test.filter " / " (List.exists (fun x -> x.Contains "Sort by All"))
        runTestsWithArgs defaultConfig args tests
