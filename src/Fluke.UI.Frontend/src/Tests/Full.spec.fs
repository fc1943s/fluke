namespace Fluke.UI.Frontend.Tests

open Fable.Core
open Fluke.UI.Frontend.Bindings
//open Fable.Core.JsInterop


module Full =
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
            abstract should : string -> string -> 'T -> unit

        type Location =
            abstract pathname : string
            abstract href : string
            abstract hash : string

        [<Emit("cy.location()")>]
        let location () : Chainable<Location> = jsNative

        [<Emit("cy.visit($0)")>]
        let visit (_url: string) : unit = jsNative

        [<Emit("cy.get($0)")>]
        let get (_selector: string) : Chainable2<string> = jsNative


    describe
        "tests"
        (fun () ->
            let homeUrl = "https://localhost:33922"
            before (fun () -> Cy.visit homeUrl)

            it
                "login"
                (fun () ->
                    Cy
                        .location()
                        .should (fun location ->
                            expect(location.href)
                                .``to``.contain $"{homeUrl}/#/login")

                    Cy.get("body").should "have.css" "background-color" "rgb(33, 33, 33)"))
