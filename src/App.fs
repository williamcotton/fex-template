module App

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Express
open Global
open GraphQLSchema
open Components
open Validus

let universalApp (app: ExpressApp) =
    app.get("/", fun req res next ->
        promise {
            let! response = 
                req 
                |> gql "query { greeting { heading content } }" {||} {||}
                
            match response with
            | Ok response -> 
                let greeting : Greeting = response?greeting
                FrontPage ({| greeting = greeting |})
                |> res.renderComponent
            | Error message -> next()
        } |> ignore
    )

    app.get("/form-elements", fun req res next ->
        promise {
            let! response = 
                req 
                |> gql "query { name { name } }" {||} {| refresh = true; cache = false |}
                
            match response with
            | Ok response -> 
                let name : Name = response?name
                FormElementPage ({| inputName = Some name.name |})
                |> res.renderComponent
            | Error message -> next()
        } |> ignore
    )

    app.post("/setName", fun req res next ->
        promise {     
            let inputName = req.body?inputName
            let! response =
                req |> gql "mutation ($inputName: String) 
                           { setName(inputName: $inputName) { success } }" 
                           {| inputName = inputName |} {||}
            res.redirect("back")
        } |> ignore
    )

    app.get("/middleware", fun req res next ->
        promise {
            let! response = 
                req 
                |> gql "query { name { name } }" {||} {| refresh = true; cache = false |}
                
            match response with
            | Ok response -> 
                let name : Name = response?name
                MiddlewarePage ()
                |> res.renderComponent
            | Error message -> next()
        } |> ignore
    )

    app.get("/form-validation", fun req res next ->
        let errors = Map.empty
        let requestBody = { inputName = ""; inputEmail = "" }
        FormValidationPage ({| errors = errors; requestBody = requestBody |}) |> res.renderComponent
    )
    
    app.post("/form-validation", fun req res next ->
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
    )

    app.get("/about", fun req res next ->
        Html.div [
            prop.text "This is a simple template for building a website with Fable, Fable.React, and Feliz."
        ]
        |> res.renderComponent
    )

    // app.get("/weather", fun req res next ->
    //     promise {
    //         let! response = 
    //             fetch "https://api.weather.gov/gridpoints/TOP/32,81/forecast"
    //         let! json = response.json()
    //         consoleLog json
    //         let forecast : obj array = json?properties?periods
    //         res.renderComponent(WeatherPage {| forecast = forecast |})
    //     } |> ignore
    // )
    
    app.get("/weather", fun req res next ->
        promise {
            let! json = 
                req.fetchJson "https://api.weather.gov/gridpoints/TOP/32,81/forecast" {||} {||}
            consoleLog json
            let forecast : obj array = json?properties?periods
            res.renderComponent(WeatherPage {| forecast = forecast |})
        } |> ignore
    )

    app.get("/github_status", fun req res next ->
        promise {
            let! json = 
                req.fetchJson "https://www.githubstatus.com/api/v2/summary.json" {||} {||}
            let status : obj array = json?components
            res.renderComponent(GithubStatusPage {| status = status |})
        } |> ignore
    )
    
    app.get("/request-context", fun req res next ->
        RequestContextPage () |> res.renderComponent
    )

    app.get("/request-response-cycle", fun req res next ->
        RequestResponseCyclePage () |> res.renderComponent
    )
    
    app.``use`` (fun (req: ExpressReq) (res: ExpressRes) next ->
        res.status 404 |> ignore
        Html.div "This page isn't here!"
        |> res.renderComponent
    )

    let errorHandler (err: obj) (req: ExpressReq) (res: ExpressRes) (next: unit -> unit) =
        match err with
        | :? System.Exception as ex ->
            let message = ex.Message
            let stackTrace = ex.StackTrace
            consoleLog message
            res.status 500 |> ignore
            Html.div [
                Html.p message
                Html.pre stackTrace
            ]
            |> res.renderComponent
        | _ -> next()

    app.``use`` errorHandler