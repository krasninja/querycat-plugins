# Schema

- [roslyn_projects](#roslyn_projects)

## **roslyn_projects**

```
roslyn_projects(solution_path: String): Object<IRowsInput>
```

Return projects within the solution.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `Id`| `Void` |  |  | The ID of the project. Multiple Project instances may share the same ID. |
| `Name`| `String` |  |  | The name of the project. This may be different than the assembly name. |
| `AssemblyName`| `String` |  |  | The name of the assembly this project represents. |
| `language`| `String` |  |  | The language associated with the project. |
| `Version`| `String` |  |  | The project version. This equates to the version of the project file. |
| `FilePath`| `String` |  |  | The path to the project file or null if there is no project file. |
| `OutputFilePath`| `String` |  |  | The path to the output file, or null if it is not known. |
