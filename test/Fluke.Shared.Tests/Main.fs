namespace Fluke.Shared

open System.Diagnostics.CodeAnalysis

open Expecto

module Main =

    [<ExcludeFromCodeCoverage>]
    [<EntryPoint>]
    let main args =
        let tests =
            Tests.tests
//            |> Test.filter " / " (fun x -> true)
        runTestsWithArgs defaultConfig args tests

