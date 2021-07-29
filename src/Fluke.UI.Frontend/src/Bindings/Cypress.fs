namespace Fluke.UI.Frontend.Bindings

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop


module Cypress =
    let inline describe (title: string) (fn: unit -> unit) =
        emitJsExpr (title, fn) "describe($0, $1)"

    let inline before (_fn: unit -> unit) = emitJsExpr () "before($0)"

    type ExpectTo<'T> =
        abstract contain : 'T -> unit

    type Expect<'T> =
        abstract ``to`` : ExpectTo<'T>

    let inline expect<'T> (_name: 'T) : Expect<'T> = emitJsExpr () "expect($0)"

    let inline it (_name: string) (_fn: unit -> unit) = emitJsExpr () "it($0, $1)"

    module Cy =
        type Chainable<'T> =
            abstract should : ('T -> unit) -> unit

        type Chainable2<'T> =
            abstract should : string -> string -> string -> unit
            abstract invoke : string -> string -> string -> Chainable2<'T>
            abstract click : {| force: bool |} option -> Chainable2<'T>
            abstract contains : string -> {| timeout: int |} option -> Chainable2<'T>
            abstract debug : unit -> unit
            abstract clear : {| force: bool |} -> Chainable2<'T>
            abstract eq : int -> Chainable2<'T>
            abstract focus : unit -> unit
            abstract first : unit -> Chainable2<'T>
            abstract ``then`` : (Chainable2<'T> -> unit) -> unit
            abstract ``type`` : string -> {| force: bool |} -> Chainable2<'T>
            abstract scrollTo : string -> {| ensureScrollable: bool |} -> unit
            abstract get : string -> Chainable2<'T>
            abstract parents : string -> Chainable2<'T>
            abstract find : string -> Chainable2<'T>
            abstract children : string -> Chainable2<'T>

        type Location =
            abstract pathname : string
            abstract href : string
            abstract hash : string

        let inline location () : Chainable<Location> = emitJsExpr () "cy.location()"
        let inline wrap<'T> (_el: Chainable2<'T>) : Chainable2<'T> = emitJsExpr () "cy.wrap($0)"
        let inline focused () : Chainable2<unit> = emitJsExpr () "cy.focused()"
        let inline visit (_url: string) : unit = emitJsExpr () "cy.visit($0)"
        let inline pause () : unit = emitJsExpr () "cy.pause()"
        let inline wait (_time: int) : unit = emitJsExpr () "cy.wait($0)"
        let inline window () : JS.Promise<Window> = emitJsExpr () "cy.window()"

        let inline contains (_text: string) (_options: {| timeout: int |} option) : Chainable2<'T> =
            emitJsExpr () "cy.contains($0, $1)"

        let inline get (_selector: string) : Chainable2<string> = emitJsExpr () "cy.get($0)"
