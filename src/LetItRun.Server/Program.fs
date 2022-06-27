// For more information see https://aka.ms/fsharp-console-apps
open LetItRun.Engine.Manifest
open Thoth.Json.Net

let file: File = { name = "test"; hash = "hash" }

printf "%s\n" (Encode.toString 2 (File.encoder file))
