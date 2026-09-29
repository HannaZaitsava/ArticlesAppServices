# 🛠️ ArticlesApp Copilot Instructions

**Project:** Enterprise ASP.NET Core Web API for Articles Management  
**Architecture:** Clean Architecture + CQRS + HybridCache (L1 In-Memory + L2 Redis)  
**Target:** .NET 10 | ASP.NET Core Web API

These guidelines help AI assistants generate code consistent with the ArticlesApp patterns and standards.

---

## 🎯 Project Overview

ArticlesApp is a **production-ready** REST API with:
- **4-layer Clean Architecture**: Domain → Application → Infrastructure → Presentation
- **CQRS pattern**: Separate Command and Query handlers with MediatR
- **Result Pattern**: All operations return typed `Result<T>` with domain errors
- **HybridCache**: Dual-level caching (In-Memory L1 + Redis L2) in MediatR Pipeline Behaviors
- **Repository + Unit of Work**: Complete abstraction over Entity Framework Core
- **FluentValidation**: Request validation in Pipeline Behaviors (not in handlers)
- **Mapster**: Configuration-based mapping via `IRegister` interface
- **Enterprise Testing**: xUnit + AutoFixture + Testcontainers (PostgreSQL 18 + Redis 8)

**Key principle:** Handlers are thin, business logic sits in Domain, infrastructure concerns stay isolated.

---

## 🏗️ Architecture & Layer Dependencies

### Layer Structure
```
┌─────────────────────────────────────────┐
│  Presentation (Controllers, Models)     │ ← API contracts, thin routing
├─────────────────────────────────────────┤
│  Application (CQRS, Validators, DTOs)   │ ← Business orchestration, caching logic
├─────────────────────────────────────────┤
│  Infrastructure (Repos, DbContext, UoW) │ ← EF Core, PostgreSQL, Redis
├─────────────────────────────────────────┤
│  Domain (Entities, Errors, Results)     │ ← Pure domain logic, no frameworks
└─────────────────────────────────────────┘
```

### Dependency Rules
✅ **DO:**
- Application depends on Domain
- Infrastructure depends on Application + Domain
- Presentation depends on Application + Domain
- Controllers inject only `IMediator` and `IMapper`

❌ **DON'T:**
- Domain depends on anything (sealed & pure)
- Application directly uses `DbContext` (go through `IBaseRepository<T>`)
- Handlers access HTTP context directly (use `IUserContext` interface)
- Infrastructure leaks into Application types (use abstractions/interfaces)

### Architecture Tests
The project enforces layers with **ArchUnitNET** tests in `Tests/ArchitectureTests/LayerTests.cs`. Verify your changes don't break layer isolation.

---

## 💾 Database Access & Unit of Work Pattern

### Repository Pattern (IBaseRepository<T>)
All data access goes through typed repositories, **never** direct `DbContext`.

**Location:** `src/Infrastructure/DataAccess/Repositories/BaseRepository.cs`

**Key methods:**
- `GetByIdAsync(Guid id, bool trackChanges = true, CancellationToken ct)` — For Commands with tracking
- `GetByIdAsync(Guid id, bool trackChanges = false, CancellationToken ct)` — For Queries without tracking
- `GetAllAsync(Expression<Func<TEntity, bool>>? predicate, bool trackChanges = false, CancellationToken ct)` — List queries
- `GetByIdProjectedAsync<TDto>(Guid id, CancellationToken ct)` — Direct DTO mapping in query
- `AddAsync(TEntity entity, CancellationToken ct)` — Add new entity
- `SaveChangesAsync(CancellationToken ct)` — Persist changes

### Unit of Work Pattern (IUnitOfWork)
Use `IUnitOfWork` **only for multi-aggregate transactions** in Commands.

**Location:** `src/Infrastructure/DataAccess/UOW/UnitOfWork.cs`

**When to use IUnitOfWork:**
- Multiple aggregates must persist atomically
- Explicit transaction management needed (`BeginTransactionAsync()` / `CommitTransactionAsync()`)
- Related entities across multiple repositories
- Typical handler flow: Validate → Add entities → Commit atomically → Return result

