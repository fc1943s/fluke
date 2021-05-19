namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Cypress =
    [<Emit("describe($0, $1)")>]
    let describe (_title: string) (_fn: unit -> unit) = jsNative

    [<Emit("before($0)")>]
    let before (_fn: unit -> unit) = jsNative

    type ExpectTo<'T> =
        abstract contain : 'T -> unit

    type Expect<'T> =
        abstract ``to`` : ExpectTo<'T>

    [<Emit("expect($0)")>]
    let expect<'T> (_name: 'T) : Expect<'T> = jsNative

    [<Emit("it($0, $1)")>]
    let it (_name: string) (_fn: unit -> unit) = jsNative

    module Cy =
        type Chainable<'T> =
            abstract should : ('T -> unit) -> unit

        type Chainable2<'T> =
            abstract should : string -> string -> string -> unit
            abstract invoke : string -> string -> string -> Chainable2<'T>
            abstract click : unit -> Chainable2<'T>
            abstract contains : string -> Chainable2<'T>
            abstract debug : unit -> unit
            abstract clear : unit -> Chainable2<'T>
            abstract eq : int -> Chainable2<'T>
            abstract focus : unit -> unit
            abstract ``then`` : (Chainable2<'T> -> unit) -> unit
            abstract ``type`` : string -> Chainable2<'T>
            abstract scrollTo : string -> {| ensureScrollable: bool |} -> unit
            abstract get : string -> Chainable2<'T>
            abstract parents : string -> Chainable2<'T>
            abstract find : string -> Chainable2<'T>
            abstract children : string -> Chainable2<'T>

        type Location =
            abstract pathname : string
            abstract href : string
            abstract hash : string

        [<Emit("cy.location()")>]
        let location () : Chainable<Location> = jsNative

        [<Emit("cy.wrap($0)")>]
        let wrap<'T> (_el: Chainable2<'T>) : Chainable2<'T> = jsNative

        [<Emit("cy.focused()")>]
        let focused () : Chainable2<unit> = jsNative

        [<Emit("cy.visit($0)")>]
        let visit (_url: string) : unit = jsNative

        [<Emit("cy.pause()")>]
        let pause () : unit = jsNative

        [<Emit("cy.wait($0)")>]
        let wait (_time: int) : unit = jsNative

        [<Emit("cy.contains($0, $1)")>]
        let contains (_text: string) (_options: {| timeout: int |} option) : Chainable2<'T> = jsNative

        [<Emit("cy.get($0)")>]
        let get (_selector: string) : Chainable2<string> = jsNative
