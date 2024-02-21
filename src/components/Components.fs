module Components

open Feliz
open Global
open GraphQLSchema
open Fable.Core
open Express

[<Import("default", "highlight.js")>]
let highlightjs : {| highlight: string -> obj -> {| value : string |} |} = jsNative

let requestContext = React.createContext(name="Request")

[<ReactComponent>]
let AppLayout (props: {| content: ReactElement; req: ExpressReq |}) =
    let Link = props.req.Link
    React.contextProvider(requestContext, props.req, React.fragment [
        Html.div [
            prop.className "sitewrapper"
            prop.children [
                Html.header [
                    Html.h1 [
                        Link {| href = "/"; children = "fex-template" |}
                    ]
                    Html.nav [
                        Html.ul [
                            Html.li [
                                Link {| href = "/about"; children = "About" |}
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "content"
                    prop.children props.content
                ]
                Html.footer [
                    prop.children [
                        Html.p [
                            prop.text "Fex Template"
                        ]
                    ]
                ]
            ]
        ]
    ])

[<ReactComponent>]
let CodeBlock (props: {| code: string; lang: string |}) =
    let code = highlightjs.highlight props.code {| language = props.lang |}
    React.fragment [ 
        Html.pre [ 
            Html.code [ 
                prop.dangerouslySetInnerHTML code.value
            ]
        ]
    ]

[<ReactComponent>]
let FrontPage(props: {| greeting : Greeting; inputName : string option |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 [
            prop.text props.greeting.heading
        ]
        Html.p [
            prop.text props.greeting.content
        ]
        Html.p [
            prop.text "This simple route handler generates static HTML on the server and then rehydrates the same HTML on the client."
        ]
        CodeBlock {| 
            lang = "javascript"; code =
"""app.get("/about", fun req res next ->
    Html.div [
        prop.text "This is a simple template for building a website with Fable, Fable.React, and Feliz"
    ]
    |> res.renderComponent
)
""" 
        |}
        Html.p [
            Html.span "Links are handled by a React component in the context of the incoming request and is seen by this link to the ";
            req.Link {| href = "/about"; children = "About" |}
            Html.span " page."
        ]
        CodeBlock {| 
            lang = "javascript"; code = """req.Link {| href = "/about"; children = "About" |}"""
        |}
        Html.p [
            Html.span "Form elements are also handled by a React component in the context of the incoming request: ";
            req.Form {| action = "/name"; method = "post"; children = [
                Html.input [ prop.type' "text"; prop.name "inputName"; prop.placeholder "Name" ]
                Html.input [ prop.type' "submit"; prop.value "Submit" ]
            ] |}
        ]
    ]
