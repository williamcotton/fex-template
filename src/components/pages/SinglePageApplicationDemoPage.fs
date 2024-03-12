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

let private spa = SinglePageApplicationDemoRouter


[<ReactComponent>]
let BaseRouteCodeBlock =
    CodeBlock {| lang = "fsharp"; code =
"""spa.get("/", fun req res next ->
    let queryColor = req.query?color
    let qError =
        match req.query?color with
        | "red" | "green" | "blue" -> ""
        | _ -> "invalid-color"

    promise {
        let! response = 
            req 
            |> gql "query { color { color } }" {||} {| cache = false |}
            
        match response with
        | Ok response -> 
            let gqlColor = response?color?color
            SinglePageApplicationDemoPage {| gqlColor = gqlColor; queryColor = queryColor; gqlError = ""; qError = qError |}
            |> res.renderComponent
        | Error message -> next()
    } |> ignore
)""" 
    |}

[<ReactComponent>]
let QueryStringButtonsCodeBlock =
    CodeBlock {| lang = "fsharp"; code =
"""req.FormButton {| baseAction = "/set-color-query"; name = "color"; value = "red"; buttonText = "Red"|}
req.FormButton {| baseAction = "/set-color-query"; name = "color"; value = "green"; buttonText = "Green"|}
req.FormButton {| baseAction = "/set-color-query"; name = "color"; value = "blue"; buttonText = "Blue"|}
req.FormButton {| baseAction = "/set-color-query"; name = "color"; value = "error"; buttonText = "Error"|}

match props.qError with
| "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
| _ -> null""" 
    |}

[<ReactComponent>]
let QueryStringPostHandlerCodeBlock =
    CodeBlock {| lang = "fsharp"; code =
"""spa.post("/set-color-query", fun req res next ->
    let color : string = req.body?color
    match color with
    | "red" | "green" | "blue" -> 
        res?redirectBackWithNewQuery({| color = color |})
    | _ -> res?redirectBackWithNewQuery({| error = "invalid-color" |})
)""" 
    |}   

[<ReactComponent>]
let GraphQLButtonsCodeBlock =
    CodeBlock {| lang = "fsharp"; code =
"""req.FormButton {| baseAction = "/"; name = "color"; value = "red"; buttonText = "Red"|}
req.FormButton {| baseAction = "/"; name = "color"; value = "green"; buttonText = "Green"|}
req.FormButton {| baseAction = "/"; name = "color"; value = "blue"; buttonText = "Blue"|}
req.FormButton {| baseAction = "/"; name = "color"; value = "error"; buttonText = "Error"|}

match props.gqlError with
| "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
| _ -> null""" 
    |}

[<ReactComponent>]
let GraphQLPostHandlerCodeBlock =
    CodeBlock {| lang = "fsharp"; code =
"""spa.post("/", fun req res next ->
    let color : string = req.body?color
    promise {
        let! response = 
            req 
            |> gql "mutation ($color: String) { setColor(color: $color) { success } }" 
                {| color = color |} {||}

        match response with
        | Ok response ->
            res.redirectBackWithNewQuery()
        | Error message ->
            SinglePageApplicationDemoPage ({| gqlColor = ""; queryColor = ""; gqlError = message; qError = "" |})
            |> res.renderErrorComponent
    } |> ignore
)""" 
    |}

[<ReactComponent>]
let UseStateButtonsCodeBlock =
    CodeBlock {| lang = "fsharp"; code =
"""let (stateColor, setStateColor) = React.useState ""

Html.button [ prop.onClick (fun _ -> setStateColor "red"); prop.text "Red" ]
Html.button [ prop.onClick (fun _ -> setStateColor "green"); prop.text "Green" ]
Html.button [ prop.onClick (fun _ -> setStateColor "blue"); prop.text "Blue" ]
Html.button [ prop.onClick (fun _ -> setStateColor "error"); prop.text "Error" ]

match stateColor with
| "error" -> Html.p "Invalid color. Please select red, green, or blue."
| _ -> null"""    
    |}

