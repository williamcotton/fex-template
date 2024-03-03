module LinksAndFormElementPage

open Feliz
open AppLayout
open CodeBlock

[<ReactComponent>]
let LinksAndFormElementPage(props: {| inputName : string option |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h3 "Link Elements"

        Html.p "Navigation is managed by React components, facilitating client-side routing that operates as a single-page application with dynamic data fetching. At the same time the Fex architectural pattern ensures the server-side rendering of the components, guarenteeing interaction before JavaScript is loaded."

        Html.p "Example navigation bar component:"

        CodeBlock {| 
            lang = "javascript"; code =
"""[<ReactComponent>]
let NavigationBar(props: {| greeting : Greeting |}) =
    let req = React.useContext requestContext
    Html.nav [
        req.Link {| href = "/"; children = "Home" |}
        req.Link {| href = "/about"; children = "About" |}
    ]
"""
        |}

        Html.h3 "Form Elements"

        Html.p "Forms integrate seamlessly, functioning on both client and server sides, enabling interactions without page reloads. The server also handles these form submissions, ensuring functionality even without JavaScript."

        Html.p "Example form component and handler:"

        CodeBlock {| lang = "fsharp"; code = 
"""[<ReactComponent>]
let LinksAndFormElementPage(props: {| inputName : string option |}) =
    let req = React.useContext requestContext
    React.fragment [
        match props.inputName with
        | Some name -> (Html.p ("Hello, " + name + "!"))
        | None -> Html.p "Please enter your name:"

        req.Form {| action = "/setName"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.name "inputName"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.value "Submit" ]
        ] |}
    ]
"""     |}

        CodeBlock {| lang = "fsharp"; code =
"""app.post("/setName", fun req res next ->
    promise {     
        let inputName = req.body?inputName
            let! response =
                req |> gql "mutation ($inputName: String) 
                           { setName(inputName: $inputName) { success } }" 
                           {| inputName = inputName |} {||}
        res.redirect("back")
    } |> ignore
)""" 
        |}

        Html.p "This is an example of the above form element components in action and shows how requests are handled without a page reload while still being structured around the foundational element of interactive web applications, the form post."
        
        match props.inputName with
        | Some name -> (Html.p ("Hello, " + name + "!"))
        | None -> Html.p "Please enter your name:"

        req.Form {| action = "/setName"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.key "inputName"; prop.name "inputName"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Submit" ]
        ] |}

        Html.p "If you disable JavaScript in your browser, form submissions will continue to work and update the state of the page, albeit with the obvious need for a page reload."

        Html.p "In some instances it might make sense to use a hook like useReducer to manage the state of some part of the page, but in most cases a simple stateless component will suffice, following the general approach of working with a functional programming language like F#."

        req.Link {| href = "/middleware"; children = "Next: Parallel Middleware" |}
    ]
