module FrontPage

open Feliz
open GraphQLSchema
open AppLayout
open CodeBlock

[<ReactComponent>]
let Counter() =
    let (count, setCount) = React.useState(0)
    Html.div [
        prop.style [ style.marginBottom 20 ]
        prop.children [ 
            Html.button [
                prop.style [ style.marginRight 5 ]
                prop.onClick (fun _ -> setCount(count + 1))
                prop.text "Increment"
            ]

            Html.button [
                prop.style [ style.marginLeft 5; style.marginRight 15 ]
                prop.onClick (fun _ -> setCount(count - 1))
                prop.text "Decrement"
            ]

            Html.span count
        ]
    ]

[<ReactComponent>]
let FrontPage(props: {| greeting : Greeting |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 props.greeting.heading

        Html.h4 props.greeting.subheading

        Html.p props.greeting.content

        Html.h3 "Universal Route Handlers"

        Html.p "Fex enables efficient server-side rendering of static HTML through simple route handlers, data queries, and React components. This initial server-side rendering delivers fast load times."
        
        Html.p "Meanwhile, the same route handlers and components power client-side interactions without full page reloads allowing for all of the benefits of interactive single-page applications."

        Html.p "Here's an example of a Felix component and a Fex route handler:"

        CodeBlock {| 
            lang = "fsharp"; code =
"""[<ReactComponent>]
let ShowGreeting(props: {| greeting : Greeting |}) =
    React.fragment [
        Html.h2 props.greeting.heading
        Html.h4 props.greeting.subheading
        Html.p props.greeting.content
    ]

app.get("/", fun req res next ->
    promise {
        let! response = 
            req 
            |> gql "query { greeting { heading subheading content } }" {||}
            
        match response with
        | Ok response -> 
            let greeting : Greeting = response?greeting
            ShowGreeting ({| greeting = greeting |})
            |> res.renderComponent
        | Error message -> next()    
)
"""     |}

        Html.p "What underlies this functionality is that `res.renderComponent` and `req.gql` are attached to the request and response objects by parallel middleware. These middleware have separate implementations for the server and the browser but offer the same typed interface."

        Html.h3 "Universal React Components"

        Html.p "React components can be crafted in F# using Feliz, allowing for both server-rendered HTML and client-side interactivity. Here's an example of both a server rendered and an interactive counter component, which should look very familiar to a React developer:"

        Counter()

        CodeBlock {| 
            lang = "fsharp"; code =
"""[<ReactComponent>]
let Counter() =
    let (count, setCount) = React.useState(0)
    Html.div [
        Html.button [
            prop.style [ style.marginRight 5 ]
            prop.onClick (fun _ -> setCount(count + 1))
            prop.text "Increment"
        ]

        Html.button [
            prop.style [ style.marginLeft 5 ]
            prop.onClick (fun _ -> setCount(count - 1))
            prop.text "Decrement"
        ]

        Html.h1 count
    ]
"""     |}

        req.Link {| href = "/link-and-form-elements"; children = "Next: Links and Form Elements" |}
    ]
