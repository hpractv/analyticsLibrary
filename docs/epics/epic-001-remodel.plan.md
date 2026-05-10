# Epic 001: Remodel Analytics Library Packages

## Summary

Remodel `analyticsLibrary` from one broad NuGet package into a set of smaller, focused packages. The remodel will also move source and installer-related code into a `src` folder, remove database provider functionality for SQL Server, Oracle, SAS, and Sybase, and update documentation, packaging metadata, and tests to match the new package boundaries.

This is a breaking-change epic. Removing public namespaces and changing package layout requires a major version release and clear migration guidance for downstream consumers.

## Goals

- Split the current monolithic `analyticsLibrary` package into smaller NuGet packages with clear responsibilities.
- Move application/library code and installer-related code/project assets into a top-level `src` folder.
- Remove SQL Server, Oracle, SAS, and Sybase namespace functionality from the supported codebase.
- Remove provider-specific dependencies that are no longer needed after those namespaces are deleted.
- Update retained NuGet dependencies to current supported versions and replace deprecated packages with well-supported alternatives.
- Preserve reusable, non-provider-specific functionality by moving it into neutral packages before deleting provider code.
- Add automated tests around retained packages before or during the split.
- Add GitHub workflows that build, unit test, package, and deploy NuGet packages.
- Produce beta NuGet packages when a PR is opened or updated.
- Rebuild, retest, package, and deploy release NuGet packages after PR approval.
- Update README, NuGet metadata, release notes, and migration guidance to reflect the new package model.

## Non-Goals

- Do not redesign retained APIs unless required to break package coupling.
- Do not keep compatibility shims for removed SQL Server, Oracle, SAS, or Sybase namespaces.
- Do not introduce new database providers as part of this epic.
- Do not publish packages until the split builds, tests pass, and package metadata is reviewed.

## Current State

The repository currently has one solution and one NuGet-producing project:

- `Analytics Library.sln`
- `analyticsLibrary/analyticsLibrary.csproj`

The current project targets `netcoreapp3.1` and packages as:

- `PackageId`: `analyticsLibrary`
- `AssemblyName`: `analyticsLibrary`
- `RootNamespace`: `analyticsLibrary`
- `Version`: `1.0.3`
- `PackageTags`: `analytics, csv, data, excel, oracle, sas, sql server, sybase, statistics`

The current package pulls all dependencies into every consumer, including provider-specific dependencies:

- `System.Data.SqlClient`
- `Oracle.ManagedDataAccess.Core`
- `SimpleImpersonation`
- `OO_DBMS.ADODBCoreWrapper`
- `EntityFramework`
- `EPPlus`
- `System.Data.OleDb`

The repository does not currently have an automated test project or CI workflow.

## Proposed Repository Layout

Create a top-level `src` folder and move code-bearing projects and installer-related project assets under it.

Proposed layout:

```text
src/
  analyticsLibrary.Core/
  analyticsLibrary.Excel/
  analyticsLibrary.Access/
  analyticsLibrary.Hadoop/
  analyticsLibrary.Statistics/
  analyticsLibrary.Algorithms/
  analyticsLibrary/
tests/
  analyticsLibrary.Core.Tests/
  analyticsLibrary.Excel.Tests/
  analyticsLibrary.Statistics.Tests/
  analyticsLibrary.Algorithms.Tests/
docs/
  epics/
    epic-001-remodel.plan.md
```

The final project list can be adjusted during implementation, but the code should no longer live directly under the repository root. Installer-related code, setup project files, release configuration, and packaging assets should move under `src` unless they are documentation-only assets that belong under `docs`.

## Proposed NuGet Packages

Use the existing project brand while creating focused package identities.

- `analyticsLibrary.Core`: CSV, fixed-width, shared data objects, neutral helpers, and common extensions.
- `analyticsLibrary.Excel`: Excel file operations and Excel-specific dependencies such as EPPlus and OleDb.
- `analyticsLibrary.Access`: Access database file support if retained.
- `analyticsLibrary.Hadoop`: Hadoop/ODBC support if retained.
- `analyticsLibrary.Statistics`: covariance, dot product, histogram, normalize, standard deviation, variance, and related statistics helpers.
- `analyticsLibrary.Algorithms`: search, sort, and computer science helpers.
- `analyticsLibrary`: optional transition/metapackage that depends on retained packages but does not reintroduce removed providers.

Package dependencies should flow from specialized packages to core packages. Core packages should not depend on Excel, Access, Hadoop, or other specialized packages.

## Namespace Strategy

Preferred namespace model:

- `analyticsLibrary.Core`
- `analyticsLibrary.Excel`
- `analyticsLibrary.Access`
- `analyticsLibrary.Hadoop`
- `analyticsLibrary.Statistics`
- `analyticsLibrary.Algorithms`

During implementation, move types into namespaces that match their package. Avoid leaving retained code in namespaces named after deleted or unrelated providers.

Removed namespaces should not remain as obsolete wrappers:

- `analyticsLibrary.dbObjects.sqlDb`
- `analyticsLibrary.oracle`
- `analyticsLibrary.sas`
- `analyticsLibrary.sybase`

