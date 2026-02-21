# SwitchableReferences

Automatically switches between `ProjectReference` and `PackageReference` based on which solution you open. Works with Visual Studio, Rider, `dotnet build`, and NCrunch (including grid nodes).

**Default: ProjectReference.** PackageReference is used only when a project is absent from the current solution or an explicit override is set.

Requires `.slnx` solution format (.NET 10+ default; migrate older solutions with `dotnet sln migrate`).

---

## Setup

### 1. Import in `Directory.Build.props`

Import `SwitchableReferences.props` and declare one property per switchable dependency:

```xml
<Project>

  <Import Project="$(MSBuildThisFileDirectory)build\SwitchableReferences.props" />

  <PropertyGroup>
    <!-- Acme.Utilities -->
    <UsePackageReference_Acme_Utilities
        Condition="'$(UsePackageReference_Acme_Utilities)' != 'true'
                 And '$(_SwitchRef_SolutionProjects)' != ''
                 And !$(_SwitchRef_SolutionProjects.Contains('|Acme.Utilities.csproj|'))">true</UsePackageReference_Acme_Utilities>

    <!-- Acme.Core -->
    <UsePackageReference_Acme_Core
        Condition="'$(UsePackageReference_Acme_Core)' != 'true'
                 And '$(_SwitchRef_SolutionProjects)' != ''
                 And !$(_SwitchRef_SolutionProjects.Contains('|Acme.Core.csproj|'))">true</UsePackageReference_Acme_Core>
  </PropertyGroup>

</Project>
```

### 2. Add conditional references in each `.csproj`

Replace plain references with a conditional pair. References **must** be in the `.csproj` file (not in imported files) and use conditional `<ItemGroup>` (not conditional items):

```xml
<!-- Acme.Utilities — switchable reference -->
<ItemGroup Condition="'$(UsePackageReference_Acme_Utilities)' == 'true'">
  <PackageReference Include="Acme.Utilities" Version="3.1.0" />
</ItemGroup>
<ItemGroup Condition="'$(UsePackageReference_Acme_Utilities)' != 'true'">
  <ProjectReference Include="..\Acme.Utilities\Acme.Utilities.csproj" />
</ItemGroup>
```

### 3. Configure NCrunch

NCrunch cannot evaluate the auto-detection, so it needs explicit flags in `.v3.ncrunchsolution` files.

**Full solution** (all projects included) — nothing to set:

```xml
<SolutionConfiguration>
  <Settings />
</SolutionConfiguration>
```

**Consumer-only solution** (some libraries come from NuGet):

```xml
<SolutionConfiguration>
  <Settings>
    <CustomBuildProperties>
      <Value>UsePackageReference_Acme_Utilities = true</Value>
      <Value>UsePackageReference_Acme_Core = true</Value>
    </CustomBuildProperties>
  </Settings>
</SolutionConfiguration>
```

These flags propagate to NCrunch grid nodes automatically.

---

## Adding a New Switchable Dependency

Property name convention: `UsePackageReference_{PackageName_with_underscores}`

**In `Directory.Build.props`** — add one property:

```xml
    <!-- {DESCRIPTION} -->
    <{PROPERTY}
        Condition="'$({PROPERTY})' != 'true'
                 And '$(_SwitchRef_SolutionProjects)' != ''
                 And !$(_SwitchRef_SolutionProjects.Contains('|{CSPROJ_FILENAME}|'))">true</{PROPERTY}>
```

**In each consuming `.csproj`** — add a conditional pair:

```xml
<ItemGroup Condition="'$({PROPERTY})' == 'true'">
  <PackageReference Include="{PACKAGE_NAME}" Version="{VERSION}" />
</ItemGroup>
<ItemGroup Condition="'$({PROPERTY})' != 'true'">
  <ProjectReference Include="{PROJECT_PATH}" />
</ItemGroup>
```

**In `.v3.ncrunchsolution`** — for consumer-only solutions, add:

```xml
<Value>{PROPERTY} = true</Value>
```

---

## CLI / CI Overrides

```shell
dotnet build /p:UsePackageReference_Acme_Utilities=true
```

Or via environment variable:

```shell
set UsePackageReference_Acme_Utilities=true
dotnet build
```

---

## Troubleshooting

- **NCrunch shows stale test results** — Check that `CustomBuildProperties` in your `.v3.ncrunchsolution` matches which libraries are/aren't in the solution.
- **Build error: project file not found** — The sibling repo isn't checked out, or you're building without a solution context (default is ProjectReference). Build via the `.slnx`, or add an `Exists()` guard (see below).
- **VS IntelliSense shows wrong references** — Close and reopen the solution, or run a manual NuGet restore.