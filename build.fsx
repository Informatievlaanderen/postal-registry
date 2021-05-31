#r "paket:
version 6.0.0-rc001
framework: netstandard20
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 5.0.4 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open ``Build-generic``

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "postal-registry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let buildSource = build assemblyVersionNumber
let buildTest = buildTest assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publishSource = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "PostalRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  buildSource "PostalRegistry.Projector"
  buildSource "PostalRegistry.Api.Legacy"
  buildSource "PostalRegistry.Api.Extract"
  buildSource "PostalRegistry.Api.CrabImport"
  buildSource "PostalRegistry.Projections.Legacy"
  buildSource "PostalRegistry.Projections.Extract"
  buildSource "PostalRegistry.Projections.LastChangedList"
  buildSource "PostalRegistry.Projections.Syndication"
  buildTest "PostalRegistry.Tests"
)

Target.create "Test_Solution" (fun _ ->
    [
        "test" @@ "PostalRegistry.Tests"
    ] |> List.iter testWithDotNet
)

Target.create "Publish_Solution" (fun _ ->
  [
    "PostalRegistry.Projector"
    "PostalRegistry.Api.Legacy"
    "PostalRegistry.Api.Extract"
    "PostalRegistry.Api.CrabImport"
    "PostalRegistry.Projections.Legacy"
    "PostalRegistry.Projections.Extract"
    "PostalRegistry.Projections.LastChangedList"
    "PostalRegistry.Projections.Syndication"
  ] |> List.iter publishSource)

Target.create "Pack_Solution" (fun _ ->
  [
    "PostalRegistry.Projector"
    "PostalRegistry.Api.Legacy"
    "PostalRegistry.Api.Extract"
    "PostalRegistry.Api.CrabImport"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "PostalRegistry.Projector" "projector")
Target.create "PushContainer_Projector" (fun _ -> push "projector")

Target.create "Containerize_ApiLegacy" (fun _ -> containerize "PostalRegistry.Api.Legacy" "api-legacy")
Target.create "PushContainer_ApiLegacy" (fun _ -> push "api-legacy")

Target.create "Containerize_ApiExtract" (fun _ -> containerize "PostalRegistry.Api.Extract" "api-extract")
Target.create "PushContainer_ApiExtract" (fun _ -> push "api-extract")

Target.create "Containerize_ApiCrabImport" (fun _ -> containerize "PostalRegistry.Api.CrabImport" "api-crab-import")
Target.create "PushContainer_ApiCrabImport" (fun _ -> push "api-crab-import")

Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "PostalRegistry.Projections.Syndication" "projections-syndication")
Target.create "PushContainer_ProjectionsSyndication" (fun _ -> push "projections-syndication")

// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore
Target.create "Push" ignore

"NpmInstall"
  ==> "DotNetCli"
  ==> "Clean"
  ==> "Restore_Solution"
  ==> "Build_Solution"
  ==> "Build"

"Build"
  ==> "Test_Solution"
  ==> "Test"

"Test"
  ==> "Publish_Solution"
  ==> "Publish"

"Publish"
  ==> "Pack_Solution"
  ==> "Pack"

"Pack"
  ==> "Containerize_Projector"
  ==> "Containerize_ApiLegacy"
  ==> "Containerize_ApiExtract"
  ==> "Containerize_ApiCrabImport"
  ==> "Containerize_ProjectionsSyndication"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_Projector"
  ==> "PushContainer_ApiLegacy"
  ==> "PushContainer_ApiExtract"
  ==> "PushContainer_ApiCrabImport"
  ==> "PushContainer_ProjectionsSyndication"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
