namespace LetItRun.Grains

open LetItRun.Core
open LetItRun.GrainInterfaces.Core
open Orleans
open Orleans.CodeGeneration
module Core =
    type WorldGrain() =
        inherit Grain()

        interface IWorldGrain with
            member __.start() = task { return true }

            member __.pause() = task { return true }

            member __.resume() = task { return true }

            member __.stop() = task { return true }

    type ExperimentGrain() =
        inherit Grain()

        interface IExperimentGrain with
            member __.load(path: string) = task {
                match Experiment.verify path with
                | Ok manifest ->
                    return true
                | Error _ ->
                    return false
            }

            member __.start() = task { return true }

            member __.pause() = task { return true }

            member __.resume() = task { return true }

            member __.stop() = task { return true }