**When NOT to use:**
- Single aggregate persistence (use `SaveChangesAsync()` on repository)
- Simple read operations (use `GetByIdAsync(trackChanges: false)`)
- Queries (never use in handlers that return `IRequest<Result<T>>`)

### EF Core Best Practices
- ✅ Always use `AsNoTracking()` for Queries (see `BaseRepository.GetAllAsync(trackChanges: false)`)
- ✅ Use `.FilterByExpression()` extension for LINQ predicates
- ✅ Return `IEnumerable<T>` from repository (let consumer decide if enumeration needed)
- ✅ Use projections (`.GetByIdProjectedAsync<DTO>()`) to avoid mapping entities → DTOs after fetch
- ❌ Don't call `.ToList()` in repository — let LINQ materialize at boundary

---

## 🔄 Caching Strategy (HybridCache)

### Overview
Caching is **handled in MediatR Pipeline Behaviors**, not inside handlers. This keeps handlers clean.

**Architecture:**
```
Request → Validation Behavior → Logging Behavior → Caching Behavior → Performance Behavior → Handler
														↓
											(Check L1 In-Memory Cache)
														↓
											(Check L2 Redis Cache)
														↓
											(Execute Handler)
														↓
											(Cache Result if Success)
```

### Cache Behavior Implementation
**Location:** `src/Application/Common/Behaviors/CachingBehavior.cs`

**Rules:**
1. Only `IRequest<TResponse>` implementing `ICachableRequest` are cached
2. Only **successful** `Result<T>` data is cached (errors bypass cache)
3. Cache stores **serialized DTO**, not the `Result<T>` wrapper
4. Authenticated users bypass cache **read** but still update it (fresh data always)
5. `BypassCache` flag on query skips both read and write

### ICachableRequest Interface
**Location:** `src/Application/Common/Caching/ICachableRequest.cs`

Queries implementing `ICachableRequest` enable HybridCache integration:
- `GetCacheKeyMetadata()` — Generate unique cache key based on parameters
- `CacheTags` — Tag for invalidation (e.g., when article is created/updated)
- `ExpirationSeconds` — L2 Redis TTL (in seconds)
- `LocalCacheExpirationSeconds` — L1 In-Memory TTL (in seconds)
- `BypassCache` — Skip both read and write (optional override)

### HybridCacheService
**Location:** `src/Infrastructure/Cache/HybridCacheService.cs`

**Key method behavior:**
- If cache hit: return cached data instantly
- If cache miss or bypass: execute factory (handler) → serialize result → store in cache
- If handler returns `Result<T>.Failure`: ❌ NOT cached, error returned to client
- If authenticated user: read bypassed, but fresh data always cached for future anonymous requests

### Cache Tags & Invalidation
**Location:** `src/Application/Common/Caching/CacheTags.cs`

Use tags to invalidate related caches when mutations occur. When creating/updating/deleting aggregates, invalidate the corresponding cache tags (Articles, Comments, Tags, etc.) to ensure fresh data is fetched on next query.

---

## 🛍️ CQRS Implementation

### Commands & Queries Format

#### Commands (Mutations)
**File pattern:** `src/Application/CQRS/Commands/{AggregateType}/{ActionName}/{ActionNameCommand.cs}`

Command types:
- **Sealed record with positional parameters** (compact, preferred for 2-3 fields)
- **Sealed record with init properties** (for more fields)

❌ **DON'T:** Mutable classes or unsealed records

#### Queries (Reads with Caching)
**File pattern:** `src/Application/CQRS/Queries/{AggregateType}/{QueryName}/{QueryNameQuery.cs}`

Queries should be sealed records implementing `ICachableRequest` for caching support, or non-cached if expensive operations. Always return `IRequest<Result<T>>`.

### Command & Query Handlers

**File pattern:** `src/Application/CQRS/{Commands|Queries}/.../...Handler.cs`

**Command Handler Pattern:**
- Validate existence (read-only repository calls with `trackChanges: false`)
- Map request to domain entity (via Mapster)
- Apply domain logic if needed
- Persist via repository's `AddAsync()` and `SaveChangesAsync()`
- Return `Result<T>.Success()` or `Result<T>.Failure()` based on outcome

