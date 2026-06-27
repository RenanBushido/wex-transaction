# infra-database

## Purpose

EF Core persistence layer with DbContext, repositories, and database migrations for purchase transactions.

## Requirements

### Requirement: EF Core DbContext for purchase transactions

The system SHALL provide a DbContext that manages purchase transaction entities using Entity Framework Core with PostgreSQL as the backing database.

#### Scenario: DbContext is configured for PostgreSQL
- **WHEN** the application starts
- **THEN** DbContext is configured to connect to PostgreSQL using the connection string from configuration

#### Scenario: Automatic migration on application startup
- **WHEN** the application starts
- **THEN** pending EF Core migrations are applied automatically if database is not up to date

#### Scenario: Transaction entity is mapped to database
- **WHEN** a PurchaseTransaction is persisted
- **THEN** it is stored in the `purchase_transactions` table with columns: id, description, transaction_date, amount

### Requirement: Generic repository pattern for persistence

The system SHALL provide generic repository interfaces and implementations for data access operations.

#### Scenario: Repository retrieves transaction by ID
- **WHEN** repository is queried by transaction ID
- **THEN** the corresponding PurchaseTransaction is returned or null if not found

#### Scenario: Repository persists new transaction
- **WHEN** a new PurchaseTransaction is saved via repository
- **THEN** it is inserted into the database with a generated GUID id

#### Scenario: Repository supports async operations
- **WHEN** repository methods are called
- **THEN** they return Task<T> for async/await usage

### Requirement: Database initialization with migrations

The system SHALL support database creation and schema management through EF Core migrations.

#### Scenario: Migration for initial schema
- **WHEN** the first migration is created
- **THEN** it creates the `purchase_transactions` table with appropriate columns, constraints, and indexes

#### Scenario: Migration can be reverted
- **WHEN** a migration needs to be rolled back
- **THEN** EF Core supports reverting to a previous schema state

### Requirement: Service registration via extension

The system SHALL register all persistence services in the DI container through an extension method in the Extensions folder.

#### Scenario: PersistenceExtensions registers DbContext
- **WHEN** PersistenceExtensions.AddPersistence() is called
- **THEN** DbContext and repositories are registered in IServiceCollection

#### Scenario: Connection string is read from configuration
- **WHEN** AddPersistence() is invoked
- **THEN** it reads the DefaultConnection string from IConfiguration
