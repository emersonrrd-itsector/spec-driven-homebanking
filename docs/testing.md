# Testing Guide

HomeBanking emphasizes quality through a comprehensive testing strategy involving Unit Tests (API & Web) and End-to-End (E2E) tests.

## 🧪 Unit Tests

### Backend (API)

The API tests are built using **xUnit**. They cover domain logic, controllers, and services.

1.  Navigate to the root directory or API test directory.
2.  Run the tests:
    ```bash
    dotnet test
    ```
    
    To run tests for a specific project:
    ```bash
    dotnet test src/api/HomeBanking.Tests
    ```

### Frontend (Web)

The web frontend tests use **Vitest** for unit and component testing.

1.  Navigate to the `src/web` directory:
    ```bash
    cd src/web
    ```
2.  Run the tests:
    ```bash
    npm run test
    ```

---

## 🎭 End-to-End (E2E) Tests

E2E tests are implemented using **Playwright**. They simulate real user interactions to verify the system works as a whole.

### Prerequisites for E2E

*   Ensure the application (API + Web) is running.
    *   **Docker**: `docker compose up` (Web at `http://localhost:3000`).
    *   **Manual**: Ensure API (`5087`) and Web (`5173`) are running. Note that default Playwright config targets `http://localhost:3000` (Docker) or `http://localhost:5173` (Dev). Check `playwright.config.ts` for the `baseURL`.

### Running E2E Tests Locally

1.  Navigate to the web directory:
    ```bash
    cd src/web
    ```

2.  Install Playwright browsers (first time only):
    ```bash
    npx playwright install
    ```

3.  Run the tests (headless mode):
    ```bash
    npx playwright test
    ```

4.  Run tests with UI (interactive mode):
    ```bash
    npx playwright test --ui
    ```

### Running E2E in CI/Docker

The CI pipeline runs Playwright tests against the containerized environment. To simulate this locally:

1.  Ensure Docker containers are up (`docker compose up`).
2.  Run tests targeting the Docker environment:
    ```bash
    # Assuming config is set to localhost:3000 or via env var
    BASE_URL=http://localhost:3000 npx playwright test
    ```
