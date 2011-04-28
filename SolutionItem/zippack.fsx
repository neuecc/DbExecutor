// archive zip for release package
#r "Ionic.Zip.dll"

open System
open System.IO
open System.Text.RegularExpressions
open Ionic.Zip

let rootDir = (new DirectoryInfo(__SOURCE_DIRECTORY__)).Parent
let pass p = Path.Combine(rootDir.FullName , p)

let flat = [
    "SolutionItem\ReadMe.txt";
    "ReleaseBinary\DbExecutor.dll";
    "ReleaseBinary\DbExecutor.pdb";
    "ReleaseBinary\DbExecutor.xml"
    ]

let contracts = [
    "ReleaseBinary\CodeContracts\DbExecutor.Contracts.dll";
    "ReleaseBinary\CodeContracts\DbExecutor.Contracts.pdb";
    ]

do
    use zip = new ZipFile()
    flat 
    |> Seq.map (fun x -> new FileInfo(pass x)) 
    |> Seq.iter (fun x -> zip.AddFile(x.FullName, "") |> ignore)
    
    contracts 
    |> Seq.map (fun x -> new FileInfo(pass x)) 
    |> Seq.iter (fun x -> zip.AddFile(x.FullName, "CodeContracts") |> ignore)
    
    pass "archive.zip" |> zip.Save