**Query Handler Pattern:**
- Fetch data using repository (preferably with `GetByIdProjectedAsync<DTO>()` for direct DTO mapping)
- Check if entity exists; return appropriate error if not
- Return `Result<T>.Success()` with the data

### MediatR Pipeline Behaviors

All cross-cutting concerns live in Pipeline Behaviors, registered in **dependency injection order**:

**Location:** `src/Application/Common/Behaviors/`

#### Order of Execution
```
1. PerformanceBehavior<,>    ← Logs execution time
2. LoggingBehavior<,>         ← Logs request/response
3. ValidationBehavior<,>      ← Validates request
4. CachingBehavior<,>         ← Caches query results
↓
Handler Execution
```

**Registration (DI):**
Services should register behaviors in order: PerformanceBehavior → LoggingBehavior → ValidationBehavior → CachingBehavior

#### ValidationBehavior
Runs all `IValidator<TRequest>` registered for the request type.
- ✅ Chains validation results
- ✅ Converts FluentValidation errors to typed `Error` objects
- ✅ Returns `Result<T>.Failure()` immediately if any validator fails
- ❌ Never throws exceptions (use Result pattern instead)

#### CachingBehavior
Only applies to `IRequest<T> & ICachableRequest`.
- ✅ Reads from L1 cache first, then L2 Redis
- ✅ Bypasses cache read for authenticated users
- ✅ Never caches failed results
- ✅ Serializes DTO and wraps in Result for return

#### LoggingBehavior & PerformanceBehavior
- Log request name, response type, execution time
- Part of production observability stack (Serilog + Seq)

### Result Pattern & Error Handling

**Location:** `src/Domain/Result/Result.cs`

All handlers return typed `Result<T>`:
- `IsSuccess` — Boolean flag for success
- `Value` — Typed result data (if success)
- `Errors` — List of typed errors (if failure)
- `Success(T value)` — Factory for successful result
- `Failure(List<Error> errors)` — Factory for failed result

**Error Structure:**
Each error contains:
- `Name` — Property name or error code
- `Message` — User-friendly message
- `Type` — ErrorType enum (Failure, Validation, NotFound, AlreadyExist, Conflict, Unauthorized, Forbidden, BadRequest, Unexpected)

**Usage Pattern:**
- Return single error: `Result<Guid>.Failure([CommentErrors.CommentNotFound(id)])`
- Return multiple validation errors: Collect errors in a list
- Return success: `Result<Guid>.Success(comment.Id)`

---

## 🗺️ Mapping (Mapster)

### Configuration via IRegister
**Location:** `src/ArticlesAPI/Models/MappingConfigurations/`

Mapster uses convention + configuration scanning. Define mappings via `IRegister`:
- Request → Command mappings
- Tuple + Request → Command (for route parameters + body)
- Request → Query mappings
- Entity → DTO projections (for queries)

### Mapper Dependency Injection
**Location:** `src/ArticlesAPI/Extensions/DI/MapperExtensions.cs`

- Scan Presentation layer assembly for configurations
- Register `IMapper` via Mapster
- Scan Application layer configurations via `AddApplicationMapperConfigurations()`

### Best Practices
- ✅ Use `IRegister` implementations for configurations
- ✅ Scan assemblies to auto-discover configs: `config.Scan(Assembly.GetExecutingAssembly())`
- ✅ Use explicit mappings for complex scenarios: `.Map(dest => dest.Field, src => src.Property)`
- ✅ Inline `.Adapt<T>()` for simple, convention-based mappings in code
- ✅ Prefer projections: `repository.GetByIdProjectedAsync<DTO>()` (mapping in query)
- ❌ Don't mix configurations across layers (keep each layer's config in that layer)
- ❌ Don't use `.Compile()` in request handlers (do it once in DI setup)

---

## ✅ Validation (FluentValidation)

### Validator Structure
**File pattern:** `src/Application/CQRS/Validators/{CommandOrQuery}Validator.cs`

Validators inherit from `AbstractValidator<TRequest>` with:
- `NotEmpty()` rules with `WithMessage()` for required fields
- `Length()` constraints with template placeholders like `{MinLength}` and `{MaxLength}`
- Constants for constraint boundaries (e.g., `CommentConstraints.MaxTextLength`)

### Validation Pipeline
Validators are **auto-discovered** and executed in `ValidationBehavior<,>` before handler.


