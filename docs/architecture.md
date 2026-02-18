# Architecture Overview

HomeBanking is designed as a modern, decoupled web application following a spec-driven development approach.

## 🏗 High-Level Architecture

The system consists of three main components:

1.  **Frontend (Web)**: A Single Page Application (SPA) built with React 18 and Vite. It serves as the user interface for banking operations.
2.  **Backend (API)**: A .NET 9 Web API that handles business logic, authentication, and data persistence.
3.  **Database**: SQL Server (containerized) or In-Memory (for isolated testing/dev) used for storing user and transaction data.

These components are typically orchestrated using **Docker Compose** for a consistent runtime environment.

## 📂 Project Structure

The repository is organized to separate concerns clearly:

```
spec-driven-homebanking/
├── .github/                # GitHub workflows and configuration
├── docs/                   # Project documentation
├── src/
│   ├── api/                # Backend Source Code
│   │   ├── HomeBanking.API/      # .NET 9 Web API project
│   │   ├── HomeBanking.Domain/   # Domain models and logic
│   │   └── HomeBanking.Tests/    # xUnit test project
│   │
│   └── web/                # Frontend Source Code
│       ├── src/            # React components, hooks, and services
│       ├── src/e2e/        # Playwright E2E tests
│       └── ...             # Vite and project config
├── docker-compose.yml      # Orchestration for API, Web, and DB
└── README.md               # Entry point documentation
```

## 🧩 Key Design Patterns

### Spec-Driven Development
The project follows a "Specification First" approach. Features are defined in detailed markdown specifications (e.g., `spec.md` or feature-specific files) before implementation begins. This ensures alignment between requirements and code.

### API Design
*   **RESTful**: The API follows REST principles for resource management.
*   **Layered Architecture**: Separation of concerns between Controllers, Services, and Data Access (Entity Framework Core).
*   **Dependency Injection**: Extensive use of .NET's built-in DI container for testability and modularity.

### Frontend Architecture
*   **Component-Based**: Modular UI using React components.
*   **Hooks**: Custom hooks for logic encapsulation (e.g., data fetching, auth).
*   **Typed**: strictly typed with TypeScript for robustness.
