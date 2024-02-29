module CodeBlock

open Feliz
open Fable.Core

[<Import("default", "highlight.js")>]
let highlightjs : {| highlight: string -> obj -> {| value : string |} |} = jsNative

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
