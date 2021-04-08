namespace Fluke.UI.Backend

open Fable.Remoting.Json
open Newtonsoft.Json


module Json =
    let converter = FableJsonConverter ()

    let serialize (value: 'a) =
        JsonConvert.SerializeObject (value, converter)

    let inline deserialize<'a> (json: string) =
        if typeof<'a> = typeof<string> then
            unbox<'a> (box json)
        else
            JsonConvert.DeserializeObject (json, typeof<'a>, converter) :?> 'a