## Provider Removal Scope

### SQL Server

Remove SQL Server-specific implementation from:

- `analyticsLibrary/dbObjects/sqlDb.cs`
- SQL Server bulk-copy paths in `analyticsLibrary/dbObjects/dataLibrary.cs`

Remove supporting dependencies if no retained code uses them:

- `System.Data.SqlClient`
- `SimpleImpersonation`
- `EntityFramework`, if it remains unused by retained code

Important coupling:

- `sqlDb.dataTypeFromString` is referenced by provider code outside SQL Server. Before removing `sqlDb`, move any retained type-mapping behavior into a neutral core helper if Hadoop or other retained code still needs it.

### Oracle

Remove Oracle-specific implementation from:

- `analyticsLibrary/oracle/oracle.cs`
- `analyticsLibrary/oracle/extensions.cs`

Remove supporting dependencies and configuration if no retained code uses them:

- `Oracle.ManagedDataAccess.Core`
- Oracle provider entries in `App.config`
- Oracle-related suppression entries in `GlobalSuppressions.cs`
- Oracle package tags and README claims

### SAS

Remove SAS-specific implementation from:

- `analyticsLibrary/sas/sas.cs`

Remove supporting dependency if no retained code uses it:

- `OO_DBMS.ADODBCoreWrapper`

Open decision:

- `analyticsLibrary/library/extensions.cs` includes `fromSasDate`. Decide during implementation whether to remove it with SAS support, rename it to a neutral date parser, or retain it as a generic parser for SAS-exported text files.

### Sybase

Remove Sybase-specific implementation from:

- `analyticsLibrary/sybase/sybase.cs`

Remove Sybase README claims, package tags, and related suppression entries. ODBC support may still be needed if Hadoop remains in scope.

## Workstreams

### 1. Source Layout

- Create `src`.
- Move the existing library project under `src`.
- Move installer-related code/project assets under `src`.
- Update solution paths and project references.
- Keep `docs`, `LICENSE`, and root-level repository metadata at the repository root.

### 2. Package Split

- Create new SDK-style project files for each retained package.
- Move source files into the package project that owns them.
- Establish project references so specialized packages depend on `analyticsLibrary.Core`.
- Configure package metadata consistently across all packages.
- Decide whether to keep `analyticsLibrary` as a transition package.

### 3. Provider Removal

- Delete SQL Server, Oracle, SAS, and Sybase implementation files.
- Remove provider dependencies from all project files.
- Remove provider configuration from `App.config` and release artifacts.
- Remove provider-specific package tags and README references.
- Remove provider-specific analyzer suppressions.

### 4. Shared Utility Extraction

- Identify shared helpers currently living in provider-specific classes.
- Move retained shared behavior into `analyticsLibrary.Core`.
- Replace references to provider-specific helpers with neutral helpers.
- Avoid preserving removed provider type names only for compatibility.

### 5. Tests

- Add test projects under `tests`.
- Cover retained CSV/fixed-width behavior.
- Cover retained statistics behavior.
- Cover retained algorithms behavior.
- Cover retained Excel behavior where practical without requiring local Office installation.
- Add focused tests for any shared helpers extracted from provider-specific classes.

### 6. Documentation and Migration

- Rewrite `README.md` as comprehensive user-facing documentation for the remodeled package family.
- Add README badges for package version and GitHub workflow build/test status.
- Include version badges for each published NuGet package or a clearly documented primary package badge if a single badge is preferred.
- Include build/test result badges for the primary CI workflow and package/release workflow.
- Update the README introduction so it describes the current purpose of the project, the package split, supported target frameworks, and the removed provider areas.
- Add a package overview section that lists each NuGet package, its purpose, its primary namespace, and its core dependencies.
- Add a namespace reference section that documents every new public namespace, the package that owns it, and the functionality available through that namespace.
- Add functionality sections for each retained area, including CSV/fixed-width data, Excel operations, Access support if retained, Hadoop support if retained, statistics, algorithms, and shared core helpers.
- Include representative type and method examples for the main retained APIs so consumers can discover the new namespace locations.
- Add installation examples for the package family and each retained package.
- Add quick-start examples for common retained use cases, including CSV/fixed-width data, Excel operations, statistics, and algorithms.
- Add a repository layout section that explains `src`, `tests`, `docs`, and workflow locations.
- Add build and test instructions for local development.
- Add NuGet package and release process documentation, including beta packages from PRs and release packages after approval.
- Add contribution guidance for package boundaries, test expectations, and dependency rules.
- Remove deleted provider claims for SQL Server, Oracle, SAS, and Sybase.
- Add migration notes for consumers moving from `analyticsLibrary` 1.x to the remodel release.
- Document removed namespaces and dependencies.
- Document the new `src` and `tests` repository layout.
- Update NuGet package tags and descriptions for each package.

### 7. GitHub Workflows and NuGet Deployment

