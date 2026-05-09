# CommitLink

**CommitLink** is a high-fidelity project management application designed to bridge the gap between development activity and task management. It provides a modern, seamless experience for developers and teams to track progress directly alongside their code changes.

## 🚀 The Vision
Most project management tools feel disconnected from the actual work happening in the repository. CommitLink changes that by making **GitHub commits first-class citizens** within your project workflow.

## 🔗 Unique Feature: Intelligent Commit Linking
CommitLink's standout feature is its ability to automatically synchronize and link repository activity to your project tasks.

- **Auto-Linking via ID**: Simply include the Task ID (e.g., `#123`) in your GitHub commit message. CommitLink will automatically detect the commit and link it to the corresponding task.
- **Smart Status Updates**: You can update a task's status directly from your terminal. By including both the ID and a status keyword (e.g., `#123 [Done]` or `#123 [In-Progress]`), CommitLink will transition the task's state on the Kanban board automatically upon synchronization.
- **Traceability**: Every task shows a complete audit trail of the specific code changes (SHAs) that contributed to its completion.

## ✨ Key Features
- **Modern Kanban Board**: Manage tasks with a high-performance Syncfusion-powered Kanban interface.
- **Full GitHub Lifecycle**: From connecting repositories to tracking live commits, stay synced with your codebase.
- **SPA Experience**: Built with **HTMX**, the application feels like a Single Page Application (SPA)—fast, responsive, and without full-page reloads.
- **High-Fidelity UI**: A "Dribbble-style" aesthetic featuring clean typography (Inter), soft shadows, and a professional indigo palette.
- **Collaborative Workspaces**: Manage team members, roles (Owner, Manager, Member), and permissions across multiple projects.

## 💎 What Makes CommitLink Different?
- **Code-Centric Transparency**: Unlike generic tools, CommitLink shows exactly which code changes completed a task, right inside the task details.
- **Zero-Friction Workflow**: Developers don't need to leave their IDE or terminal to update task progress—the commit message does the work.
- **HTMX-Powered Efficiency**: Get the responsiveness of a heavy JavaScript framework with the simplicity and performance of server-side rendering.

## 🛠️ Tech Stack
- **Backend**: ASP.NET Core 9.0 (C#)
- **Frontend Interactivity**: HTMX (SPA Architecture)
- **UI Components**: Syncfusion EJS (High-fidelity components)
- **Styling**: Bootstrap 5 + Custom Modern CSS
- **Database**: Entity Framework Core + SQL Server
- **Integration**: GitHub API

## 🏁 Getting Started
1. Clone the repository.
2. Configure your GitHub OAuth credentials in `appsettings.json`.
3. Run `dotnet ef database update` to set up your local database.
4. Run the project and start linking your work!

---
*CommitLink - Bridging the gap between code and tasks.*
