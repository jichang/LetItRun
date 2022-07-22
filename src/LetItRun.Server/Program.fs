// For more information see https://aka.ms/fsharp-console-apps
open LetItRun.Core.Manifest
open LetItRun.Grains.Core
open System
open System.IO
open System.Security.Cryptography
open System.Threading.Tasks
open Thoth.Json.Net
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Hosting
open Orleans.Configuration

let createManifest () =
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
    let content = System.IO.File.ReadAllBytes(Path.Join("./main.wasm"))
    let rsaProvider = new RSACryptoServiceProvider()
    let bytesRead = ref 0
    do rsaProvider.ImportRSAPrivateKey(privateKey, bytesRead)
    let hash = rsaProvider.SignData(content, signer.algorithm)

    let source =
        { name = sourceName
          hash = hash |> Convert.ToBase64String }

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

let configureOrleansCluster (options: ClusterOptions) =
    options.ClusterId = "dev" |> ignore
    options.ServiceId = "LetItRun" |> ignore

let startSilo () =
    task {
        let builder = HostBuilder()
        builder.UseOrleans(fun cfg ->
            cfg.UseLocalhostClustering()
                .Configure(configureOrleansCluster)
                .ConfigureApplicationParts(fun parts ->
                    let t = typeof<WorldGrain>
                    parts.AddApplicationPart(t.Assembly).WithReferences().WithCodeGeneration(null)
                    |> ignore
                )
                .ConfigureLogging(fun logging -> 
                    logging.AddConsole()
                    |> ignore
                )
            |> ignore
        ) |> ignore

        let host = builder.Build()
        do! host.StartAsync()
    }

let task = startSilo ()
task.Wait()
