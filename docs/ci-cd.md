# CI/CD Pipeline

HomeBanking utilizes **GitHub Actions** for Continuous Integration and Continuous Deployment (CI/CD) to ensure code quality and reliability.

## 🚀 Workflow Overview

The primary workflow is defined in `.github/workflows/ci.yml`. It triggers automatically on:
*   **Push** to the `main` branch.
*   **Pull Requests** targeting the `main` branch.

## ⚙️ Jobs

The pipeline works in stages to validate both backend and frontend components.

### 1. `build-and-test-api`
Focuses on the .NET Backend.
*   **Setup**: Sets up the .NET 9 SDK.
*   **Restore**: Restores NuGet dependencies.
*   **Build**: Compiles the solution to check for build errors.
*   **Test**: Runs unit tests using `dotnet test`.

### 2. `build-and-test-web`
Focuses on the React Frontend.
*   **Setup**: Sets up Node.js 22.
*   **Install**: Installs npm packages (`npm ci`).
*   **Lint**: Runs code linting to ensure code style compliance.
*   **Build**: Builds the production bundle (`npm run build`).

### 3. `e2e-tests`
Runs End-to-End tests against the full application stack. This job **depends on** the successful completion of the API and Web build jobs.
*   **Setup**: Configures Node.js and Playwright.
*   **Service Startup**: Uses `docker compose up` to spin up the API and Web containers.
*   **Wait**: Scripts wait for services to be healthy (answering on ports 5001 and 3000).
*   **Test**: Runs Playwright tests against the running containers.
*   **Artifacts**: Uploads test reports if the run fails, aiding in debugging.

## 📊 Status Checks

GitHub requires all these jobs to pass before a Pull Request can be merged (if branch protection is enabled). This guarantees that:
1.  Code compiles.
2.  Unit tests pass.
3.  The application works as a whole in a production-like containerized environment.
