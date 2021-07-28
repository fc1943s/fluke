namespace FsStore

open FsCore.Model


module Model =
    let collection = Collection (nameof FsStore)

    type LogLevel =
        | Trace = 0
        | Debug = 1
        | Information = 2
        | Warning = 3
        | Error = 4
        | Critical = 5
