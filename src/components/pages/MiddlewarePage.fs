module MiddlewarePage

open Feliz
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock

[<ReactComponent>]
let MiddlewarePage() =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Parallel Middleware"

        Html.p "Middleware is a powerful tool for managing requests and responses. It can be used to handle errors, parse requests, and manage sessions. Middleware can also be used to manage the flow of requests and responses, allowing for a wide range of customizations."

        Html.p "With the Fex architectural pattern, parallel middleware is used to allow for environment agnostic route handlers and components."

        Html.p "For example, our GraphQL server-side middleware directly makes a call to the GraphQL service."

        CodeBlock {| lang = "javascript"; code =
"""export default ({ schema, rootValue }) => (req, res, next) => {
    req.gql = async (query, variables) => {

      // ... some code here to handle caching and other options

      const response = await graphql(schema, query, rootValue, req, variables);
        
      const { data, errors } = response;

      if (errors) {
        const statusCode = errors[0].message === "NotFound" ? 404 : 500;
        throw new HTTPError(statusCode, errors[0].message);
      }

      req.dataQuery = {
        data,
        errors,
        query,
        variables,
      };

      return data;
    };
    next();
  };
""" 
        |}

        Html.p "Whereas the client-side middleware fetches from the server, which in turn makes a call to the GraphQL service."

        CodeBlock {| lang = "javascript"; code =
"""
export default ({ route }) => (req, res, next) => {
  req.gql = async (query, variables, options = {}) => {
    
    // ... some code here to handle caching and other options

    const fetchResponse = async () => {
      const response = await fetch(route, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json',
          'X-CSRF-Token': req.csrf,
        },
        body: JSON.stringify({ query, variables }),
      });
      return response.json()
    };

    const response = await fetchResponse()

    // ... some code here to handle caching and other options

    req.dataQuery = {
      data,
      errors,
      query,
      variables,
    };

    return data;
  };
""" 
        |}

        Html.p "Together this allows for a call to `req.gql` to offer the same interface for both the server-side and client-side middleware. In our example this allows for a consistent approach to handling GraphQL queries and mutations."

        req.Link {| href = "/form-validation"; children = "Next: Form Validation" |}
    ]