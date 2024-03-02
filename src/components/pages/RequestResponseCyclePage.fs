module RequestResponseCyclePage

open Feliz
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock

[<ReactComponent>]
let serverRawRequest = 
    CodeBlock {| lang = "plaintext"; code =
"""GET / HTTP/1.1
Host: example.com
"""|}


[<ReactComponent>]
let serverSocketListener = 
    CodeBlock {| lang = "javascript"; code =
""" // from express/lib/application.js

app.listen = function listen() {
  var server = http.createServer(this);
  return server.listen.apply(server, arguments);
};
"""|}

[<ReactComponent>]
let serverExpressApp = 
    CodeBlock {| lang = "fsharp"; code =
"""// from server/Server.fs

let app = express()
useMiddleware(expressStatic("build"))
useMiddleware(cookieSession({| name = "session"; sameSite = "lax"; secret = sessionSecret |}))
useMiddleware(bodyParser.urlencoded({| extended = false |}))
useMiddleware(bodyParser.json())
useMiddleware(cookieParser())
useMiddleware(csurf())
useMiddlewareRoute "/graphql" (createHandler({| schema = schema.schema; rootValue = rootValue; graphiql = true; context = customContextFunction |}))
useMiddleware(graphqlClientMiddleware({| schema = schema.schema; rootValue = rootValue |}));
useMiddleware(fetchClientMiddleware())
useMiddleware(analyticstMiddleware({| analyticsRouter = analyticsRouter; app = app; analyticsPageview = analyticsPageviev; analyticsEvent = analyticsEvent |}))
useMiddleware(expressLinkMiddleware({| defaultTitle = defaultTitle |}))
useMiddleware(reactRendererMiddleware({| appLayout = AppLayout |}))

universalApp app

app.listen(int port, fun _ ->
    printfn "Listening on port %s" port
)
"""|}

[<ReactComponent>]
let serverMiddelware = 
    CodeBlock {| lang = "javascript"; code =
""" // server/middleware/express-link.js

export default ({ defaultTitle }) =>
  (req, res, next) => {
    req.csrf = req.csrfToken();

    res.expressLink = {
      queryCache: {},
      csrf: req.csrf,
      user: req.user,
      defaultTitle,
    };

    req.renderDocument = ({ renderedContent, title, description }) =>
      renderDocument({
        defaultTitle,
        expressLink: res.expressLink,
        renderedContent,
        title,
        description,
      });

    res.navigate = (path, query) => {
      const pathname = query ? `${path}?${qs.stringify(query)}` : path;
      res.redirect(pathname);
    };

    res.redirect = res.redirect.bind(res);

    res.cacheQuery = (key, data) => {
      res.expressLink.queryCache[key] = data;
    };

    next();
  };

};

// from server/middleware/react-renderer.js

export default ({ appLayout }) =>
  (req, res, next) => {
    req.Link = (props) => React.createElement("a", props);

    // ...

    next();
  };

"""|}

[<ReactComponent>]
let serverRenderHtml = 
    CodeBlock {| lang = "javascript"; code =
"""// from server/middleware/react-renderer.js

res.renderComponent = (content, options = {}) => {
    const layout = options.layout || appLayout || DefaultLayout;
    const renderedContent = renderToString(
    React.createElement(layout, { content, req })
    );
    const { title, description } = options;
    const statusCode = options.statusCode || 200;
    res.writeHead(statusCode, { "Content-Type": "text/html" });
    res.end(
    req.renderDocument({
        renderedContent,
        title,
        description,
    })
    );
};
"""|}

