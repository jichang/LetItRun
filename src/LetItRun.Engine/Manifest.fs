namespace LetItRun.Engine

open Thoth.Json.Net

module Manifest =
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

    type File = { name: string; hash: string }

    module File =
        let decoder: Decoder<File> =
            Decode.object (fun get ->
                { name = get.Required.Field "name" Decode.string
                  hash = get.Required.Field "hash" Decode.string })

        let encoder (file: File) : JsonValue =
            Encode.object [ "name", Encode.string file.name
                            "hash", Encode.string file.hash ]

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

    type Signature = string

    module Signature =
        let decoder: Decoder<Signature> = Decode.string
        let encoder = Encode.string

    type Manifest =
        { version: Version
          metadata: Metadata
          files: File array
          signature: Signature }

    module Manifest =
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