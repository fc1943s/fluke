namespace Fluke.FileSystem.Cli

open System.IO
open Expecto
open Expecto.Flip
open Fluke.Shared
open Fluke.Shared.Domain
open FSharpPlus



module Tests =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    module Data =
        let directories =
            {|
                Main = @"C:\home"
                Others =
                    [
                        @"C:\home\fs\onedrive\home", "onedrive"
                    ]
            |}

    let tests =
        testList
            "Tests"
            [

                testList
                    "FileSystem"
                    [

                        test "1" {
                            1 |> Expect.equal "" 1

                            //                            let baseState = RootPrivateData.State.getBaseState ()


                            //                            let state2 =
//                                {|
//                                    User = baseState.Session.User
//                                    TreeStateMap = baseState.Session.TreeStateMap
//                                |}

                            ////                            let fsharpJson = FSharp.Json.Json.serialize state2
//                            let thoth = Thoth.Json.Net.Encode.Auto.toString(0, state2)
////                            let stj = System.Text.Json.JsonSerializer.Serialize(state2)
//
//                            let remoting = Newtonsoft.Json.JsonConvert.SerializeObject(state2, [| Fable.Remoting.Json.FableJsonConverter() :> Newtonsoft.Json.JsonConverter |])
//
////                            File.WriteAllText("./fsharpJson.json", fsharpJson)
//                            File.WriteAllText("./thoth.json", thoth)
////                            File.WriteAllText("./stj.json", stj)
//                            File.WriteAllText("./remoting.json", remoting)


                            []
                            |> List.map (fun information ->
                                match information with
                                | Archive _ -> None
                                | information -> Some (Information.toString information, information.Name))
                            |> List.choose id
                            |> List.groupBy fst
                            |> List.map (fun (informationKind, informationNameList) ->
                                informationKind, informationNameList |> List.map snd)
                            |> List.map (fun (informationKind, informationNameList) ->
                                let getDirectoriesIo homePath =
                                    Directory.GetDirectories (Path.Combine (homePath, informationKind), "*.*")
                                    |> Array.map (fun path ->
                                        Path.GetDirectoryName path,
                                        Path.GetFileName path,
                                        (FileInfo path).Attributes.HasFlag FileAttributes.ReparsePoint)
                                    |> Array.toList

                                let mainDirectories = getDirectoriesIo Data.directories.Main

                                let otherDirectories =
                                    Data.directories.Others
                                    |> List.map (fun (otherPath, otherAlias) -> getDirectoriesIo otherPath, otherAlias)

                                informationNameList, mainDirectories, otherDirectories)
                            |> List.map (fun (informationNameList, mainDirectories, otherDirectories) ->

                                otherDirectories
                                |> List.iter (fun (otherDirectories, otherAlias) ->

                                    otherDirectories
                                    |> List.iter (fun (path, name, symlink) ->

                                        mainDirectories
                                        |> List.filter (fun (_, mainName, mainSymlink) ->
                                            not symlink
                                            && mainSymlink
                                            && mainName = sprintf "%s-%s" name otherAlias |> not)
                                        |> List.map (fun (mainPath, _mainName, _mainSymlink) ->
                                            printfn
                                                "Missing symlink: %s -> %s"
                                                (path + string Path.DirectorySeparatorChar + name)
                                                (mainPath
                                                 + string Path.DirectorySeparatorChar
                                                 + name
                                                 + "-"
                                                 + otherAlias))
                                        |> ignore

                                        //                            x |> ignore
                                        )
                                    //                        x |> ignore
                                    )

                                informationNameList, mainDirectories, otherDirectories)
                            |> fun x ->
                                x |> ignore
                                printfn "@ %A" x

                            ()
                        }
                    ]
            ]
