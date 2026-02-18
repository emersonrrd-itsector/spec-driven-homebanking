# HomeBanking

**A Spec-Driven Modern Banking Application**

HomeBanking is a robust, full-stack banking application built with a focus on specification-driven development. It features a scalable .NET 9 API backend and a responsive React frontend, orchestrated via Docker for seamless deployment and development.

## 🚀 Key Features

*   **Modern Tech Stack**: Built with the latest .NET 9 and React 18 capabilities.
*   **Spec-Driven Development**: Architecture and implementation strictly follow defined specifications.
*   **Containerized**: Fully Dockerized environment for consistent development and deployment.
*   **Comprehensive Testing**: Includes Unit, Integration, and End-to-End (E2E) test suites.
*   **CI/CD Ready**: Automated pipelines using GitHub Actions.

## 🛠 Tech Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Backend** | ![.NET 9](https://img.shields.io/badge/.NET%209-512BD4?style=flat&logo=dotnet&logoColor=white) | REST API, Entity Framework Core |
| **Frontend** | ![React](https://img.shields.io/badge/React-20232A?style=flat&logo=react&logoColor=61DAFB) ![Vite](https://img.shields.io/badge/Vite-646CFF?style=flat&logo=vite&logoColor=white) | React 18, TypeScript, Tailwind CSS |
| **Database** | ![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat&logo=microsoft-sql-server&logoColor=white) | SQL Server (Docker) / LocalDB (Dev) |
| **DevOps** | ![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white) | Container orchestration |
| **CI/CD** | ![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-2088FF?style=flat&logo=github-actions&logoColor=white) | Automated testing and build pipelines |

## 🏁 Quick Start

To get the application running quickly with Docker:

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/your-org/homebanking.git
    cd homebanking/spec-driven-homebanking
    ```

2.  **Run with Docker Compose**:
    ```bash
    docker compose up --build
    ```

3.  **Access the Application**:
    *   **Web App**: [http://localhost:3000](http://localhost:3000)
    *   **API Swagger**: [http://localhost:5001/swagger](http://localhost:5001/swagger)

## 📚 Documentation

For detailed instructions and architectural insights, please refer to the documentation:

*   [📅 Setup Guide](docs/setup.md) - prerequisites and running locally/docker.
*   [🧪 Testing Guide](docs/testing.md) - running unit and E2E tests.
*   [🏗 Architecture](docs/architecture.md) - project structure and design patterns.
*   [🔄 CI/CD](docs/ci-cd.md) - GitHub Actions workflows.