### Best Practices
- ✅ Validators inherit from `AbstractValidator<TRequest>`
- ✅ Use constants for constraint boundaries: `CommentConstraints.MaxTextLength`
- ✅ Message templates with placeholders: `"Must be {MinLength} to {MaxLength} chars"`
- ✅ One validator per Command/Query class
- ✅ Validators live in Application layer, only validate input contracts
- ❌ Don't do database lookups in validators (use handler for existence checks)
- ❌ Don't throw exceptions from validators (FluentValidation returns errors)
- ❌ Don't put business logic in validators (keep them simple)

### Registration
Validators are auto-registered via reflection in DI. The framework automatically discovers all `IValidator<T>` implementations during application startup.

---

## 🎛️ Controllers & API Endpoints

### BaseApiController
**Location:** `src/ArticlesAPI/Controllers/BaseApiController.cs`

All controllers inherit from `BaseApiController`:
- Base route: `[Route("api/[controller]")]`
- Response format: `[Produces("application/json")]`
- Standard response types: 400 BadRequest, 401 Unauthorized, 403 Forbidden, 500 InternalServerError

**Why `[Produces("application/json")]`?**
- Fixes Swagger media type (default: `text/plain` breaks ProblemDetails handler)
- Ensures consistent JSON error responses

### Thin Controller Pattern
**Location:** `src/ArticlesAPI/Controllers/ArticlesController.cs`

Controllers do **only:**
1. Extract route/query/body parameters
2. Map to CQRS request (via Mapster)
3. Send to MediatR
4. Map result to HTTP response

**Flow:**
1. Inject only `IMediator` and `IMapper`
2. Extract parameters from `[FromRoute]`, `[FromQuery]`, or `[FromBody]`
3. Send CQRS request via `_mediator.Send()`
4. Check `result.IsSuccess` and return appropriate HTTP status
5. Convert domain errors to ProblemDetails via `result.Errors.ToProblem(HttpContext)`
6. Use `CreatedAtRoute()` for POST (201 with Location header)

### Error Mapping Extension
**Location:** `src/ArticlesAPI/Extensions/ErrorMappingExtensions.cs`

Extension method converts `List<Error>` to HTTP problem response:
- Maps error types to HTTP status codes (NotFound → 404, Validation → 400, etc.)
- Combines multiple errors into single response with Detail field
- Sets Response.StatusCode and returns ObjectResult with ProblemDetails

### Best Practices
- ✅ Inject only `IMediator` and `IMapper`
- ✅ Use `[Authorize]` attribute for protected endpoints
- ✅ Include XML documentation (for Swagger)
- ✅ Use `[ProducesResponseType]` for each possible response
- ✅ Check `result.IsSuccess` and handle both paths
- ✅ Map domain entities to API response DTOs
- ✅ Use `CreatedAtRoute()` for POST (201 with Location header)
- ❌ Don't put business logic in controllers
- ❌ Don't query database directly (use CQRS only)
- ❌ Don't catch exceptions in controllers (let global error handler deal with it)

---

## 🧪 Testing Standards

### Integration Tests

**Pattern:** Testcontainers (PostgreSQL 18 + Redis 8) + HttpClient + Arrange-Act-Assert

**Base Fixture:**
- PostgreSQL container initialized once per collection
- Redis container initialized once per collection  
- Respawner for database state reset between tests
- Migrations applied automatically

**Base Test Class:**
- Inherits `IClassFixture<ArticlesAppFactory>`
- `[Collection("ArticlesApp Collection")]` for shared setup
- Access to `DbContext`, `HttpClient`, `IFixture` (AutoFixture)
- `ClearTracker()` method to reset change tracker after Arrange phase

### Unit Tests

**Pattern:** xUnit + AutoFixture + Moq + FluentAssertions