[<ReactComponent>]
let serverResponse = 
    CodeBlock {| lang = "html"; code =
"""HTTP/1.1 200 OK
Date: Fri, 29 Feb 2024 12:34:56 GMT
Content-Type: text/html; charset=UTF-8
Content-Length: 123
Connection: close

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width,minimum-scale=1,initial-scale=1"/>
    <title>Fex Template</title>
    <link rel="stylesheet" href="/app.css"/>
    <meta name="description" content="Fex Template">
</head>
<body>
    <div id="app">
        <div class="sitewrapper">
            <header>
                <h1>
                    <a href="/">fex-template</a>
                </h1>
                <nav>
                    <ul>
                        <li>
                            <a href="/about">About</a>
                        </li>
                        <li>
                            <a href="/github_status">GitHub Status</a>
                        </li>
                    </ul>
                </nav>
            </header>
            <div class="content">
                <h2>Welcome to Fex</h2>
                <p>Fex is an architectural pattern for building web applications with Express in JavaScript, utilizing Fable, Fable.React, and Feliz. It&#x27;s not a framework but an approach that emphasizes simplicity and flexibility. By leveraging both the `express` npm module for server-side and the `browser-express` module for client-side operations, it allows developers to craft route handlers, middleware, and React components that seamlessly work across both environments. The goal is to simplify web development while giving developers the freedom to adapt their architecture as their application evolves to meet user needs.</p>
                <a href="/form-elements">Next: Form Elements</a>
            </div>
            <footer>
                <p>Fex Template</p>
            </footer>
        </div>
    </div>
    <script type="text/javascript" charset="utf-8">
    window.expressLink = {
        "queryCache": {
            "query { greeting { heading content } }-({})": {
                "data": {
                    "greeting": {
                        "heading": "Welcome to Fex",
                        "content": "Fex is an architectural pattern for building web applications with Express in JavaScript, utilizing Fable, Fable.React, and Feliz. It's not a framework but an approach that emphasizes simplicity and flexibility. By leveraging both the `express` npm module for server-side and the `browser-express` module for client-side operations, it allows developers to craft route handlers, middleware, and React components that seamlessly work across both environments. The goal is to simplify web development while giving developers the freedom to adapt their architecture as their application evolves to meet user needs."
                    }
                }
            }
        },
        "csrf": "DeEDQsfr--Prn3HcTvqWfYeJT0ykENtXuKMc",
        "defaultTitle": "Fex Template"
    };
    </script>
    <script src="/app.js" type="text/javascript" charset="utf-8"></script>
</body>
</html>
"""|}

[<ReactComponent>]
let clientClickEvent = 
    CodeBlock {| lang = "javascript"; code =
"""const anchor = document.createElement('a');
anchor.href = '/';
const event = new MouseEvent('click', {
    'view': window,
    'bubbles': true,
    'cancelable': true
});
anchor.dispatchEvent(event);
"""|}

[<ReactComponent>]
let clientEventListener = 
    CodeBlock {| lang = "javascript"; code =
"""
window.addEventListener('click', function (e) {
    if (e.target.tagName === 'A') {
        router.navigate(e.target.href);
    }
});
"""|}

[<ReactComponent>]
let clientBrowserExpressApp = 
    CodeBlock {| lang = "fsharp"; code =
"""// from browser/Browser.fs

let app = express()
useMiddleware(expressLinkMiddleware())
useMiddleware(reactRendererMiddleware({| app = app; appLayout = AppLayout |}))
useMiddleware(graphqlClientMiddleware({| route = "/graphql" |}))
useMiddleware(fetchClientMiddleware())
useMiddleware(analyticstMiddleware({| analyticsRouter = analyticsRouter; fetch = fetch |}))

universalApp app

app.listen(3000, fun _ ->
    printfn "Listening on port 3000"
)
"""                 |}

[<ReactComponent>]
let clientMiddleware = 
    CodeBlock {| lang = "javascript"; code =
""" // from browser/middleware/express-link.js

export default () => (req, res, next) => {
  Object.keys(expressLink).forEach((key) => (req[key] = expressLink[key])); // eslint-disable-line no-return-assign

  req.renderDocument = ({ title }) => {
    document.querySelector("title").innerText = title || defaultTitle; // eslint-disable-line no-param-reassign
    return { appContainer: document.querySelector("#app") };
  };

  res.navigate = (path, query) => {
    const pathname = query ? `${path}?${qs.stringify(query)}` : path;
    res.redirect(pathname);
  };

  res.redirect = res.redirect.bind(res);

  next();
};

// from browser/middleware/react-renderer.js

export default ({ app, appLayout }) =>
  (req, res, next) => {
    const onClick = (e) => {
      e.preventDefault();
      function hrefOrParent(target) {
        if (target.href) {
          return target.href;
        }
        if (target.parentElement) {
          return hrefOrParent(target.parentElement);
        }
        return false;
      }
      const href = hrefOrParent(e.currentTarget);
      app.navigate(href);
    };

    const Link = (props) => {
      const mergedProps = { onClick, ...props };
      return React.createElement("a", mergedProps);
    };

    req.Link = Link;

    // ...

    next();
  };
  """|}

