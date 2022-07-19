module ExperimentTests

open Xunit
open System.IO
open LetItRun.Engine
open LetItRun.Engine.Manifest
open LetItRun.Engine.Experiment

[<Fact>]
let ``Experiment.load should support loading manifest from specified directory`` () =
    let path = Path.Join(Directory.GetCurrentDirectory(), "demo")
    let result = Experiment.load path

    let sources =
        [| { name = "sources/main.wasm"
             hash =
               "ywf0q3dl83mesBcV9jVZB5/b0V8cetjbBQaPvAvNmsJVxu7SQAZfc99abjP040shCrv7sdd93+xjv5M+dDeH7Dm7yu2QBtgNnATpw8GXjHSTAQsiR/0qHstn3xqT71XhOJDMPWu4wCK+uZqqbp4g2ukWQHqE8buTSrOrMFpuXnyXqnDgXg23gBGJK5CIF7E+Z0/s2vH3Q5/EkoX5Su/CfdL2nHlgGSaewxIqbu1PzCai5FDFlF8CZHJnKCV+jmBJZvhWDFGHVvy9U75IWlTfM7S5BjDdJfLO0LNf4sPxjxTrqfMnqRVhH/l8iP7/M29Xh1+Vt1Xgu8YxAz6usJ7LyQ==" } |]

    let signer =
        { algorithm = "sha256"
          publicKey =
            "MIIBCgKCAQEA5spe6mpdtGlqlpgeLCVFOyDMEuy36K6nNeEfUa37fT3ltYJgNIqYoC91N/nEMahk5bsZ4w8TcYPEfCOjjoC3TrpNRs41oJc6Ol+FXIagZui2YNCEdSYmrYNDELQmyT791vC7U4lLMMUvV4XqV8gauz4/ywZ010f6KbQUIHxwfTsLf3lwtZuxP5YQQ6RKjeoLCrdJMnBs9plZs70fHK++14nX2RzOnqPwbMzW0L2vgX6vvAqZiaATpfS3Vanz8Ob30bHWV3SdwToWUvkXC3HFWbLjSTLqPuuaEtD4Rynh3FvajBVXTH5m2v21VUPxfbfmlq+2U7FG3rv9NXTGvudhcQIDAQAB" }

    let manifest =
        { version = V1
          metadata =
            { version = "1.0.0"
              id = "0"
              name = "Demo"
              description = "demo experiment"
              author = "LetItRun" }
          sources = sources
          signer = signer
          signature =
            "WjoSGXpj1CMU2CO5IxNAJtGc6/1Kv+j1E6TwsVR4/SMRlyXILzgS/1PRwSxYEhEX3RDqWKUCTVDXYF+xDy+Djfv0C2ysdNqDJ3Cb6icCemQ1lgYm78IYozEYb/oU1bc1dE3fzV7B726WchBS0eACb9PpggTn+rA2qA8OII4iMPgDhge/GbWYF+0ZvHmKz47yNDOuWICj8qFLJ25sMvapa1vTExNfX9wL2Wu1Ow67rdGwQgaP7YsVW7P5qcp0GZod14o88NasBAAHa+sgjodwvrNQTSQ9DNX3IJ8/0/pgz7sVH+oXJhOziGGExy/mr0N9PNostR1verhpn6NB4S/S7A==" }

    Assert.Equal(Ok manifest, result)