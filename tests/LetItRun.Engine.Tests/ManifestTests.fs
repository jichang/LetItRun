module ManifestTests

open System
open LetItRun.Engine
open LetItRun.Engine.Manifest
open Xunit
open Thoth.Json.Net
open System.Security.Cryptography;

[<Fact>]
let ``manifest should support JSON encoding and decoding`` () =
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
        signature = { algorithm = "sha256"; publicKey = "key"; value = "value"}
    }

    let json = Encode.toString 0 (Manifest.encoder manifest)
    let result = Decode.fromString Manifest.decoder json

    Assert.Equal (result, (Ok manifest))

[<Fact>]
let ``verify should validate signature of manifest`` () =
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
        signature = { algorithm = "sha256"; publicKey = "key"; value = "value"}
    }

    let rsa = RSA.Create()
    let privateKey = rsa.ExportRSAPrivateKey()
    let publicKey = rsa.ExportRSAPublicKey()

    let value = Manifest.sign manifest privateKey
    let newSignature = { manifest.signature with value = value |> Convert.ToBase64String; publicKey = publicKey |> Convert.ToBase64String }
    let signedManifest = { manifest with signature = newSignature }
    let isMatch = Manifest.verify signedManifest
    Assert.Equal (true, isMatch)