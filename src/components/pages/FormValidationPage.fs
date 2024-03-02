module FormValidationPage

open Feliz
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock

type RequestBody =
    { inputName: string
      inputEmail: string }    

[<ReactComponent>]
let FormValidationPage(props: {| errors : Map<string, string list>; requestBody : RequestBody |})  =
    let req = React.useContext requestContext
    let errors = props.errors // Assuming errors is a Map with the field name as key and error message as value
    let requestBody = props.requestBody // Cast requestBody to the RequestBody type
    let inputName = requestBody.inputName
    let inputEmail = requestBody.inputEmail

    React.fragment [
        Html.h2 "Form Validation"

        Html.p "Form validation is a critical part of any web application. It ensures that the data submitted by the user is accurate and secure. Fex provides a simple and flexible approach to form validation based on the built-in Result types in F#."

        req.Form {| action = "/form-validation"; method = "post"; children = [
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

        let isFormSuccessfullySubmitted =
            not (isObjEmpty requestBody) && Map.isEmpty errors

        match isFormSuccessfullySubmitted with
        | true -> Html.p "Form submitted successfully"
        | _ -> null

        Html.p "F# is an excellent choice when it comes to validating form data. It's a functional-first language that provides a powerful type system and a rich set of libraries for working with data. Below is an example using the Validus NuGet package."

        CodeBlock {| lang = "fsharp"; code =
"""app.post("/form-validation", fun req res next ->
    let requestBody = req.body :?> RequestBody
    let inputName = requestBody.inputName
    let inputEmail = requestBody.inputEmail
    
    let nameValidator fieldName =
        let msg = fun _ -> $"{fieldName} must be between 3 and 64 characters"
        Check.WithMessage.String.betweenLen 3 64 msg

    let validatedInput =  
        validate {
            let! inputName = nameValidator "Name" "inputName" inputName
            and! inputEmail = nameValidator "Email address" "inputEmail" inputEmail
            return {
                inputName = inputName
                inputEmail = inputEmail
            }
        }

    let errors = 
        match validatedInput with
        | Ok validInput -> Map.empty
        | Error e -> e |> ValidationErrors.toMap

    FormValidationPage ({| errors = errors; requestBody = requestBody |})
    |> res.renderComponent
)"""    |}

        Html.p "Again, since we're using the Fex architectural pattern we can turn off JavaScript in our web browsers and have the exact same experience."

        Html.p "Take a look in w3m, links, or lynx and you'll see that the form still works and the page still updates, allowing for the easy creation of a TUI web application!"

        req.Link {| href = "/request-response-cycle"; children = "Next: Request-Response Cycle Illustrated" |}
    ]