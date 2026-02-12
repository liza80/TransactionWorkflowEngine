# Transaction Workflow Engine

A backend service that manages customer transactions with configurable workflow statuses.

## Technology

- .NET 10
- SQL Server LocalDB
- Entity Framework Core
- xUnit

## How to Run

1. Clone the repository
2. Open `TransactionWorkflowEngine.slnx` in Visual Studio
3. Press F5 to run
4. Navigate to `https://localhost:PORT/swagger`

**Database:** Created automatically on first run using LocalDB. The app calls `Database.Migrate()` on startup which:
- Creates the database if it doesn't exist
- Applies all migrations
- Seeds initial statuses and transitions

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/transactions` | Create a new transaction |
| GET | `/transactions/{id}` | Get transaction by ID |
| POST | `/transactions/{id}/transition` | Move to next status |
| GET | `/transactions/{id}/available-transitions` | Get allowed transitions |
| GET | `/transactions/{id}/history` | Get status history |

## Workflow

```
CREATED → VALIDATED → PROCESSING → COMPLETED
                            ↘ FAILED → VALIDATED (retry)
```

## Design Decisions

1. **Data-driven workflow** - Statuses and transitions stored in database, not hardcoded
2. **Service layer** - Business logic in `TransactionService`, not in controllers
3. **Workflow caching** - Transitions cached in memory for performance
4. **Concurrency** - RowVersion for optimistic concurrency control
5. **Status history** - All transitions recorded with timestamp and reason

## Tradeoffs

| Choice | Alternative | Reason |
|--------|-------------|--------|
| EF Core | Dapper | Better for complex relationships |
| LocalDB | Docker SQL | Easier local setup |
| Memory cache | Redis | Simpler for single instance |

## Running Tests

```bash
dotnet test
```
