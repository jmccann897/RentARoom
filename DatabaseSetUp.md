# RentARoom Azure Sql Database Set Up

This project includes some seed data for helping set up the project whether running locally or deploying to cloud (Azure).

## Prerequisites

1. **SSMS - SQL Server Management Studio**

* SSMS is the GUI tool used to manage your SQL Server database.
* Follow instructions to download SSMS: https://learn.microsoft.com/en-us/ssms/download-sql-server-management-studio-ssms


2. **SQL Express**

* SQL Express is a free, lightweight edition of Microsoft SQL Server, ideal for local development.
* Follow instructions on how to install SQL Express: https://www.microsoft.com/en-gb/sql-server/sql-server-downloads

```npm install -g azurite ```

3. **Azure Subscription**
If you want to deploy RentARoom to the cloud:
* Sign up or log in at the Azure Portal: https://azure.microsoft.com/en-us/get-started/azure-portal
* You'll need permission to create resource groups and SQL databases (Free Student or Pay-as-you-go accounts work).

---

## Running SSMS
  
### 1. **Connect to Local SQL Server Express**
Once SSMS is installed:

1. Launch **SQL Server Management Studio**.
2. In the **Connect to Server** dialog:
   - **Server type**: `Database Engine`
   - **Server name**: `.\SQLEXPRESS` (this is the default instance name for SQL Express)
   - **Authentication**: `Windows Authentication` (recommended for local development)

> ✅ Tip: If `.\SQLEXPRESS` doesn't work, try `localhost\SQLEXPRESS` or simply `(local)`.

3. Click **Connect**.
You should now be connected to your local SQL Express instance.

### 2. **Connect to Azure SQL Database (Optional)**
If you're using an Azure-hosted database:

1. In SSMS, choose **Connect → Database Engine**.
2. Set the **Server name** to your Azure SQL server (e.g. `your-db-server.database.windows.net`).
3. Set **Authentication** to `SQL Server Authentication` and provide your username and password (same as the set admin and password on Azure for the Azure SQL Server and Database).
4. Click **Options** → on the **Connection Properties** tab, enter the database name if known.
5. Click **Connect**.

---

## Important Notes

  ### 🔐 Connection Strings

If you're using **Azure Key Vault**, nested configurations need flattening:

| ASP.NET Format                   | Azure Key Vault Equivalent        |
|----------------------------------|-----------------------------------|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings--DefaultConnection` |

Example `UserSecrets` format:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YourConnectionString"
  },
  "AzureBlobStorage": {
    "ConnectionString": "YourAzureBlobStorageConnectionString",
    "ContainerName": "YourAzureBlobStorageContainerName"
  }
}
```
---

### 🌱 Seed Data

* Seed logic is defined in :
    - `RentARoom.DataAccess/Data/ApplicationDbContext.cs` method `SeedDataAsync`
    - Triggered during app startup from `RentARoom/Program.cs`

> ℹ️ The app checks whether the database exists before seeding.
---

## Helpful SQL Scripts
1. **Update Seeded Users to have unique names**
Alter Name fields to suit your requirements
```sql
UPDATE AspNetUsers SET [Name] = 'ExampleUser' WHERE UserName = 'user@gmail.com'
UPDATE AspNetUsers SET [Name] = 'TestAgent' WHERE UserName = 'agent@gmail.com'
UPDATE AspNetUsers SET [Name] = 'AdminName' WHERE UserName = 'admin@gmail.com'
```

2. **Transfer Admin properties to an Agent user**
Update the ApplicationUserIds fields to match your database
```sql
UPDATE [Property]
SET ApplicationUserId = 'applicationUserId of new agent'
WHERE ApplicationUserId = 'applicationUserId of seeded admin';
```