# RentARoom 🏠

**RentARoom** is a MSc in Software Development project submitted for module CSC 7058: Individual Software Development Project.

It is a modern web application designed for **student renters** to find rental accommodation. It includes the ability to add Points of Interest allowing **travel time calculation** from Properties you are interested in. Prospective renters can contact Landlords/Agents via supplied phone numbers and emails or use the **in app chat system**.


---

## 🎯 Objective

The primary goal of RentARoom is to offer a more meaningful and location-aware rental search experience. This app enables users to:

- **Filter properties** by travel time to custom Points of Interest (e.g. university, workplace)
- **Interact via built-in chat** with real-time messaging and notifications
- **View property maps** and optional LPS land sale overlays
- **Track property popularity** via view counts as a Landlord or Agent. 

---

## 🧱 Tech Stack & Frameworks

This project is built using modern Microsoft and open-source technologies:

- **ASP.NET Core** — Web framework
- **C# (.NET 8)** — Backend logic
- **JavaScript (ES6 Modules)** — Frontend interactivity
- **SignalR** — Real-time messaging
- **Entity Framework Core** — Data access layer
- **SQL Express** — Primary database
- **Leaflet.js** — Interactive map
- **OpenStreetMap** - Map tiles
- **OpenRouteServiceAPI** - Travel time calculations
- **Azure Blob Storage** — Property image hosting
- **DataTables** — Dynamic table components for listings and admin areas

---

## 📁 Key Areas

| Module | Description |
|--------|-------------|
| `RentARoom` | Main web application with UI, Hubs, and routing logic |
| `RentARoom.Models` | Shared DTOs, ViewModels, and Entity Models |
| `RentARoom.DataAccess` | Repositories, DbContext, Services |
| `RentARoom.Services` | Business logic layer |
| `RentARoom.Tests` | Unit and Integration test cases | 
| `RentARoom.Utility` | Static helpers and constants |
| `wwwroot` | Static content including JS modules and CSS |
| `Hubs` | SignalR Hubs for Chat and Notifications |

---

## 📚 Documentation

### 🛠 [Setup Instructions](https://github.com/jmccann897/RentARoom/blob/main/Setup.md)

A full step-by-step guide to setting up the application locally.

### 📦 [Database Instructions](https://github.com/jmccann897/RentARoom/blob/main/DatabaseSetUp.md)

Additional database information including Azure integration, seed data and connection strings.

### 🧪 [Test Instructions](https://github.com/jmccann897/RentARoom/blob/main/IntegrationTestSetUp.md)

Guidance on how to run the integration test for RentARoom.

---

## 🔒 Access & Auth

- Basic functionality (property search & filters) is accessible without login
- Registration enables chat and property management features
- Role-based access control: **Admin**, **Agent (Landlord)**, **User (tenant)**
- ASP .Net Identity used

---

## 👤 Author

Built by [Josh McCann](https://github.com/jmccann897)

