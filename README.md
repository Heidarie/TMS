# Task Management System

## Setup instruction

1. Make sure you have **Docker** and **docker-compose** installed on your machine
2. Clone this repository:
   ```
   git clone https://github.com/Heidarie/TMS.git
   cd your-repo
   ```
3. Run the service with
   `
   docker-compose up --build
   `
4. Once everything is up, you can access the API documentation via Swagger at
   `
   http://localhost:8080/swagger/index.html
   `
   
### Service overview
 - API: http://localhost:8080
 - Swagger UI: http://localhost:8080/swagger/index.html
 - PostgreSQL: localhost:5432 (user: postgres, password: Password123)
 - RabbitMQ UI: http://localhost:15672 (username: guest, password: guest)

## Design overview

**TMS** is a modular application designed to manage tasks, supporting full CRUD operations, task progress updates, and integration with external systems such as RabbitMQ for messaging.

---

## Architecture

The system follows the **Clean Architecture** pattern, separating concerns into distinct layers:

- **API Layer** – Handles HTTP requests/responses.
- **Application Layer** – Contains business logic, DTOs, and service interfaces.
- **Infrastructure Layer** – Implements repositories, services, and external integrations.
- **Domain Layer** – Defines core entities, enums, and domain events.

---

## Key Components

### 1. API Layer
- **File**: `TMS.Api/Program.cs`
- **Responsibilities**:
  - Configures the application (Swagger, middleware, dependency injection).
  - Defines RESTful endpoints for task operations:
    - `GET /api/tasks`: Fetch all tasks
    - `POST /api/tasks`: Create a new task
    - `PUT /api/tasks/{id}`: Update task progress
  - Handles exceptions and maps them to appropriate HTTP responses.

---

### 2. Application Layer
- **Key Files**:
  - `ITaskService.cs`: Service interface for task operations.
  - `CreateTaskDto.cs`: DTO for creating a task.
- **Responsibilities**:
  - Encapsulates business logic in services (e.g., `TaskService`).
  - Validates input data (e.g., task name, description).
  - Maps domain entities to DTOs for API responses.

---

### 3. Infrastructure Layer
- **Key Files**:
  - `TaskRepository.cs`: Implements data access (Entity Framework).
  - `TaskService.cs`: Implements `ITaskService`, interacts with repo.
  - `TasksDbContext.cs`: Configures the EF database context.
- **Responsibilities**:
  - Manages CRUD operations for tasks.
  - Dispatches domain events to RabbitMQ (e.g., on task completion).
  - Logs and handles database-level errors.

---

### 4. Domain Layer
- **Key Files**:
  - `TaskItem.cs`: Core domain entity representing a task.
  - `Status.cs`: Enum for task status (`NotStarted`, `InProgress`, `Completed`).
- **Responsibilities**:
  - Encapsulates domain behavior (e.g., progress updates).
  - Tracks and publishes domain events.

---

## Testing

### 1. Unit Tests
- **File**: `TMS.Tests/Services/TaskServiceTests.cs`
- **Responsibilities**:
  - Validate business logic in `TaskService`.
  - Use mocks for `ITaskRepository`, `IDomainEventDispatcher`.
  - Test scenarios like:
    - Creating tasks (valid/invalid input)
    - Updating tasks in various states

### 2. Integration Tests
- **File**: `TMS.Tests/Integrations/IntegrationTests.cs`
- **Responsibilities**:
  - End-to-end testing of API functionality
  - Use **Testcontainers** for PostgreSQL & RabbitMQ
  - Test scenarios:
    - Creating, fetching, updating tasks
    - Verifying RabbitMQ messages on task completion

---

## Dependencies

- **Database**: PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Messaging**: RabbitMQ (`RabbitMQ.Client`)
- **Testing**:
  - `xUnit`: Test framework
  - `Moq`: Mocking dependencies
  - `Testcontainers`: Containerized integration testing

---

## Workflow Overview

### 1. Task Creation
- `POST /api/tasks` with `CreateTaskDto`
- Validates input → creates `TaskItem` → saves via `TaskRepository`
- Returns the created task as DTO

### 2. Task Retrieval
- `GET /api/tasks`
- Fetches all tasks → maps to DTOs

### 3. Task Update
- `PUT /api/tasks/{id}`
- Retrieves task → updates status → saves
- If completed → dispatches domain event to RabbitMQ

### 4. Messaging
- On task completion, sends `TaskCompletedMessage` to RabbitMQ

---

## Error Handling

- **API Layer**: Maps exceptions to HTTP responses (400, 404, etc.)
- **Infrastructure Layer**: Logs and wraps low-level exceptions in domain-specific errors

---

## Design Trade-offs & Limitations

---

### 1. Repository Design

**Trade-offs:**
-  **Simplicity** – The Repository Pattern provides a clean abstraction for data access, improving maintainability and testability.
-  **Tightly Coupled to EF Core** – Direct usage of `DbContext` and `DbSet` creates coupling to Entity Framework, reducing flexibility to swap ORMs.

**Limitations:**
-  **Limited Query Flexibility** – Only basic CRUD operations are supported; complex queries (e.g., filtering, sorting) require extensions.
  
---

### 2. Exception Handling

**Trade-offs:**
-  **Centralized Logging** – Database-related errors are logged, aiding debugging.
-  **Consistent Error Types** – Exceptions are wrapped (e.g., in `InvalidOperationException`) for uniform handling.

**Limitations:**
-  **Generic Messages** – Errors like "Failed to create task" lack specific context (e.g., which field/entity failed).

---

### 3. Entity Design

**Trade-offs:**
-  **Encapsulation** – Domain logic resides in the `TaskItem` class (e.g., `UpdateProgress`, `MarkInProgress`), following DDD principles.
-  **Controlled Mutability** – Use of private setters ensures more predictable state changes.

**Limitations:**
-  **Limited Validation** – Entity-level validation (e.g., for name length) is missing and relies on database constraints.

---

### 4. Migration Design

**Trade-offs:**
-  **Schema Synchronization** – Migrations align the database schema with the domain model.
-  **Schema Namespacing** – Using a dedicated `tasks` schema improves modularity in multi-schema setups.

**Limitations:**
-  **No Indexing** – Lacks indexes on commonly queried fields (e.g., `Status`), which could degrade performance at scale.

---

### 5. Data querying

**Limitations:**
-  **No Pagination** – `GetAllTasksAsync` loads all tasks at once, which may impact performance with large datasets.
