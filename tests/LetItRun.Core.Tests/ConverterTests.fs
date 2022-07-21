module ConverterTests

open LetItRun.Core.Converter
open Xunit

[<Fact>]
let ``strToBytes and bytesToStr should match`` =
    let source = "你好，世界"
    Assert.Equal(source, source |> strToBytes |> bytesToStr)