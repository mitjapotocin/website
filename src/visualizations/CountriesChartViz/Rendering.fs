﻿module CountriesChartViz.Rendering

open CountriesChartViz.Synthesis
open System
open Browser
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

open Analysis
open Highcharts
open Types

type CountriesDisplaySet = {
    Label: string
    CountriesCodes: string[]
}

// source: https://unstats.un.org/unsd/tradekb/knowledgebase/country-code
let countriesDisplaySets = [|
    { Label = "Nordijske države"
      CountriesCodes = [| "DNK"; "FIN"; "ISL"; "NOR"; "SWE" |]
    }
    { Label = "Ex-Yugoslavia"
      CountriesCodes = [| "BIH"; "HRV"; "MKD"; "MNE"; "RKS"; "SRB" |]
    }
    { Label = "Sosedje"
      CountriesCodes = [| "AUT"; "HRV"; "HUN"; "ITA" |]
    }
|]

type Msg =
    | DataRequested
    | DataLoaded of Data.OurWorldInData.OurWorldInDataRemoteData
    | ChangeCountriesSelection of int

[<Literal>]
let DaysOfMovingAverage = 5

let init: ChartState * Cmd<Msg> =
    let state = {
        Data = NotAsked
        DisplayedCountriesSet = 0
    }
    state, Cmd.ofMsg DataRequested

let update (msg: Msg) (state: ChartState) : ChartState * Cmd<Msg> =
    match msg with
    | ChangeCountriesSelection setIndex ->
        { state with DisplayedCountriesSet = setIndex }, Cmd.none
    | DataRequested ->
        let countriesCodes =
            let displaySetIndex = state.DisplayedCountriesSet

            "SVN" ::
            (countriesDisplaySets.[displaySetIndex].CountriesCodes
                |> Array.toList)
        { state with Data = Loading },
        Cmd.OfAsync.result (Data.OurWorldInData.load countriesCodes DataLoaded)
    | DataLoaded remoteData ->

        match remoteData with
        | NotAsked ->
            printfn "Not asked"
        | Loading ->
            printfn "Loading"
        | Failure error ->
            printfn "Error: %s" error
        | Success data ->
            printfn "Success %A" data

        { state with Data = remoteData }, Cmd.none

let renderChartCode (state: ChartState) (chartData: ChartData) =
    let myLoadEvent _ =
        let ret _ =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let allSeries =
        chartData.Series
        |> Array.map (fun countrySeries ->
             pojo
                {|
                visible = true
                color = countrySeries.Color
                name = countrySeries.CountryAbbr
                data =
                    countrySeries.Data
                    |> Array.map (fun ((dayIndex, _), value) ->
                        (dayIndex, value))
                marker = pojo {| enabled = false |}
                |}
            )
        // we need to reverse the array, for some reason
        |> Array.rev

    let legend =
        {|
            enabled = true
            title = ""
            align = "left"
            verticalAlign = "top"
            borderColor = "#ddd"
            borderWidth = 1
            //labelFormatter = string //fun series -> series.name
            layout = "vertical"
            floating = true
            x = 20
            y = 30
            backgroundColor = "rgba(255,255,255,0.5)"
            reversed = true
        |}

    let baseOptions = basicChartOptions Linear "covid19-metrics-comparison"
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = "spline"
                zoomType = "x"
                events = {| load = myLoadEvent("countries") |}
            |}
        title = pojo {| text = None |}
        series = allSeries
        xAxis =
            pojo {|
                   ``type`` = "int"
                   allowDecimals = false
                   title = pojo {| text = chartData.XAxisTitle |}
            |}
        yAxis =
            pojo {|
                   opposite = true
                   title =
                       pojo {|
                            align = "middle"
                            text = chartData.YAxisTitle
                        |}
            |}
        plotOptions = pojo
            {|
                series = pojo {| stacking = ""; |}
            |}
        legend = pojo {| legend with enabled = true |}
        tooltip = pojo {| formatter = fun () -> legendFormatter jsThis |}
    |}


let renderChartContainer state chartData =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartCode state chartData
            |> chart
        ]
    ]

let render state _ =
    let chartData =
        state
        |> prepareChartData FirstDeath DaysOfMovingAverage

    match chartData with
    | Some chartData ->
        Html.div [
            renderChartContainer state chartData
    //        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)

            Html.div [
                prop.className "disclaimer"
            ]
        ]
    | None -> Html.div []

let renderChart() =
    React.elmishComponent
        ("CountriesChartViz/CountriesChart", init, update, render)
