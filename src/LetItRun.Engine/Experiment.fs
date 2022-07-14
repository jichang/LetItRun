namespace LetItRun.Engine

open System.IO
open Thoth.Json.Net
open Manifest
open System.Security.Cryptography

module Experiment =
    let load (manifest: Manifest) =
        manifest
