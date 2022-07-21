namespace LetItRun.Core

open System
open Thoth.Json.Net
open System.Security.Cryptography

module Manifest =
    ///
    /// <summary>Manifest version</summary>
    ///
    type Version = V1

    module Version =
        let decoder: Decoder<Version> =
            Decode.string
            |> Decode.andThen (function
                | "V1" -> Decode.succeed V1
                | invalid -> Decode.fail $"{invalid} is not a valid version, expecting V1")

        let encoder (version: Version) : JsonValue =
            match version with
            | V1 -> Encode.string "V1"


    ///
    /// <summary>metadata of experiment, all field should be in UTF-8 encoding</summary>
    /// <param name="version">version of this experiment</param>
    /// <param name="id">id of this experiment</param>
    /// <param name="name">name of this experiment</param>
    /// <param name="description">description of this experiment</param>
    /// <param name="author">author of this experiment</param>
    ///
    type Metadata =
        { version: string
          id: string
          name: string
          description: string
          author: string }

    module Metadata =
        let decoder: Decoder<Metadata> =
            Decode.object (fun get ->
                { version = get.Required.Field "version" Decode.string
                  id = get.Required.Field "id" Decode.string
                  name = get.Required.Field "name" Decode.string
                  description = get.Required.Field "description" Decode.string
                  author = get.Required.Field "author" Decode.string })

        let encoder (metadata: Metadata) : JsonValue =
            Encode.object [ "version", Encode.string metadata.version
                            "id", Encode.string metadata.id
                            "name", Encode.string metadata.name
                            "description", Encode.string metadata.description
                            "author", Encode.string metadata.author ]

    ///
    /// <summary>signer of experiment, all field should be in UTF-8 encoding</summary>
    /// <param name="algorithm">Algorithm used by the signer</param>
    /// <param name="publicKey">Public key used by the signer, should be encoded as base64</param>
    /// <param name="value">Signer value, should be encoded as base64</param>
    ///
    type Signer =
        { algorithm: string
          publicKey: string }

    module Signer =
        let decoder: Decoder<Signer> =
            Decode.object (fun get ->
                { algorithm = get.Required.Field "algorithm" Decode.string
                  publicKey = get.Required.Field "publicKey" Decode.string })

        let encoder (signer: Signer) : JsonValue =
            Encode.object [ "algorithm", Encode.string signer.algorithm
                            "publicKey", Encode.string signer.publicKey ]

    ///
    /// <summary>WASM Source, all field should be in UTF-8 encoding</summary>
    /// <param name="name">Source name</param>
    /// <param name="hash">Hash of Source content</param>
    ///
    type Source = { name: string; hash: string }

    module Source =
        let verify (signer: Signer) (source: Source) (content: byte array) =
            let rsa = new RSACryptoServiceProvider()
            let publicKey = Convert.FromBase64String signer.publicKey
            let bytesRead = ref signer.publicKey.Length
            do rsa.ImportRSAPublicKey(publicKey, bytesRead)
            rsa.VerifyData(content, signer.algorithm, source.hash |> Convert.FromBase64String)

        let decoder: Decoder<Source> =
            Decode.object (fun get ->
                { name = get.Required.Field "name" Decode.string
                  hash = get.Required.Field "hash" Decode.string })

        let encoder (source: Source) : JsonValue =
            Encode.object [ "name", Encode.string source.name
                            "hash", Encode.string source.hash ]

    ///
    /// <summary>Experiment manifest, all field should be in UTF-8 encoding</summary>
    /// <param name="version">Manifest version, different version will have different structure</param>
    /// <param name="metadata">Experiment metadata</param>
    /// <param name="Sources">Wasm code Sources</param>
    /// <param name="signer">Signer of manifest</param>
    ///
    type Manifest =
        { version: Version
          metadata: Metadata
          sources: Source array
          signer: Signer
          signature: string }

    module Manifest =
        let extract (manifest: Manifest) =
            let version = Encode.toString 0 (Version.encoder manifest.version)

            let metadata =
                [| version
                   manifest.metadata.version
                   manifest.metadata.id
                   manifest.metadata.name
                   manifest.metadata.author |]

            let Sources =
                Array.map (fun (source: Source) -> source.name + source.hash) manifest.sources

            let plaintext =
                Array.append metadata Sources
                |> Array.reduce (fun a b -> a + b)
                |> Converter.strToBytes

            plaintext

        let sign (manifest: Manifest) (privateKey: byte []) =
            let plaintext = extract manifest
            let rsa = new RSACryptoServiceProvider()
            let bytesRead = ref 0
            do rsa.ImportRSAPrivateKey(privateKey, bytesRead)
            rsa.SignData(plaintext, manifest.signer.algorithm)

        let verify (manifest: Manifest) =
            let publicKey = Convert.FromBase64String manifest.signer.publicKey
            let plaintext = extract manifest
            let rsa = new RSACryptoServiceProvider()
            let bytesRead = ref publicKey.Length
            do rsa.ImportRSAPublicKey(publicKey, bytesRead)
            rsa.VerifyData(plaintext, manifest.signer.algorithm, manifest.signature |> Convert.FromBase64String)

        let decoder: Decoder<Manifest> =
            Decode.object (fun get ->
                { version = get.Required.Field "version" Version.decoder
                  metadata = get.Required.Field "metadata" Metadata.decoder
                  sources = get.Required.Field "sources" (Decode.array Source.decoder)
                  signer = get.Required.Field "signer" Signer.decoder
                  signature = get.Required.Field "signature" Decode.string })

        let encoder (manifest: Manifest) : JsonValue =
            Encode.object [ "version", Version.encoder manifest.version
                            "metadata", Metadata.encoder manifest.metadata
                            "sources", Encode.array (Array.map Source.encoder manifest.sources)
                            "signer", Signer.encoder manifest.signer
                            "signature", Encode.string manifest.signature ]
