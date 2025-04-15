# 🛠 RentARoom — Setup Instructions

This guide will help you set up **RentARoom** on your local machine for development and testing.

---

## 📦 Prerequisites

Before you clone and run the project, ensure the following software is installed:

### ✔️ Required Software

| Tool | Purpose | Download Link |
|------|---------|---------------|
| [Git](https://git-scm.com/downloads) | Clone the repository |
| [Visual Studio 2022+](https://visualstudio.microsoft.com/downloads/) | Primary IDE for .NET development |
| [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) | Local database |
| [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) | Manage your SQL Express DB |
| [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio) | Local Azure Blob Storage emulator |
| [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) | Required to build and run the app |
| [Node.js (LTS)](https://nodejs.org/) | For managing front-end dependencies (optional) |

---

## 🧬 Clone the Repository

```cmd
git clone https://github.com/jmccann897/RentARoom.git
cd RentARoom
```
## ⚙️ Run the Application

### Option 1: Visual Studio (Recommended)

1. Open `RentARoom.sln` in Visual Studio  
2. Set **RentARoom** as the startup project  
3. Press `F5` to run the project

### Option 2: Visual Studio Code

1. Open the root folder in VS Code  
2. Add a `launch.json` under `.vscode/` like below:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/RentARoom/bin/Debug/net8.0/RentARoom.dll",
      "args": [],
      "cwd": "${workspaceFolder}/RentARoom",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/RentARoom/Views"
      }
    }
  ]
}
```
3. Launch with `F5`

---

## 🧷 Setup SQL Server Express Database

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to: `(localdb)\MSSQLLocalDB`
3. Build the solution in Visual Studio to ensure migrations are available.
4. Run one of the following commands in the terminal (depends in in VS Code or Visual Studio which) to apply Entity Framework Core migrations:
VS Code Dotnet command (Useful link - https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
```bash
cd RentARoom.DataAccess
dotnet ef database update
```
Visual Studio NuGet manager command (Useful link - https://learn.microsoft.com/en-us/ef/core/cli/powershell)
```powershell
cd RentARoom.DataAccess
Update Database
```


> ✅ Tip: Make sure your `appsettings.Development.json` is correctly pointing to the local SQL Server instance.
Example:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RentARoomDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
---
## 🔐 Managing Secrets Locally with User Secrets

To keep sensitive data like API keys, connection strings, and access tokens out of source control, **RentARoom** uses the [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) feature during local development.

---

### 📦 What Are User Secrets?

User Secrets store configuration values in a JSON file outside your project directory, scoped to your user profile, and are not included in version control. They're ideal for local development.

---

### 🚀 How to Use User Secrets in RentARoom

1. Open the `RentARoom` project in Visual Studio or VS Code.

2. Initialize user secrets for the project:

```bash
dotnet user-secrets init
```
This adds a UserSecretsId to the .csproj file (if not already present):

```xml
<UserSecretsId>your-guid-here</UserSecretsId>
```
3. Add secrets by updating the file which is accessible by right-clicking the RentARoom.csproj file within the web app project. 
4. Access these values in your appsettings.json or Startup.cs via the IConfiguration service:
```chsarp
var apiKey = Configuration["OpenRouteService:ApiKey"];
```

### ✅ Tips

- Do **not** commit secrets to Git.
- User secrets are only available in the `Development` environment.
- Use environment variables or secure config stores (like Azure Key Vault) in production.

---

## 🛠️ What You’ll Need to Run RentARoom

To run **RentARoom** locally or in your own environment, you will need the following services and credentials:

### 1. **Database (SQL Server Express)**

You will need a local instance of SQL Server Express to store the RentARoom application data. You can either use **SQL Server Express** or **SQL Server LocalDB**.

- **Connection String**: This will be used in the application to connect to the database.
- Example Connection String:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RentARoomDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
  ```

### 2. **OpenRouteService API Key**

RentARoom uses the **OpenRouteService** API to calculate travel times between properties and user-defined locations (like campuses, stations, etc.).

- **Sign up** for an API key at [OpenRouteService](https://openrouteservice.org/sign-up/).
- Once you have your API key, store it in the user secrets or `appsettings.Development.json` file:
  ```json
  "OpenRouteService": {
    "ApiKey": "your-api-key-here"
  }
  ```

### 3. **Azure Blob Storage Connection String**

RentARoom uses **Azure Blob Storage** to store images uploaded for properties. You will need an **Azure Storage Account** and a **Blob Storage Container** to store the images.

- **Connection String**: Retrieve this from the Azure portal.
- **Container Name**: Create a container in Azure Blob Storage to store the images.
- Example configuration for user secrets:
  ```json
  "AzureStorage": {
    "ConnectionString": "your-azure-connection-string-here",
    "ContainerName": "your-container-name-here"
  }
  ```
