namespace Fluke.FileSystem.Cli

open System.IO
open Expecto
open Expecto.Flip
open Fluke.Shared
open FSharpPlus


module Tests =
    open Model

    let tests = testList "Tests" [

        testList "FileSystem" [

            test "1" {
                1
                |> Expect.equal "" 1

                let treeData = RootPrivateData.treeData

                treeData.InformationList
                |> List.map (fun information ->
                    match information with
                    | Project project -> Some (information.KindName, project.Name)
                    | Area area -> Some (information.KindName, area.Name)
                    | Resource resource -> Some (information.KindName, resource.Name)
                    | Archive _ -> None
                )
                |> List.choose id
                |> List.groupBy fst
                |> List.map (fun (informationName, informationList) ->
                    informationName, informationList |> List.map snd
                )
                |> List.map (fun (informationName, informationList) ->
                    let getDirectoriesIo homePath =
                        Directory.GetDirectories (Path.Combine (homePath, informationName), "*.*")
                        |> Array.map (fun path ->
                            Path.GetDirectoryName path,
                            Path.GetFileName path,
                            (FileInfo path).Attributes.HasFlag FileAttributes.ReparsePoint
                        )
                        |> Array.toList

                    let mainDirectories = getDirectoriesIo PrivateData.Tests.directories.Main

                    let otherDirectories =
                        PrivateData.Tests.directories.Others
                        |> List.map (fun (otherPath, otherAlias) ->
                            getDirectoriesIo otherPath, otherAlias
                        )

                    informationList, mainDirectories, otherDirectories
                )
                |> List.map (fun (informationList, mainDirectories, otherDirectories) ->

                    otherDirectories
                    |> List.iter (fun (otherDirectories, otherAlias) ->

                        otherDirectories
                        |> List.iter (fun (path, name, symlink) ->

                            mainDirectories
                            |> List.filter (fun (_, mainName, mainSymlink) ->
                                not symlink &&
                                mainSymlink &&
                                mainName = sprintf "%s-%s" name otherAlias
                                |> not
                            )
                            |> List.map (fun (mainPath, _mainName, _mainSymlink) ->
                                printfn "Missing symlink: %s -> %s"
                                     (path + string Path.DirectorySeparatorChar + name)
                                     (mainPath + string Path.DirectorySeparatorChar + name + "-" + otherAlias)
                            ) |> ignore

//                            x |> ignore
                        )
//                        x |> ignore
                    )

                    informationList, mainDirectories, otherDirectories
                )
                |> fun x ->
                    x |> ignore
                    printfn "@ %A" x

                ()
            }
        ]
    ]

