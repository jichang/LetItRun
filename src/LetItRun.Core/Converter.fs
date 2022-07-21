namespace LetItRun.Core

open System.Text

module Converter =
    let strToBytes (str: string) =
        Encoding.UTF8.GetBytes(str)

    let bytesToStr (bytes: byte array) =
        Encoding.UTF8.GetString(bytes)