### Best Practices
- ✅ Use **Arrange-Act-Assert** structure for all tests
- ✅ **Integration tests**: One container setup per collection (static initialization)
- ✅ **Unit tests**: Mock all dependencies, inject via constructor
- ✅ Use `IFixture` + `AutoFixture` for test data generation
- ✅ Use `FluentAssertions` for readable assertions
- ✅ Clear `DbContext.ChangeTracker` after Arrange in integration tests
- ✅ Use `Respawner` to reset database state between tests
- ✅ Test both happy path and error cases
- ✅ Name tests: `{MethodName}_{Condition}_{ExpectedResult}`
- ❌ Don't use multiple Testcontainers per test (use collection fixtures)
- ❌ Don't leave test data in database (use Respawner or transaction rollback)
- ❌ Don't hardcode IDs or magic strings (use `Guid.NewGuid()`, fixtures)

---

## 📍 Routing Rules

These rules enable Copilot to activate the right block based on file path and naming patterns.

| **Context Block** | **File Patterns** | **Activation Rule** | **Priority** |
|---|---|---|---|
| **CQRS: Commands** | `*Command.cs`, `*CommandHandler.cs`, `*CommandValidator.cs` | Path contains `/Commands/` AND filename ends with `Command` | HIGH |
| **CQRS: Queries** | `*Query.cs`, `*QueryHandler.cs` | Path contains `/Queries/` AND filename ends with `Query` | HIGH |
| **Repository & UoW** | `*Repository.cs`, `UnitOfWork.cs`, `IUnitOfWork.cs`, `IBaseRepository.cs` | Path contains `/Repositories/` or `/DataAccess/` | HIGH |
| **Caching** | `*CacheService.cs`, `CachingBehavior.cs`, `ICachableRequest.cs`, `*CacheTags.cs` | Path contains `/Cache/` or filename contains `Caching` or `Cache` | HIGH |
| **Validation** | `*Validator.cs`, `ValidationBehavior.cs` | Filename ends with `Validator` or contains `Validation` | HIGH |
| **Mapster Config** | `*MappingConfig.cs`, `*MappingConfiguration.cs`, `MapperExtensions.cs` | Path contains `/MappingConfigurations/` or filename contains `Mapping` | HIGH |
| **Controllers** | `*Controller.cs`, `BaseApiController.cs` | Path contains `/Controllers/` AND filename ends with `Controller` | HIGH |
| **Integration Tests** | `*Tests.cs` in `/IntegrationTests/` | Path contains `/IntegrationTests/` AND class inherits `BaseIntegrationTest` | MEDIUM |
| **Unit Tests** | `*Tests.cs` in `/UnitTests/` | Path contains `/UnitTests/` AND uses `IFixture` + `Mock` | MEDIUM |
| **Architecture Tests** | `LayerTests.cs`, `*LayerTests.cs` | Filename contains `LayerTests` or `ArchitectureTests` | MEDIUM |
| **Domain: Result & Errors** | `Result.cs`, `Error.cs`, `ErrorType.cs`, `*Errors.cs` | Path contains `/Domain/` AND (`Result` or `Error` in filename) | MEDIUM |
| **API Contracts** | `*ApiRequest.cs`, `*ApiResponse.cs`, `*Request.cs`, `*Response.cs` | Path contains `/Models/` or `/Contracts/` in Presentation layer | MEDIUM |
| **DI & Configuration** | `DependencyInjection.cs`, `*Extensions.cs` in `/DI/` or `/Extensions/` | Path contains `/DI/` or `/Extensions/` in any layer | LOW |
| **Behaviors** | `*Behavior.cs` | Path contains `/Behaviors/` | HIGH |

### How to Use Routing Rules
1. **When creating a new Command handler** → Copilot auto-loads `CQRS: Commands` block
2. **When editing a Repository** → Copilot auto-loads `Repository & UoW` block
3. **When writing integration test** → Copilot auto-loads `Integration Tests` block
4. **Multiple matches** → Highest priority block is primary; secondary blocks provide context


## 🔗 Quick References

- **Project Root:** `src/`
- **Solution File:** `ArticlesApp.slnx`
- **Repository:** `git@github.com:HannaZaitsava/ArticlesApp.git`
- **Target Framework:** .NET 10
- **ORM:** Entity Framework Core 10 + PostgreSQL 18
- **Cache:** HybridCache (In-Memory L1 + Redis 8 L2)
- **Testing:** xUnit + AutoFixture + Testcontainers + FluentAssertions


**Last Updated:** 2026  
**Version:** 1.0  
**Maintainer:** ArticlesApp Team
