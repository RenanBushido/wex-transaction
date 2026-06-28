# WexTransaction

# Requirements

## Requirement #1: Store a Purchase Transaction

Your application must be able to accept and store (i.e., persist) a purchase transaction with a description, transaction
date, and a purchase amount in **United States dollars**. When the transaction is stored, it will be assigned a **unique identifier**.

## Field requirements

● Description: must not exceed 50 characters
● Transaction date: must be a valid date format
● Purchase amount: must be a valid positive amount rounded to the nearest cent
● Unique identifier: must uniquely identify the purchase

## Requirement #2: Retrieve a Purchase Transaction in a Specified Country’s Currency

Provide a way to retrieve the stored purchase transactions converted to currencies supported by the **Treasury Reporting Rates of Exchange API**
based upon the exchange rate active for the date of the purchase.

**https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange**

The retrieved purchase should include the identifier, the description, the transaction date, the original US dollar purchase
amount, the exchange rate used, and the converted amount based upon the specified currency’s exchange rate for the
date of the purchase.

## Currency conversion requirements

● When converting between currencies, you do not need an exact date match, but must use a currency conversion rate 
    less than or equal to the purchase date from within the last 6 months.
● If no currency conversion rate is available within 6 months equal to or before the purchase date, an error should be 
    returned stating the purchase cannot be converted to the target currency.
● The converted purchase amount to the target currency should be rounded to two decimal places (i.e., cent).

## General Structure

- [x] The project will use **OpenSpec** as its AI tool.
- [x] The project will use Clean Architecture as its design architecture.
- [x] The project will be divided into layers, adhering to the architectural design.
- [x] There is no information regarding the volume of requests, but the application will be prepared.
- [x] Build the **Domain** layer.
- [x] Build the **Application** layer.
- [x] Build the **Infrastructure** layer.
- [ ] Build the **Presentation** layer.


## OpenSpec

-   I used OpenSpec to help me create this application. I employed the Spec-Driven Development methodology to build the application, refining each step of the project structure whenever possible.
-   After that, I chose Clean Architecture and the SOLID principles as the design architecture.
-   Also i created a **SKILL** to review each step that I advanced

## Domain Layer

-   I used some conecepts of DDD, creating IEntity and IAuditablesEntity for shared the same identity.
-   Simplify extensibility and enable polymorphism and follow the principles Open/Close of SOLID. 
-   ExchangeRateSelector it's Domain Service (DDD) application orquestration.

## Infrastructure Layer

-   This layer has 2 projects (Database and ServiceRatesExchange)
-   **Database**: 
    -   Implemented the IBaseRepository, IUnitOfWork and ITransactionRepository Interfaces. I Used repository pattherns with IUnitOfWork, preparing the application to avoid concurrency of events like: Queries and Persistences.
    -   I used EF Core as the persistence tool.
    -   I prepared the code for execute Migrations.

-   **Services RatesExchange**:
    -   I used **Refit** e **HttpClient Factory** as tool to call an external API.

## Application Layer

-   Implemented 2 ways  
    

Domain Layer - Estrutura & Organização:
- ✅ GlobalUsings.cs criado com imports de projeto (segue CLAUDE.md)
- ✅ Estrutura de pastas bem organizada:
  - Common/ — Interfaces base (IEntity, IEntityAuditable) ✓
  - Entities/ — Agregados raiz (PurchaseTransaction) ✓
  - ValueObjects/ — Valores imutáveis (Money, TransactionDescription, ExchangeRate, ConvertedTransactionResult) ✓
  - Exceptions/ — Exceções de domínio customizadas ✓
  - Services/ — Serviços de domínio puros (ExchangeRateSelector) ✓
  - Interfaces/ — Contratos adicionais ✓

Database Layer - Estrutura & Implementação:
- ✅ GlobalUsings.cs com imports apropriados (Microsoft.EntityFrameworkCore, ValueConversion, etc) ✓
- ✅ Estrutura modular:
  - Extensions/ — PersistenceExtensions (segue padrão CLAUDE.md) ✓
  - Repositories/ — ITransactionRepository, TransactionRepository com padrão genérico ✓
  - Config/ — Configuração de entidades EF Core ✓
  - Data/ — DbContext (WexTransactionDbContext) ✓
  - Migrations/ — Versionamento EF Core ✓

Aderência a CLAUDE.md:
- ✅ Extension Pattern: PersistenceExtensions em Extensions/ folder ✓
- ✅ Fluent Interface: AddPersistence retorna IServiceCollection ✓
- ✅ Async/Await: Todos os métodos são async (Task<T>) ✓
- ✅ Single Responsibility: Cada classe tem responsabilidade única ✓
- ✅ SOLID Principles:
  - S: Repository tem única responsabilidade (CRUD) ✓
  - O: Aberto para extensão (Generic Repository pattern) ✓
  - L: Implementações substituem interfaces corretamente ✓
  - I: Interfaces segregadas (ITransactionRepository, IEntity, IEntityAuditable) ✓
  - D: Depende de abstrações (ITransactionRepository, IConfiguration) ✓

Clean Architecture:
- ✅ Domain Layer: Não depende de nada (Domain é independente) ✓
- ✅ Database Layer: Depende apenas de Domain ✓
- ✅ Sem dependências circulares ✓
- ✅ ValueConverters para mapear value objects ao banco ✓

Base Repository Pattern (Novo):
- ✅ Classe genérica BaseRepository<T> para operações CRUD comuns ✓
- ✅ TransactionRepository herda de BaseRepository, adiciona lógica específica ✓
- ✅ Reduz duplicação de código ✓

UnitOfWork Pattern (Novo):
- ✅ IUnitOfWork interface para coordenar transações ✓
- ✅ UnitOfWork implementação com Commit() assíncrono ✓
- ✅ Centraliza SaveChanges() logic ✓

🔍 Boas Práticas Aplicadas

✅ Repository Pattern: Abstração clara de data access
✅ Unit of Work Pattern: Coordenação de transações
✅ Value Converters: Mapeamento elegante de value objects
✅ Generic Base: BaseRepository reduz código duplicado
✅ Null Checks: Validação apropriada em constructores
✅ Async All The Way: Sem sync-over-async antipattern
✅ Configuration-Driven: Connection strings em appsettings.json