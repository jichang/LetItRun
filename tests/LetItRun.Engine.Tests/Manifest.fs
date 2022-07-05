module ManifestTests
    open LetItRun.Engine.Manifest
    open System
    open Xunit
    open Thoth.Json.Net

    let metadata = {
        id = "000000"
        version = "1.0.0"
        name = "Test"
        description = ""
        author = ""
    }

    let manifest = {
        version = V1
        metadata = metadata
        files = [| { name = "file"; hash = "hash"} |]
        signature = ""
    }

    let json = Encode.toString 0 (Manifest.encoder manifest)
    let result = Decode.fromString Manifest.decoder json

    Assert.Same (result, (Ok manifest))
