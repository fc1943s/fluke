namespace Fluke.FileSystem.Cli

open System.IO
open Expecto
open Expecto.Flip
open Fluke.Shared
open FSharpPlus
open FSharp.Data


module Tests =
    open Model
    
    type BookmarksSchema = JsonProvider<PrivateData.Tests.bookmarksJsonPath>
    let tests = testList "Tests" [
        
        testList "FileSystem" [
            
            test "Bookmarks" {
                let bookmarks = BookmarksSchema.Load PrivateData.Tests.bookmarksJsonPath
                
                let data2 = [
                    for xa in bookmarks.Roots.BookmarkBar.Children do
                        if xa.Type = "url"
                        then xa.Name, None
                        else xa.Name, Some [
                            for xb in xa.Children do
                                if xb.Type = "url"
                                then xb.Name, None
                                else xb.Name, Some [
                                    for xc in xb.Children do
                                        xc.Name, None
                                ]
                        ]
                ]
                
                let data2 = [
                    bookmarks.Roots.BookmarkBar.Children
                    |> Array.map (function
                        | x when x.Type = "url" -> x.Name, [||]
                        | x -> x.Name, x.Children |> Array.map (function
                            | x when x.Type = "url" -> x.Name, [||]
                            | x -> x.Name, x.Children |> Array.map (function
                                | x when x.Type = "url" -> x.Name, [||]
                                | x -> x.Name, x.Children |> Array.map (function
                                    | x when x.Type = "url" -> x.Name, [||]
                                    | x -> x.Name, x.Children |> Array.map (function
                                        | x when x.Type = "url" -> x.Name, [||]
                                        | x -> x.Name, x.Children |> Array.map (function
                                            | x when x.Type = "url" -> x.Name, [||]
                                            | x -> x.Name, x.Children |> Array.map (function
                                                | _ -> x.Name, [||] )))))))
                            ] |> ignore
                    
                let data =
                    bookmarks.Roots.BookmarkBar.Children
                    |> Array.map (function
                        | x when x.Type = "url" -> x.Name, [||]
                        | x -> x.Name, x.Children |> Array.map (function
                            | x when x.Type = "url" -> x.Name, [||]
                            | x -> x.Name, x.Children |> Array.map (function
                                | x when x.Type = "url" -> x.Name, [||]
                                | x -> x.Name, x.Children |> Array.map (function
                                    | x when x.Type = "url" -> x.Name, [||]
                                    | x -> x.Name, x.Children |> Array.map (function
                                        | x when x.Type = "url" -> x.Name, [||]
                                        | x -> x.Name, x.Children |> Array.map (function
                                            | x when x.Type = "url" -> x.Name, [||]
                                            | x -> x.Name, x.Children |> Array.map (function
                                                | _ -> x.Name, [||] )))))))
                    |> ignore
                    
                    let rec loop q =
                        match q with
                        | (x: BookmarksSchema.Child[])  -> x
                    let rr =
                        loop bookmarks.Roots.BookmarkBar.Children
                    rr |> ignore
                    ()
                ()
            }
            
            test "Information Folders" {
                
                let taskData = PrivateData.Tasks.tempManualTasks
                    
                taskData.InformationList
                |> List.map (function
                    | Project project -> Some ("projects", project.Name)
                    | Area area -> Some ("areas", area.Name)
                    | Resource resource -> Some ("resources", resource.Name)
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
                                not symlink
                                && mainSymlink
                                && mainName = (sprintf "%s-%s" name otherAlias)
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