- Add GitHub Actions workflows under `.github/workflows`.
- Add a pull request workflow that restores dependencies, builds all projects, runs unit tests, packs all NuGet packages, and publishes beta packages.
- Use prerelease versions for PR packages, such as `major.minor.patch-beta.pr-<number>.<run_number>`.
- Store PR package artifacts on the workflow run even when beta publishing is skipped or unavailable.
- Publish beta packages only from trusted PR contexts where NuGet credentials are available.
- Add a release workflow that runs after PR approval, rebuilds from the approved commit, reruns tests, repacks packages, and deploys release packages to NuGet.
- Protect NuGet release publishing with a GitHub Environment, required reviewers, or equivalent repository protection so approval is explicit and auditable.
- Use repository secrets for NuGet API keys. Do not commit package credentials.
- Fail the deployment if package metadata, package versions, or test results are invalid.
- Include package inspection steps so the workflow verifies expected `.nupkg` files are produced for each retained package.

### 8. Dependency Modernization

- Audit all current package references for support status, deprecation status, known vulnerabilities, license compatibility, and target framework compatibility.
- Remove package references that only supported deleted SQL Server, Oracle, SAS, or Sybase functionality.
- Upgrade retained packages to current stable supported versions where the upgrade does not require unrelated redesign.
- Replace deprecated or poorly supported packages with better-supported libraries when retained functionality still needs that capability.
- Prefer package choices that support the selected target frameworks and work cleanly in CI on GitHub-hosted runners.
- Consider central package management with `Directory.Packages.props` if the remodel creates several projects with shared dependency versions.
- Document any intentional version pinning, especially when a newer package version has breaking changes, licensing impact, platform restrictions, or unsupported target framework requirements.
- Validate dependency changes with build, unit tests, package generation, and package inspection.

## Acceptance Criteria

- Code and installer-related project assets live under `src`.
- Retained packages build from the solution.
- Removed provider namespaces are no longer compiled or packaged.
- `System.Data.SqlClient`, `Oracle.ManagedDataAccess.Core`, `SimpleImpersonation`, and `OO_DBMS.ADODBCoreWrapper` are removed unless retained code has a documented reason to keep them.
- SQL Server, Oracle, SAS, and Sybase are removed from README feature lists and NuGet package tags.
- Retained packages have package IDs, descriptions, tags, license metadata, repository metadata, and version metadata.
- Retained dependencies are upgraded to supported versions or replaced with documented supported alternatives.
- Removed-provider dependencies are no longer referenced by retained packages.
- Deprecated packages are removed unless a documented exception explains why temporary retention is required.
- Tests exist for each retained package with meaningful logic.
- `README.md` includes version badges, build/test result badges, package overview, namespace reference, functionality reference, installation, quick starts, repository layout, build/test instructions, release workflow notes, contribution guidance, and migration guidance.
- GitHub workflows build, test, and pack every retained NuGet package.
- PR workflows produce beta package artifacts and publish beta packages when credentials are available.
- Approved PRs trigger a release workflow that rebuilds, tests, packages, and deploys release packages to NuGet.
- NuGet deployment uses repository secrets and protected deployment controls.
- A migration note documents removed namespaces, replacement packages, and expected breaking changes.
- The solution builds cleanly after the split.

## Risks

- Downstream consumers may rely on public provider APIs that will be removed.
- Shared helpers currently located in provider-specific classes may be accidentally deleted.
- Moving projects under `src` may break solution paths, installer references, package output paths, or release scripts.
- Excel and Access support may keep Windows-specific dependencies, so those packages should stay isolated from core packages.
- Lack of existing tests means implementation may need characterization tests before moving code.
- Publishing beta packages for PRs can leak untrusted code into NuGet if fork and secret handling is not constrained.
- Deploying release packages on PR approval instead of merge requires careful versioning and branch protection so NuGet releases only come from intended commits.
- Package upgrades can introduce breaking API, licensing, runtime, or platform behavior changes that need focused validation.
- Replacing deprecated packages may require small API redesigns in retained packages.

## Suggested Sequence

1. Add tests around high-value retained behavior in the current layout.
2. Create `src` and move the existing project plus installer-related project assets.
3. Split the current project into focused package projects.
4. Extract neutral shared helpers into `analyticsLibrary.Core`.
5. Remove SQL Server, Oracle, SAS, and Sybase provider files and dependencies.
6. Audit, upgrade, and replace retained package dependencies as needed.
7. Add GitHub workflows for build, unit test, package, beta package publish, and release NuGet deployment.
8. Update package metadata, README, and migration notes.
9. Build, test, inspect generated packages, and prepare a major version release.

## Open Decisions

- Should the root `analyticsLibrary` package remain as a transition/metapackage or be discontinued after the split?
- Should `analyticsLibrary.Access` and `analyticsLibrary.Hadoop` be retained in this remodel, or deferred to later epics?
- Should `fromSasDate` be removed with SAS support or renamed into a neutral parser?
- Should target frameworks remain on `netcoreapp3.1`, or should package split work include a target framework modernization plan?
- Should release package deployment happen immediately on PR approval, or should approval mark the package releasable and deploy after merge to the protected release branch?
- Should dependency versions be managed centrally with `Directory.Packages.props` after the package split?
