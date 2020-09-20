namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fable.Core
open Feliz.Recoil
open Fluke.UI.Frontend
open Fable.Core.JsInterop

module Chakra =
    [<AbstractClass>]
    type IChakra =
        abstract Box: obj

    [<ImportAll "@chakra-ui/core">]
    let chakra: IChakra = jsNative

    //    let Box (children: #seq<ReactElement>) = Interop.reactElementWithChildren "Box" children

    //    [<ImportMember("@chakra-ui/core")>]
//    let Box (children: #seq<ReactElement>): ReactElement = jsNative
//    printfn "chakra %A" (JS.JSON.stringify(chakra.Box))
//    Browser.Dom.window?box <- Box
    let box props children = ReactBindings.React.createElement (chakra.Box, props, children)


module TreeSelectorComponent =

    //    let chakra = JsInterop.importAll "@chakra-ui/core"

    let render =
        React.memo (fun () ->
            let state = Recoil.useValue Recoil.Atoms.state

            Chakra.box
                {| position = "relative" |}
                [
                    str "tree3"
                ])
