module SinglePageApplicationAdvancedDemoPage

open Express
open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Global
open AppLayout
open Validus

[<Import("default", "router")>]
let router : unit -> ExpressApp = jsNative
let SinglePageApplicationAdvancedDemoRouter = router()
let private spa = SinglePageApplicationAdvancedDemoRouter

type SinglePageApplicationAdvancedDemoPageProps = {
    color : string
    colorError : string
    name : string
    nameError : string
    inputName : string
    inputEmail : string
    inputNameErrors : string array
    inputEmailErrors : string array
}

type InputNameAndEmail = {
    inputName : string
    inputEmail : string
    inputNameErrors : string array
    inputEmailErrors : string array
}

type Color = {
    color : string
    colorError : string
}

type Name = {
    name : string
    nameError : string
}

[<ReactComponent>]
let SinglePageApplicationAdvancedDemoPage(props: SinglePageApplicationAdvancedDemoPageProps) =
    let req = React.useContext requestContext
    React.fragment [
        let name = props.name
        let nameError = props.nameError

        match nameError with
        | "invalid-name" -> Html.p "Please enter your name:"
        | _ -> Html.p (sprintf "Hello, %s!" name)

        req.Form {| id = "setName"; baseAction = "/set-name"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.key "name"; prop.name "name"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Change Name" ]
        ] |}

        let color = props.color
        let colorError = props.colorError

        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "red"; buttonText = "Red"|}
        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "green"; buttonText = "Green"|}
        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "blue"; buttonText = "Blue"|}
        req.FormButton {| baseAction = "/set-color"; name = "color"; value = "error"; buttonText = "Error"|}

        match colorError with
        | "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
        | _ -> null

        Html.div [
            prop.style [ style.color color]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        let inputName = props.inputName
        let inputNameErrors = props.inputNameErrors
        let inputEmail = props.inputEmail
        let inputEmailErrors = props.inputEmailErrors

        req.Form {| baseAction = "/form-validation"; method = "post"; children = [
            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputName"; prop.text "Name" ]
                    textInputFieldWithStringListError "inputName" "Name" inputName (Some inputNameErrors)
                ]
            ]

            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputEmail"; prop.text "Email" ]
                    textInputFieldWithStringListError "inputEmail" "Email" inputEmail (Some inputEmailErrors)
                ]
            ]

            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Submit" ]
        ] |}
    ]

spa.get("/", fun req res next ->
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
            let props = {
                color = response?color?color
                colorError = req.query?colorError
                name = response?name?name
                nameError = req.query?nameError
                inputName = req.query?inputName
                inputEmail = req.query?inputEmail
                inputNameErrors = req.query?inputNameErrors
                inputEmailErrors = req.query?inputEmailErrors
            }

            SinglePageApplicationAdvancedDemoPage props
            |> res.renderComponent
        | Error message -> next()
    } |> ignore
)

spa.post("/set-color", fun req res next ->
    let newColor : string = req.body?color
    promise {
        let! response = 
            req 
            |> gql "mutation ($color: String) { setColor(color: $color) { success } }" 
                {| color = newColor |} {||}

        let color = 
          match response with
          | Ok response -> { colorError = ""; color = newColor}
          | Error message -> { colorError = message; color = newColor}

        res.redirectBackAndMergeQuery<Color>(color)
    } |> ignore
)

spa.post("/set-name", fun req res next ->
    let newName : string = req.body?name
    promise {
        let! response = 
            req 
            |> gql "mutation ($name: String) { setName(inputName: $name) { success } }" 
                {| name = newName |} {||}

        let name =
          match response with
          | Ok response -> { nameError = ""; name = newName }
          | Error message -> { nameError = message; name = newName }

        res.redirectBackAndMergeQuery<Name>(name)        
    } |> ignore
)

spa.post("/form-validation", fun req res next ->
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

            return {| inputName = inputName; inputEmail = inputEmail |}
        }
    
    let inputNameAndEmail = 
      match validatedInput with
      | Ok _ -> 
          { inputName = inputName; inputEmail = inputEmail; inputNameErrors = [||]; inputEmailErrors = [||] }
      | Error validationErrors ->
          let inputNameErrors = extractErrors validationErrors "inputName"
          let inputEmailErrors = extractErrors validationErrors "inputEmail"

          { inputNameErrors = inputNameErrors; inputEmailErrors = inputEmailErrors; inputName = inputName; inputEmail = inputEmail }
    consoleLog(inputNameAndEmail)
    
    res.redirectBackAndMergeQuery<InputNameAndEmail>(inputNameAndEmail)
)