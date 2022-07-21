module ManifestTests

open System
open LetItRun.Core
open LetItRun.Core.Manifest
open Xunit
open Thoth.Json.Net
open System.Security.Cryptography

[<Fact>]
let ``manifest should support JSON encoding and decoding`` () =
    let metadata =
        { id = "000000"
          version = "1.0.0"
          name = "Test"
          description = ""
          author = "" }

    let manifest =
        { version = V1
          metadata = metadata
          sources = [| { name = "file"; hash = "hash" } |]
          signer =
            { algorithm = "sha256"
              publicKey = "key" }
          signature = "value" }

    let json = Encode.toString 0 (Manifest.encoder manifest)
    let result = Decode.fromString Manifest.decoder json

    Assert.Equal(result, (Ok manifest))

[<Fact>]
let ``verifySignature should validate signature of manifest`` () =
    let metadata =
        { id = "0"
          version = "1.0.0"
          name = "Demo"
          description = "demo experiment"
          author = "LetItRun" }

    let rsa = RSA.Create()
    let privateKey = rsa.ExportRSAPrivateKey()
    let publicKey = rsa.ExportRSAPublicKey()

    let signer =
        { algorithm = "sha256"
          publicKey = publicKey |> Convert.ToBase64String }

    let manifest =
        { version = V1
          metadata = metadata
          sources = [| { name = "file"; hash = "hash" } |]
          signer = signer
          signature = "" }

    let signature = Manifest.sign manifest privateKey

    let signedManifest =
        { manifest with signature = signature |> Convert.ToBase64String }

    let isMatch = Manifest.verify signedManifest
    Assert.Equal(true, isMatch)