[<ReactComponent>]
let universalRouteHandler = 
    CodeBlock {| lang = "fsharp"; code =
""" // from App.fs

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
)"""|}

[<ReactComponent>]
let universalReactComponent = 
    CodeBlock {| lang = "fsharp"; code =
"""// from components/pages/FrontPage.fs

[<ReactComponent>]
let FrontPage(props: {| greeting : Greeting |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 props.greeting.heading
        Html.p props.greeting.content
    ]
    req.Link {| href = "/form-elements"; children = "Next: Form Elements" |}

"""|}

[<ReactComponent>]
let clientRenderDom = 
    CodeBlock {| lang = "javascript"; code =
"""// from browser/middleware/react-renderer.js

res.renderComponent = (content, options = {}) => {
    const { title, description } = options;
    const statusCode = options.statusCode || 200;
    const layout = options.layout || appLayout;
    const { appContainer } = req.renderDocument({ title, description });
    const root = app.root || createRoot(appContainer);
    app.root = root;
    root.render(React.createElement(layout, { content, req }));
    res.status(statusCode);
    res.send();
};
"""|}

let createParallelStepWithRequestStep requestStep (title: string, stepIndex: int, offsetX: int, offsetY: int, backgroundColor: string) =
    React.fragment [
        Svg.rect [
            svg.fill (match stepIndex with 
                            | step when step = requestStep -> color.green
                            | _ -> backgroundColor)
            svg.x(offsetX)
            svg.y(offsetY)
            svg.width(160)
            svg.height(28)
        ]
        Svg.text [
            svg.x(offsetX + 5)
            svg.y(offsetY + 20)
            svg.fontSize 18
            svg.fill color.white
            svg.text title
        ]
    ]

let createCombinedStepWithRequestStep requestStep ( title: string, stepIndex: int, offsetX: int, offsetY: int, backgroundColor: string) =
    React.fragment [
        Svg.rect [
            svg.fill (match stepIndex with
                            | step when step = requestStep -> color.green
                            | _ -> backgroundColor)
            svg.x(offsetX)
            svg.y(offsetY)
            svg.width(494)
            svg.height(28)
        ]
        Svg.text [
            svg.x(offsetX + 130)
            svg.y(offsetY + 20)
            svg.fontSize 18
            svg.fill color.white
            svg.text title
        ]
    ]

[<ReactComponent>]
let requestResponseCycleSvg requestStep =
    Svg.svg [
        svg.width (694) // Width to accommodate the elements
        svg.height (300) // Adjusted height to fit the additional rows
        svg.children [ 
            let createParallelStep = createParallelStepWithRequestStep requestStep
            let createCombinedStep = createCombinedStepWithRequestStep requestStep

            // Dynamically create SVG blocks for each step with specific background colors
            createParallelStep("HTTP Request", 0, 0, 0, "darkred")
            createParallelStep ("Click", 0, 534, 0, "darkblue")

            createParallelStep ("Event Listener", 1, 534, 30, "darkblue")
            createParallelStep( "Socket Listener", 1, 0, 30, "darkred")

            createParallelStep("Express", 2, 0, 60, "darkred")
            createParallelStep ( "Browser-Express", 2, 534, 60, "darkblue")

            createParallelStep("Server Middleware", 3, 0, 90, "darkred")
            createParallelStep("Client Middleware", 3, 534, 90, "darkblue")

            createCombinedStep("Universal Request Handlers", 4, 100, 120, "purple")
            createCombinedStep("Universal React Components", 5, 100, 150, "purple")

            createParallelStep("Rendered HTML", 6, 0, 180, "darkred")
            createParallelStep("Diffed DOM", 6, 534, 180, "darkblue")    

            createParallelStep("HTTP Response", 7, 0, 210, "darkred")
            createParallelStep("DOM Update", 7, 534, 210, "darkblue")
        ]
    ]

