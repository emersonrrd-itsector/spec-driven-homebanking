# Tech Stack

This document outlines the technology stack used in the Spec-Driven HomeBanking project. Our choices prioritize performance, type safety, maintainability, and modern development standards.

## Backend

*   **Runtime:** [.NET 9](https://dotnet.microsoft.com/)
    *   **Rationale:** Chosen for its high performance, strong type system, cross-platform capabilities, and Long Term Support (LTS) readiness.
*   **Language:** C# 12/13
*   **Framework:** ASP.NET Core Web API
*   **Database Access:** Entity Framework Core (EF Core)
    *   **Rationale:** Provides a robust ORM for strictly typed database interactions and migrations.
*   **Testing:** xUnit
    *   **Rationale:** The hidden standard for .NET unit testing, offering extensibility and parallel test execution.
*   **Containerization:** Docker
    *   **Rationale:** Ensures consistence across development and deployment environments.
*   **Authentication:** JWT (JSON Web Tokens)

## Frontend

*   **Library:** [React 18](https://react.dev/)
    *   **Rationale:** Component-based architecture allows for reusable UI elements and excellent state management ecosystem.
*   **Language:** TypeScript
    *   **Rationale:** Adds static typing to JavaScript, reducing runtime errors and improving developer tooling/refactoring capabilities.
*   **Build Tool:** [Vite](https://vitejs.dev/)
    *   **Rationale:** Extremely fast server start and HMR (Hot Module Replacement) compared to Webpack.
*   **Styling:** [Tailwind CSS](https://tailwindcss.com/)
    *   **Rationale:** Utility-first CSS framework that speeds up UI development and ensures design consistency.
*   **Unit Testing:** [Vitest](https://vitest.dev/)
    *   **Rationale:** Native Vite test runner, faster than Jest for Vite projects.
*   **E2E Testing:** [Playwright](https://playwright.dev/)
    *   **Rationale:** Reliable end-to-end testing with support for multiple browsers and easy debugging/tracing.

## Infrastructure & DevOps

*   **Orchestration:** Docker Compose
    *   **Rationale:** Simplified local development setup to spin up API, Database, and Frontend with a single command.
*   **Database Engine:** SQL Server (running in Docker)
    *   **Rationale:** Robust relational database that pairs naturally with the Microsoft .NET ecosystem.
*   **CI/CD:** GitHub Actions (Planned/In-use)
    *   **Rationale:** Integrated directly with the source code repository for automated building and testing.

## Editor & Tools

*   **Primary IDE:** Visual Studio Code or Visual Studio 2022.
*   **AI Assistance:** GitHub Copilot.
