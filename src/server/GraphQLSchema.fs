module GraphQLSchema

open Fable.Core
open Fable.Core.JsInterop
open Global

type Greeting = {
    heading: string
    content: string
}


let schemaString = "
  scalar JSON

  type Greeting {
    heading: String
    content: String
  }

  type Query {
    greeting: Greeting
  }"

let rootValueInitializer =
    let greeting () =
        promise {
            let greeting = {
                heading = "Welcome to Fex"; 
                content = "Fex is a universal Express JavaScript web app framework using Fable, Fable.React, and Feliz. The same higher-order codebase can be used to render on the server and the client. It works by using both the `express` and `browser-express` npm packages, creating routes, middlware, and React components that can be used in both environments." 
            }
            return greeting
        }

    {|
        greeting = greeting;
    |}