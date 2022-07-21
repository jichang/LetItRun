namespace LetItRun.Core

open System.IO
open Thoth.Json.Net
open Manifest

// Newer version FSharp.Core has add these methods, will remove it in future
module ResultExtension =
    let isOk result =
        match result with
        | Ok _ -> true
        | _ -> false

    let isError result =
        match result with
        | Ok _ -> false
        | _ -> true

    exception UnwrapError of string

    let getOkValue result =
        match result with
        | Ok value -> value
        | _ -> raise (UnwrapError("getOkValue can only be called with Ok variant"))

    let getErrorValue result =
        match result with
        | Ok _ -> raise (UnwrapError("getErrorValue can only be called with Error variant"))
        | Error error -> error

module Experiment =
    type SourceLoadError =
        | SourceNotFound of string
        | SourceInvalidSignature of string

    let verifySource (manifest: Manifest) (path: string) (source: Source) =
        let sourceFilePath = Path.Join(path, source.name)
        if File.Exists sourceFilePath then
            let content = File.ReadAllBytes (sourceFilePath)
            if Source.verify manifest.signer source content then
                Ok (source, content)
            else
                Error (SourceInvalidSignature sourceFilePath)
        else
            Error (SourceNotFound sourceFilePath)

    type ExperimentLoadError =
        | ManifestNotFound of string
        | ManifestInvalidFormat of string
        | ManifestInvalidSignature of string
        | SourcesInvalid of SourceLoadError array

    let verify (path: string) =
        let manifestPath = Path.Join(path, "manifest.json")
        if File.Exists manifestPath then
            let content = File.ReadAllText(manifestPath)
            match Decode.fromString Manifest.decoder content with
            | Ok manifest ->
                if Manifest.verify manifest then
                    let sources = Array.map (verifySource manifest path) manifest.sources
                    let invalidSources =
                        Array.filter ResultExtension.isError sources
                        |> Array.map ResultExtension.getErrorValue
                    if Array.isEmpty invalidSources then
                        Ok manifest
                    else
                        Error (SourcesInvalid invalidSources)
                else
                    Error (ManifestInvalidSignature manifestPath)
            | Error e ->
                Error (ManifestInvalidFormat e)
        else
            Error (ManifestNotFound manifestPath)
