# Setup Guide

This guide covers the prerequisites and steps to run the HomeBanking application locally, either manually or using Docker.

## Prerequisites

Ensure you have the following installed on your machine:

*   **[Docker Desktop](https://www.docker.com/products/docker-desktop/)**: For containerized execution.
*   **[.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)**: For backend development.
*   **[Node.js 22](https://nodejs.org/)**: For frontend development (LTS version recommended).

---

## 🐳 Running with Docker (Recommended)

The easiest way to run the full stack (API + Web + Database) is using Docker Compose.

1.  Navigate to the solution root:
    ```bash
    cd spec-driven-homebanking
    ```

2.  Build and start the services:
    ```bash
    docker compose up --build
    ```

3.  Access the application:
    *   **Web Frontend**: [http://localhost:3000](http://localhost:3000)
    *   **API**: [http://localhost:5001](http://localhost:5001) (Health check: `/health`)

To stop the services, press `Ctrl+C` or run:
```bash
docker compose down
```

---

## 💻 Running Locally (Manual)

If you prefer to run services individually for development:

### 1. Backend (API)

1.  Navigate to the API project directory:
    ```bash
    cd src/api/HomeBanking.API
    ```

2.  Run the application:
    ```bash
    dotnet run
    ```
    
    *   The API will start on **http://localhost:5087** (or https://localhost:7297).
    *   Swagger UI: http://localhost:5087/swagger

### 2. Frontend (Web)

1.  Navigate to the Web project directory:
    ```bash
    cd src/web
    ```

2.  Install dependencies:
    ```bash
    npm install
    ```

3.  Start the development server:
    ```bash
    npm run dev
    ```

    *   The Web app will start on **http://localhost:5173**.
    *   **Note**: Ensure the API is running. You might need to configure the `VITE_API_URL` environment variable if the API port strictly differs from default.

---
