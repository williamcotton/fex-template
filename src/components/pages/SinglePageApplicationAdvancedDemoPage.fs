module SinglePageApplicationAdvancedDemoPage

open Express
open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock
open Validus


[<Import("default", "router")>]
let router : unit -> ExpressApp = jsNative

let SinglePageApplicationAdvancedDemoRouter = router()

let spaa = SinglePageApplicationAdvancedDemoRouter

let textInputFieldWithError fieldName placeholder (defaultValue: string) (errors : Map<string,string list>) =
    let flattenedErrors = errors |> Map.map(fun k v -> String.concat " " v)
    let hasError fieldName = errors.ContainsKey(fieldName)
    let getError fieldName = if hasError fieldName then flattenedErrors.[fieldName] else ""
    if hasError fieldName then 
        React.fragment [
            Html.input [ prop.type' "text"; prop.key fieldName; prop.name fieldName; prop.placeholder placeholder; prop.className "error"; (prop.defaultValue defaultValue) ]
            Html.p [ prop.text (getError fieldName); prop.className "error"]
        ]
    else
        Html.input [ prop.type' "text"; prop.key fieldName; prop.name fieldName; prop.placeholder placeholder; (prop.defaultValue defaultValue) ]

[<ReactComponent>]
let SinglePageApplicationAdvancedDemoPage(props: {| color : string; name : string; error : string; inputName : string; inputEmail : string |}) =
    let req = React.useContext requestContext
    React.fragment [
      
        let color = props.color
        let name = props.name
        let inputName = props.inputName
        let inputEmail = props.inputEmail
        let error = props.error

        // Defining a function to update the error map based on the error prop
        let updateErrorMap (error: string) (errors: Map<string, string>): Map<string, string> =
            match error with
            | "invalid-color" -> Map.add "color" "Invalid color. Please select red, green, or blue." errors
            | "invalid-name" -> Map.add "inputName" "Name must be between 3 and 64 characters" errors
            | _ -> errors


        consoleLog error

        // Initialize errors map and update based on props.error
        let errors = Map.empty
        let errorMap = updateErrorMap error errors

        match name with
        | "" -> Html.p "Please enter your name:"
        | _ -> Html.p (sprintf "Hello, %s!" name)

        req.Form {| id = "setName"; baseAction = "/set-name"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.key "name"; prop.name "name"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Change Name" ]
        ] |}

        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "red"; buttonText = "Red"|}
        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "green"; buttonText = "Green"|}
        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "blue"; buttonText = "Blue"|}
        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "error"; buttonText = "Error"|}

        match props.error with
        | "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
        | _ -> null

        Html.div [
            prop.style [ style.color color]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        req.Form {| baseAction = "/form-validation"; method = "post"; children = [
            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputName"; prop.text "Name" ]
                    textInputFieldWithError "inputName" "Name" inputName errors
                ]
            ]
            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputEmail"; prop.text "Email" ]
                    textInputFieldWithError "inputEmail" "Email" inputEmail errors
                ]
            ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Submit" ]
        ] |}
    ]

spaa.get("/", fun req res next ->
    let error = req.query?error
    promise {
        let! response = 
            req 
            |> gql 
              "
              query { 
                  color { color } 
                  name { name }
              }
              " {||} {| cache = false |}
            
        match response with
        | Ok response -> 
            let color = response?color?color
            let name = response?name?name
            SinglePageApplicationAdvancedDemoPage {| color = color; name = name; error = error; inputName = ""; inputEmail = "" |}
            |> res.renderComponent
        | Error message -> next()
    } |> ignore
)

spaa.post("/set-color", fun req res next ->
    let color : string = req.body?color
    promise {
        let! response = 
            req 
            |> gql "mutation ($color: String) { setColor(color: $color) { success } }" 
                {| color = color |} {||}

        match response with
        | Ok response -> res.redirectBack()
        | Error message -> res?redirectBack({| error = message |})
    } |> ignore
)

spaa.post("/set-name", fun req res next ->
    let name : string = req.body?name
    promise {
        let! response = 
            req 
            |> gql "mutation ($name: String) { setName(inputName: $name) { success } }" 
                {| name = name |} {||}

        consoleLog response

        match response with
        | Ok response -> res.redirectBack()
        | Error message -> res?redirectBack({| error = message |})
    } |> ignore
)

spaa.post("/form-validation", fun req res next ->
    let requestBody = req.body
    let inputName = requestBody?inputName
    let inputEmail = requestBody?inputEmail
    
    let nameValidator fieldName =
        let msg = fun _ -> $"{fieldName} must be between 3 and 64 characters"
        Check.WithMessage.String.betweenLen 3 64 msg

    let emailValidator =
        ValidatorGroup(Check.WithMessage.String.betweenLen 7 256 (fun _ -> "Email must be between 7 and 256 characters"))                                                  
            .And(Check.WithMessage.String.pattern @"[^@]+@[^\.]+\..+" (fun _ -> "Please provide a valid email address"))
            .Build()

    let validatedInput =  
        validate {
            let! inputName = nameValidator "Name" "inputName" inputName
            and! inputEmail = emailValidator "inputEmail" inputEmail
            return {|
                inputName = inputName
                inputEmail = inputEmail
            |}
        }
    
    let errors = 
        match validatedInput with
        | Ok validInput -> Map.empty
        | Error e -> e |> ValidationErrors.toMap

    match validatedInput with
      | Ok response -> res.redirectBack({| inputName = inputName; inputEmail = inputEmail |})
      | Error message -> res?redirectBack({| error = errors |})

)