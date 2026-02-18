# Contributing to Spec-Driven HomeBanking

Thank you for your interest in contributing to the Spec-Driven HomeBanking project! We value your contributions and want to make the process as easy and transparent as possible.

## How to Contribute

### Reporting Bugs

If you find a bug, please create a new issue in the repository. Be sure to include:

*   **A clear and descriptive title.**
*   **Steps to reproduce:** detailed steps to reproduce the behavior.
*   **Expected behavior:** what you expected to happen.
*   **Actual behavior:** what actually happened.
*   **Environment:** OS, Browser, .NET version, Node version.
*   **Screenshots or logs** if applicable.

### Suggesting Enhancements

We welcome ideas for improvements! Open an issue with the tag `enhancement` and describe:

*   The current behavior.
*   The proposed change.
*   The motivation for the change.

### Pull Requests

1.  **Fork the repository** and create your branch from `main`.
2.  **Follow the Code Style** guidelines (see below).
3.  **Ensure all tests pass** locally before submitting.
4.  **Issue reference:** If your PR addresses an existing issue, please link to it in the description.

## PR Checklist

Before submitting your Pull Request, please ensure the following:

- [ ] **Tests Pass:** run `dotnet test` for the backend and `npm run test` (or `npx playwright test`) for the frontend.
- [ ] **Linter Check:** Verify there are no linting errors (e.g., `npm run lint` for frontend).
- [ ] **No Build Errors:** The solution must build cleanly.
- [ ] **Documentation:** Update `README.md` or other docs if you changed behavior or added features.
- [ ] **Clean History:** Squash trivial commits if possible to keep the history clean.

## Code Style

### General Principles

*   **Spec-Driven Development:** Implementation should strictly follow the documented specs (`spec.md`) and requirements. If the code deviates from the spec, update the spec or fix the code.
*   **Clean Code:** Write readable, maintainable code. Variable names should be descriptive. Functions should represent a single responsibility.
*   **SOLID Principles:** adhere to SOLID design principles where applicable.

### Backend (.NET)

*   Follow standard C# coding conventions.
*   Use `async/await` for I/O bound operations.
*   Use strict typing where possible.
*   Services should be injected via Dependency Injection.

### Frontend (React/TypeScript)

*   Use functional components and Hooks.
*   Strict TypeScript types (avoid `any` unless absolutely necessary).
*   Components should be small and focused.
*   Use Tailwind CSS utility classes for styling.

Thank you for contributing!
