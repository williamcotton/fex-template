module App

open Feliz
open Fable.Core.JsInterop
open Express
open Global
open GraphQLSchema
open Components

let universalApp (app: ExpressApp) =
    app.get("/", fun req res next ->
        let inputName =
            if req.query?inputName <> "" then Some (req.query?inputName)
            else None
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
                FormElementPage ({| inputName = Some name?name |})
                |> res.renderComponent
            | Error message -> next()
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

    app.get("/about", fun req res next ->
        Html.div [
            prop.text "This is a simple template for building a website with Fable, Fable.React, and Feliz."
        ]
        |> res.renderComponent
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


    app.``use`` (fun (req: ExpressReq) (res: ExpressRes) next ->
        res.status 404 |> ignore
        Html.div "This page isn't here!"
        |> res.renderComponent
    )

    let errorHandler (err: obj) (req: ExpressReq) (res: ExpressRes) (next: unit -> unit) =
        match err with
        | :? System.Exception as ex ->
            let message = ex.Message
            consoleLog message
            res.status 500 |> ignore
            Html.div message
            |> res.renderComponent
        | _ -> next()

    app.``use`` errorHandler