namespace LetItRun.Engine

open System.IO
open Thoth.Json.Net
open Manifest

module Experiment =
    type ExperimentLoadError =
        | ManifestNotFound of string
        | ManifestInvalidFormat of string
        | ManifestInvalidSignature of string
        | SourceNotFound of string
        | SourceInvalidSignature of string

    let loadSource (manifest: Manifest) (path: string) (source: Source) =
        let sourceFilePath = Path.Join(path, source.name)
        if File.Exists sourceFilePath then
            let content = File.ReadAllBytes (sourceFilePath)
            if Source.verify manifest.signer source content then
                Ok content
            else
                Error (SourceInvalidSignature sourceFilePath)
        else
            Error (SourceNotFound sourceFilePath)

    let load (path: string) =
        let manifestPath = Path.Join(path, "manifest.json")
        if File.Exists manifestPath then
            let content = File.ReadAllText(manifestPath)
            match Decode.fromString Manifest.decoder content with
            | Ok manifest ->
                if Manifest.verify manifest then
                    Ok manifest
                else
                    Error (ManifestInvalidSignature manifestPath)
            | Error e ->
                Error (ManifestInvalidFormat e)
        else
            Error (ManifestNotFound manifestPath)
