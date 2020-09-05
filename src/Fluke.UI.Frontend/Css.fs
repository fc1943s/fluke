namespace Fluke.UI.Frontend

open Zanaptak.TypedCssClasses


[<AutoOpen>]
module Css =
    type Css = CssClasses<"public/index.scss", Naming.CamelCase>
