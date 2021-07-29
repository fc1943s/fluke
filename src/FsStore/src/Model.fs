namespace FsStore

open FsCore.Model

module FsStore =
    let collection = Collection (nameof FsStore)

module Model =

    type LogLevel =
        | Trace = 0
        | Debug = 1
        | Information = 2
        | Warning = 3
        | Error = 4
        | Critical = 5
