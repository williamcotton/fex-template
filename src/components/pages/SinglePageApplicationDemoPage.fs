module SinglePageApplicationDemoPage

open Express
open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock

[<Import("default", "router")>]
let router : unit -> ExpressApp = jsNative

let SinglePageApplicationDemoRouter = router()

let spa = SinglePageApplicationDemoRouter

[<ReactComponent>]
let SinglePageApplicationDemoPage(props: {| color : string |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Single Page Application Demo"

        Html.p "This first approach uses the query string parameters of the URL to control the state of the page. This is ideal for sharing links and bookmarks, but it can be difficult to manage the state of complex applications."

        let queryColor = req.query?color

        req.FormButton {| baseAction = "/set-color-query"; name = "color"; value = "red"; buttonText = "Make Red "|}
        req.FormButton {| baseAction = "/set-color-query"; name = "color"; value = "green"; buttonText = "Make Green "|}
        req.FormButton {| baseAction = "/set-color-query"; name = "color"; value = "blue"; buttonText = "Make Blue "|}

        Html.div [
            prop.style [ style.color queryColor]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        Html.p "This second approach uses a GraphQL mutation to update the state of the page. This is ideal for complex applications that require a lot of persistant state management."

        let color = props?color

        req.FormButton {| baseAction = "/set-color-gql"; name = "color"; value = "red"; buttonText = "Make Red "|}
        req.FormButton {| baseAction = "/set-color-gql"; name = "color"; value = "green"; buttonText = "Make Green "|}
        req.FormButton {| baseAction = "/set-color-gql"; name = "color"; value = "blue"; buttonText = "Make Blue "|}

        Html.div [
            prop.style [ style.color color]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        Html.p "This third approach uses a useStore hook to manage the state of the page. This is ideal for complex applications that don't need to track or persist parts of the state of the application."

        let (stateColor, setStateColor) = React.useState ""

        Html.button [ prop.onClick (fun _ -> setStateColor "red"); prop.text "Make Red" ]
        Html.button [ prop.onClick (fun _ -> setStateColor "green"); prop.text "Make Green" ]
        Html.button [ prop.onClick (fun _ -> setStateColor "blue"); prop.text "Make Blue" ]

        Html.div [
            prop.style [ style.color stateColor]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

    ]

spa.get("/", fun req res next ->
    promise {
        let! response = 
            req 
            |> gql "query { color { color } }" {||} {| cache = false |}
            
        match response with
        | Ok response -> 
            let color = response?color?color
            SinglePageApplicationDemoPage ({| color = color |})
            |> res.renderComponent
        | Error message -> next()
    } |> ignore
)

spa.post("/set-color-query", fun req res next ->
    let color : string = req.body?color
    res?redirectBack({| color = color |})
)

spa.post("/set-color-gql", fun req res next ->
    let color : string = req.body?color
    promise {
        let! response = 
            req 
            |> gql "mutation ($color: String) { setColor(color: $color) { success } }" 
                {| color = color |} {||}

        match response with
        | Ok response -> 
            res.redirect("back")
        | Error message -> next()
    } |> ignore
)