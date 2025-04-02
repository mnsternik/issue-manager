# IssueManager 🚀

A professional issue tracking system with role-based access control, designed for efficient team collaboration and request management. Deployed as a Docker container on Azure App Service.

**Live Demo**: [https://issuemanager.azurewebsites.net/](https://issuemanager.azurewebsites.net/)  
*(Admin Credentials: demo.admin@issuemanager.com / DemoAdmin123!)*

![Dashboard Preview] (https://github.com/user-attachments/assets/8d9fd99e-b825-4f5c-b4fb-46b4473565f4)

! [Request Details Preview](https://github.com/user-attachments/assets/78864040-b5ec-4393-8113-dfb6cfbdf981)

## Key Features ✨

-**Role - Based Access Control * *(Admin / User)
- **Full Request Lifecycle Management * *:
  -Create / Edit / Assign requests
  - File attachments(PDF, Images, Docs)
  - Team collaboration with threaded responses
- **Advanced Search * *with 13 filters
- **Admin Dashboard * *:
  -User / Team / Category management
  - Password & role administration
- **Concurrency Control * *with EF Core

## Technology Stack 🛠️

| Category | Technologies |
| ------------------------| ----------------------------------------------------------------------------|
| **Backend * *            | ASP.NET Core MVC 8, Entity Framework Core 8, AutoMapper |
| **Frontend * *           | Razor Pages, Bootstrap 5, JavaScript ES6 |
| **Authentication * *     | ASP.NET Identity, Role - based Authorization |
| **Database * *           | SQL Server, Azure SQL Database |
| **DevOps * *             | Docker, Azure App Service |
| **Testing * *            | xUnit, Moq |

## Project Structure 📂
IssueManager /
├── Controllers / # MVC Controllers
├── Services / # Business logic layer
├── Models / # Domain models and ViewModels
├── Data / # EF Core DbContext and migrations
├── Utilities / # Helpers and extensions
├── wwwroot / # Static files
└── Dockerfile # Docker container configuration

## Getting Started 🚀

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- SQL Server 2019 +

### Local Installation
```bash
# Clone repository
git clone https://github.com/mnsternik/issue-manager.git

# Build and run with Docker
docker - compose up--build

# Apply database migrations
docker exec - it issuemanager - web - 1 dotnet ef database update
```

# License 📄
Distributed under the MIT License.See LICENSE for more information.