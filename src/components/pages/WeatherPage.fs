module WeatherPage

open Feliz
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock


[<ReactComponent>]
let WeatherPage(props: {| forecast : obj array|}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Weather"
        props.forecast
        |> Array.map(fun (period: obj) ->
            let detailedForecast : string = period?detailedForecast
            Html.p detailedForecast
        )
        |> React.fragment
    ]