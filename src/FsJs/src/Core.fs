namespace FsJs

open Fable.DateFunctions
open System


module DateTime =
    let format format (dateTime: DateTime) = dateTime.Format format

    let addDays (days: int) (dateTime: DateTime) = dateTime.AddDays days