[<ReactComponent>]
let RequestResponseCyclePage() =
    let req = React.useContext requestContext
    React.fragment [
        let (requestStep, setRequestStep) = React.useState(0)
        let steps = 8 // Steps including server and client middleware

        let cycleRequestStep = fun _ -> setRequestStep((requestStep + 1) % steps)
        let cycleBackRequestStep = fun _ -> setRequestStep(if requestStep = 0 then steps - 1 else requestStep - 1)

        Html.h2 "Request-Response Cycle"

        Html.p [
            Html.button [
                prop.onClick (fun _ -> cycleRequestStep())
                prop.text "Cycle Forward"
            ]
            Html.button [
                prop.onClick (fun _ -> cycleBackRequestStep())
                prop.text "Cycle Backward"
            ]
        ]

        requestResponseCycleSvg requestStep

        Html.p [
            prop.className "explanation"
            prop.children [
                Html.p [
                    prop.children [
                        match requestStep with
                        | 0 -> Html.p "An HTTP request by the client initiates the communication cycle, requesting server resources. Similarly, user interactions like clicking a link are captured on the client side, indicating an intent to navigate or request new content."
                        | 1 -> Html.p "On receiving an HTTP request, the server's socket listener springs into action, waiting to process incoming connections. Meanwhile, the client-side event listener monitors user actions, such as link clicks, to handle navigation programmatically, preventing default browser behavior."
                        | 2 -> Html.p "Both server and client set up their Express applications to manage the request lifecycle. The server uses Express to define routes, middleware, and response handling, while the client uses Browser Express to mimic this structure for client-side navigation and state management."
                        | 3 -> Html.p "Middleware on both ends plays a crucial role in processing requests and responses. Server middleware may involve authentication, data parsing, and session management, while client middleware adapts these concepts for the client, handling state updates and UI changes and creating req and res objects with the same interface for both environments."
                        | 4 -> Html.p "Universal request handlers are designed to run both on the server for initial page rendering and on the client for handling subsequent navigations or updates. This approach ensures a consistent rendering process across environments."
                        | 5 -> Html.p "Universal React components enable writing UI code that runs both server-side and client-side, facilitating seamless rendering and rehydration of the app's UI. This strategy enhances performance and user experience by leveraging React's efficient DOM updates."
                        | 6 -> Html.p "The server's response includes rendered HTML content for the requested page. The client-side flow involves diffing the existing DOM with the new content to apply only the necessary updates, minimizing browser reflow and repaint costs."
                        | 7 -> Html.p "Completing the cycle, the server sends an HTTP response back to the client, which includes the requested content or data. On the client side, this response triggers updates to the DOM, reflecting changes in the UI based on the received data or user navigation actions."
                        | _ -> null
                    ]
                ]
            ]
        ]

        Html.p [
            prop.className "server"
            prop.children [
                Html.p [
                    match requestStep with
                    | 0 -> (serverRawRequest)
                    | 1 -> (serverSocketListener)
                    | 2 -> (serverExpressApp)
                    | 3 -> (serverMiddelware)
                    | 6 -> (serverRenderHtml)
                    | 7 -> (serverResponse)
                    | _ -> null
                ]
            ]
        ]

        Html.p [
            prop.className "universal"
            prop.children [
                Html.p [
                    match requestStep with
                    | 4 -> (universalRouteHandler)
                    | 5 -> (universalReactComponent)
                    | _ -> null
                ]
            ]
        ]

        Html.p [
            prop.className "client"
            prop.children [
                Html.p [
                    match requestStep with
                    | 0 -> (clientClickEvent)
                    | 1 -> (clientEventListener)
                    | 2 -> (clientBrowserExpressApp)
                    | 3 -> (clientMiddleware)
                    | 6 | 7 -> (clientRenderDom)
                    | _ -> null
                ]
            ]
        ]
        
        req.Link {| href = "/caveats"; children = "Next: Caveats" |}

    ]