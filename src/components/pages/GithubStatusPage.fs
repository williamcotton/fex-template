module GithubStatusPage

open Feliz
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock

[<ReactComponent>]
let GithubStatusPage(props: {| status : obj array |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "GitHub Status"
        props.status
        |> Array.map(fun (status: obj) ->
            let statusMessage : string = status?status
            let name : string = status?name
            let description : string = status?description
            Html.div [
                Html.h3 name
                Html.p statusMessage
                Html.p description
            ]
        )
        |> React.fragment
    ]