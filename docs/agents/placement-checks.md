# The placement checks

`docs/agents/rules/file-placement.md` holds the rules the code was written under. Read them there, and judge placement and direction against what they say. Read the context map beside it too, where the repo has one, because a rule reaches only the code the map gives it.

Cite a breach by the name of the check that catches it. Where a rule has no check behind it, cite the rule by its number. List each check here as the repo gains one:

| Check | The rule it runs |
|---|---|
| `SliceFoldersTests.EveryFirstLevelFolderUnderACodeRootIsASliceOrShared` | 1 |
| `SliceFoldersTests.EveryFolderInsideAFrontEndSliceIsAConcern` | 2 |
| `SliceReadsTests.ASliceNeverReadsAnotherSlice` | 3 |
| `SliceReadsTests.SharedNeverReadsASlice` | 4 |
| `SharedDoorTests.EveryFolderInSharedNamesAWordOfTheGlossary` | 5 |
| `SliceFoldersTests.ASliceFolderSitsAtTheFirstLevelUnderItsOwnNameInEveryCodeRoot` | 6 |
| `SliceFoldersTests.EverySliceIsAHeadwordOfTheGlossary` | 8 |
| `SA1402` and `SA1649`, StyleCop.Analyzers in every build, set in `.editorconfig` | Files: a C# file holds one top-level type, named as the subject |
| `GlobalUsingsTests.NoFileHoldsOnlyUsings` and `GlobalUsingsTests.NoUsingItemOutsideATestProjectNamesASlice` | Files: global usings go in the project file |
| `IDE0130` in every build, set in `.editorconfig` and `Directory.Build.props` | Files: a C# namespace is the root namespace, then the folder path |
| `TestPlacementTests.EveryTestSitsWhereTheCodeItTestsSits` | Tests: where a test sits |
| `FolderSizeTests.NoFolderHoldsMoreSubjectsThanTheLimit` | Folder size |
| `NameMapTests.ASubjectThatMatchesAPatternSitsInThatPatternsFolder` | Name map |
| `BannedFolderNamesTests.NoFolderCarriesABannedName` | One shape: banned folder names |
| `RS0030`, Microsoft.CodeAnalysis.BannedApiAnalyzers in every build, reading `BannedSymbols.txt` | `determinism.md`: Time |
| `BannedWordsTests.NoSourceFileUsesAWordThatLost` | `words.md` |
| `DocCommentsTests.DocCommentsSitWhereTheSettingSays` | `comments.md`: doc comments |

The table is an index from a breach back to a rule, and the rules themselves stay in the one file.

The starter tests live in `tests/RaggidyRag.Architecture.Tests`. They read each rule's YAML block at every run, so a changed setting changes what they check. The project proves the repo and holds no Slice, so the eight Slice rules leave it out, and it belongs to no context. A Slice test skips, and says so, while no context declares Slices.

## The checks that prove placement

A review runs the commands listed here, each in its folder from the repo root, and nothing else. Where a row names a command to run first, run it first in the same folder, unless the path it names is there in that folder.

List the narrowest commands that prove placement, and leave out the slow ones that prove something else. Write one row per command, and put the command and the folder in backticks. A front end's boundary lint, for example, is the command `make lint`, the folder `web`, and run first `make install`, unless `deps` is there.

| Command | Folder | Run first |
|---|---|---|
| `dotnet test --project tests/RaggidyRag.Architecture.Tests` | `.` | |
| `dotnet build RaggidyRag.slnx` | `.` | |

The first row runs the starter tests. The second runs the analyzers, `SA1402`, `SA1649`, `IDE0130` and `RS0030`, which fail the build.

## The two bends

Holding the author and the reviewer to one document is the point of the Architecture axis, so two items of the arrangement baseline in [`review-architecture.md`](review-architecture.md) bend wherever the repo has written the rule down:

- **A shared folder with a written door is not grab-bag growth.** The baseline item is about a folder nobody decided on. Judge against the door the repo wrote, not against the name.
- **Duplication across a boundary can be correct.** Where the repo says two modules may hold one name, the merge is the defect, not the duplication.
