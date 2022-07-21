// For more information see https://aka.ms/fsharp-console-apps
open LetItRun.Core.Manifest
open System
open System.IO
open System.Security.Cryptography
open Thoth.Json.Net

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

let sourceName = "sources/main.wasm"
let content = System.IO.File.ReadAllBytes (Path.Join("./main.wasm"))
let rsaProvider = new RSACryptoServiceProvider()
let bytesRead = ref 0
do rsaProvider.ImportRSAPrivateKey(privateKey, bytesRead)
let hash = rsaProvider.SignData(content, signer.algorithm)

let source = { name = sourceName; hash = hash |> Convert.ToBase64String }

let manifest =
    { version = V1
      metadata = metadata
      sources = [| source |]
      signer = signer
      signature = "" }

let signature = Manifest.sign manifest privateKey

let signedManifest =
    { manifest with signature = signature |> Convert.ToBase64String }

let str = Encode.toString 2 (Manifest.encoder signedManifest)
let path = System.IO.Path.Join("./manifest.json")
System.IO.File.WriteAllText(path, str)