[<ReactComponent>]
let SinglePageApplicationDemoPage(props: {| gqlName: string; gqlColor : string; queryColor : string; gqlError : string; qError : string |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Single Page Application Demo"

        Html.p "This page demonstrates three different approaches to managing the state of a single page application."

        BaseRouteCodeBlock

        Html.h3 [ prop.text "Query String Parameters"; prop.id "setColorQuery" ]

        Html.p "This first approach uses the query string parameters of the URL to control the state of the page. This is ideal for sharing links and bookmarks, but it can be difficult to manage the state of complex applications. This approach will also work without JavaScript enabled."

        QueryStringButtonsCodeBlock
        QueryStringPostHandlerCodeBlock

        req.FormButton {| baseAction = "/set-color-query#setColorQuery"; name = "color"; value = "red"; buttonText = "Red"|}
        req.FormButton {| baseAction = "/set-color-query#setColorQuery"; name = "color"; value = "green"; buttonText = "Green"|}
        req.FormButton {| baseAction = "/set-color-query#setColorQuery"; name = "color"; value = "blue"; buttonText = "Blue"|}
        req.FormButton {| baseAction = "/set-color-query#setColorQuery"; name = "color"; value = "error"; buttonText = "Error"|}

        match props.qError with
        | "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
        | _ -> null

        Html.div [
            prop.style [ style.color props.queryColor]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        Html.h3 [ prop.text "Persistent State"; prop.id "setColorGql" ]

        Html.p "This second approach uses a GraphQL mutation in this case to update and persist the state of the page in the user's session data. This could just as easily use a separate CORS-enabled API with REST endpoints backed by a SQL database. This is ideal for complex applications that require a lot of persistant state management. This is another approach that will work without JavaScript enabled."

        GraphQLButtonsCodeBlock
        GraphQLPostHandlerCodeBlock

        let gqlColor = props.gqlColor

        let name = props.gqlName

        Html.p ("Hello, " + name)

        req.FormButton {| baseAction = "/set-color-gql#setColorGql"; name = "color"; value = "red"; buttonText = "Red"|}
        req.FormButton {| baseAction = "/set-color-gql#setColorGql"; name = "color"; value = "green"; buttonText = "Green"|}
        req.FormButton {| baseAction = "/set-color-gql#setColorGql"; name = "color"; value = "blue"; buttonText = "Blue"|}
        req.FormButton {| baseAction = "/set-color-gql#setColorGql"; name = "color"; value = "error"; buttonText = "Error"|}

        match props.gqlError with
        | "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
        | _ -> null

        Html.div [
            prop.style [ style.color gqlColor]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        Html.h3 "Temporary State"

        Html.p "This third approach uses a useStore hook to manage the state of the page. This is ideal for complex applications that don't need to track or persist parts of the state of the application. Since this is client-side only, it's not ideal for sharing links or bookmarks nor will it work without JavaScript enabled. Temporary state is best used to set up the conditions for creating persistent state."

        UseStateButtonsCodeBlock

        let (stateColor, setStateColor) = React.useState ""

        Html.button [ prop.onClick (fun _ -> setStateColor "red"); prop.text "Red" ]
        Html.button [ prop.onClick (fun _ -> setStateColor "green"); prop.text "Green" ]
        Html.button [ prop.onClick (fun _ -> setStateColor "blue"); prop.text "Blue" ]
        Html.button [ prop.onClick (fun _ -> setStateColor "error"); prop.text "Error" ]

        match stateColor with
        | "error" -> Html.p "Invalid color. Please select red, green, or blue."
        | _ -> null

        Html.div [
            prop.style [ style.color stateColor]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        Html.p "Together the three approaches demonstrate the flexibility of Fex to handle different state management needs and can be mixed and matched depending on then needs of the single page application. We'll see more of this approach in the next section."

        req.Link {| href = "/single-page-application-advanced-demo"; children = "Next: Single Page Application Advanced Demo" |}  
    ]

spa.get("/", fun req res next ->
    let queryColor = req.query?color
    let qError = req.query?qError
    let gqlError = req.query?gqlError

    promise {
        let! response = 
            req 
            |> gql "query { 
                color { color } 
                name { name }
            }" {||} {| cache = false |}

        match response with
        | Ok response -> 
            let gqlColor = response?color?color
            let gqlName = response?name?name
            SinglePageApplicationDemoPage {| gqlName = gqlName; gqlColor = gqlColor; queryColor = queryColor; gqlError = gqlError; qError = qError |}
            |> res.renderComponent
        | Error message -> next()
    } |> ignore
)

spa.post("/set-color-gql", fun req  res next ->
    let color : string = req.body?color
    promise {
        let! response = 
            req 
            |> gql "mutation ($color: String) { setColor(color: $color) { success } }" 
                {| color = color |} {||}

        match response with
        | Ok response -> res.redirect("back")
        | Error message -> res.redirectBackWithNewQuery({| gqlError = message |})
    } |> ignore
)

spa.post("/set-color-query", fun req res next ->
    let color : string = req.body?color
    match color with
    | "red" | "green" | "blue" -> res?redirectBackWithNewQuery({| color = color |})
    | _ -> res?redirectBackWithNewQuery({| qError = "invalid-color" |})
)