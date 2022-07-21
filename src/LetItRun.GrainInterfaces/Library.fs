namespace LetItRun.GrainInterfaces

open System.Threading.Tasks
open Orleans

module Core =
    type IWorldGrain =
        inherit IGrainWithStringKey

        abstract member start: unit -> Task<bool>
        abstract member pause: unit -> Task<bool>
        abstract member resume: unit -> Task<bool>
        abstract member stop: unit -> Task<bool>

    type IExperimentGrain =
        inherit IGrainWithStringKey

        abstract member load: string -> Task<bool>
        abstract member start: unit -> Task<bool>
        abstract member pause: unit -> Task<bool>
        abstract member resume: unit -> Task<bool>
        abstract member stop: unit -> Task<bool>
