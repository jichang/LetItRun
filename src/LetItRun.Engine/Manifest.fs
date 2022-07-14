namespace LetItRun.Engine

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
    /// <summary>signature of experiment, all field should be in UTF-8 encoding</summary>
    /// <param name="algorithm">Algorithm used by the signature</param>
    /// <param name="publicKey">Public key used by the signature, should be encoded as base64</param>
    /// <param name="value">Signature value, should be encoded as base64</param>
    /// 
    type Signature = {
        algorithm: string
        publicKey: string
        value: string
    }

    module Signature =
        let decoder: Decoder<Signature> =
            Decode.object (fun get ->
                { algorithm = get.Required.Field "algorithm" Decode.string
                  publicKey = get.Required.Field "publicKey" Decode.string
                  value = get.Required.Field "value" Decode.string })

        let encoder (signature: Signature) : JsonValue =
            Encode.object [ "algorithm", Encode.string signature.algorithm
                            "publicKey", Encode.string signature.publicKey
                            "value", Encode.string signature.value ]

    ///
    /// <summary>WASM File, all field should be in UTF-8 encoding</summary>
    /// <param name="name">File name</param>
    /// <param name="hash">Hash of file content</param>
    /// 
    type File = { name: string; hash: string }

    module File =
        let decoder: Decoder<File> =
            Decode.object (fun get ->
                { name = get.Required.Field "name" Decode.string
                  hash = get.Required.Field "hash" Decode.string })

        let encoder (file: File) : JsonValue =
            Encode.object [ "name", Encode.string file.name
                            "hash", Encode.string file.hash ]

    ///
    /// <summary>Experiment manifest, all field should be in UTF-8 encoding</summary>
    /// <param name="version">Manifest version, different version will have different structure</param>
    /// <param name="metadata">Experiment metadata</param>
    /// <param name="files">Wasm code files</param>
    /// <param name="signature">Signature of manifest</param>
    /// 
    type Manifest =
        { version: Version
          metadata: Metadata
          files: File array
          signature: Signature }

    module Manifest =
        let extract (manifest: Manifest) =
            let version = Encode.toString 0 (Version.encoder manifest.version)
            let metadata = [|
                version
                manifest.metadata.version
                manifest.metadata.id
                manifest.metadata.name
                manifest.metadata.author
            |]
            let files =
                Array.map (fun (file: File) -> file.name + file.hash) manifest.files
            let plaintext =
                Array.append metadata files
                |> Array.reduce (fun a b -> a + b)
                |> Converter.strToBytes
            plaintext

        let sign (manifest: Manifest) (privateKey: byte[]) =
            let plaintext = extract manifest
            let rsa = new RSACryptoServiceProvider()
            let bytesRead = ref 0
            do rsa.ImportRSAPrivateKey (privateKey, bytesRead)
            rsa.SignData(plaintext, manifest.signature.algorithm)

        let verify (manifest: Manifest) =
            let publicKey = Convert.FromBase64String manifest.signature.publicKey
            let plaintext = extract manifest
            let rsa = new RSACryptoServiceProvider()
            let bytesRead = ref publicKey.Length
            do rsa.ImportRSAPublicKey (publicKey, bytesRead)
            rsa.VerifyData (plaintext, manifest.signature.algorithm, manifest.signature.value |> Convert.FromBase64String)

        let decoder: Decoder<Manifest> =
            Decode.object (fun get ->
                { version = get.Required.Field "version" Version.decoder
                  metadata = get.Required.Field "metadata" Metadata.decoder 
                  files = get.Required.Field "files" (Decode.array File.decoder)
                  signature = get.Required.Field "signature" Signature.decoder })

        let encoder (manifest: Manifest) : JsonValue =
            Encode.object [ "version", Version.encoder manifest.version
                            "metadata", Metadata.encoder manifest.metadata 
                            "files", Encode.array (Array.map File.encoder manifest.files)
                            "signature", Signature.encoder manifest.signature ]
