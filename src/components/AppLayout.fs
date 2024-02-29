module AppLayout

open Express
open Feliz

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
                            Html.li [
                                Link {| href = "/github_status"; children = "GitHub Status" |}
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