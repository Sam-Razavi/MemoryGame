# 🎮 MemoryGame – Databasteknik och Webbaserade System

[![CI](https://github.com/Sam-Razavi/MemoryGame/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/Sam-Razavi/MemoryGame/actions/workflows/ci.yml)

A turn-based multiplayer **Memory Game** built with **ASP.NET Core MVC** and **SQL Server** as part of the course  
📘 *Databasteknik och webbaserade system* at IT-Högskolan Sverige AB.

---

## 🏗️ Project Overview
**MemoryGame** is a web-based multiplayer game where two players take turns flipping cards to find matching pairs.  
The project demonstrates the use of relational databases, MVC architecture, CRUD operations, and session handling in a full-stack ASP.NET web application.

---

## 🎓 My Role & What I Built

This was an **individual school project** that I designed and implemented end-to-end. Everything in this repository was written by me.

**What I built:**
- **Database schema** – designed from scratch: six normalized tables with composite foreign keys that enforce referential integrity at the DB level (e.g. a `Move`'s tiles must belong to the same `Game` row).
- **Custom Repository layer using raw ADO.NET** – no Entity Framework; all queries are hand-written parameterized SQL, which was a course requirement to demonstrate direct SQL knowledge.
- **Session-based authentication** – including PBKDF2-SHA256 password hashing with a random salt (`Security/PasswordHasher.cs`), without relying on ASP.NET Identity.
- **Real-time turn polling** – the game board auto-refreshes via AJAX so each player sees the opponent's last move without a full page reload.
- **Lobby system** – Player A creates a game, Player B joins, and the game transitions automatically through `Waiting → InProgress → Completed`.
- **Card Admin CRUD interface** – full create/read/update/delete for the card catalogue.
- **Azure deployment** via Visual Studio's publish wizard.

**What I learned:**
- Writing safe, parameterized ADO.NET queries without an ORM safety net.
- Designing a turn-based state machine enforced by a SQL `CHECK` constraint.
- Managing sessions in ASP.NET Core without the Identity middleware stack.
- Structuring an MVC application with Dependency Injection for testability.

---

## ✨ Features
✅ Two-player **turn-based gameplay**  
✅ **Login system** with sessions  
✅ **Lobby** where users can create, join, or delete games  
✅ **Real-time updates** through AJAX polling  
✅ **Card Admin CRUD interface** (Create / Read / Update / Delete)  
✅ **SQL database integration** for all game entities  
✅ **Clean Bootstrap 5 interface**  
✅ **Session-based authentication**

---

## 🧱 Tech Stack

| Layer | Technology |
|-------|-------------|
| **Frontend** | HTML5, CSS3, Bootstrap 5, Razor Views |
| **Backend** | ASP.NET Core MVC (C#) |
| **Database** | Microsoft SQL Server |
| **Data Access** | Custom Repository Layer (ADO.NET) |
| **Authentication** | Session-based login |
| **Version Control** | Git & GitHub |

---

## 🗂️ Architecture

```
Browser (Razor Views + jQuery AJAX)
           │
           ▼
   ┌───────────────────────────────────────┐
   │            Controllers                │
   │  AccountController · GameController   │
   │  CardAdminController · HomeController │
   └────────────────┬──────────────────────┘
                    │  constructor-injected interfaces
                    ▼
   ┌───────────────────────────────────────┐
   │         Repositories (ADO.NET)        │
   │  UserRepository  · GameRepository     │
   │  CardRepository  · TileRepository     │
   │  MoveRepository                       │
   └────────────────┬──────────────────────┘
                    │  SqlConnection via IDbConnectionFactory
                    ▼
   ┌───────────────────────────────────────┐
   │             SQL Server                │
   │  Tables: User · Game · GamePlayer     │
   │          Card · Tile · Move           │
   └───────────────────────────────────────┘
```

Session cookies hold the logged-in `UserID` and `Username`. No Entity Framework — all queries are hand-written parameterized SQL.

---

## 📂 Project Structure
<img width="532" height="785" alt="image" src="https://github.com/user-attachments/assets/e00c15e5-1bed-4da8-8aa0-2cb460fbfb0c" />

---

## 🚀 How to Run Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** (any edition), **SQL Server Express**, or **LocalDB** (ships with Visual Studio)
- SQL Server Management Studio (SSMS) or Azure Data Studio *(optional)*

### 1. Clone the repository
```bash
git clone https://github.com/sam-razavi/memorygame.git
cd memorygame
```

### 2. Create and populate the database
Open your SQL client and run:
```sql
CREATE DATABASE MemoryGameDb;
```
Then open `MemoryGame/Database/01_CreateSchema.sql` and execute it against `MemoryGameDb`.  
The script drops and recreates all tables, so it is safe to re-run.

### 3. Configure the connection string
Copy the example file:
```bash
cp MemoryGame/appsettings.example.json MemoryGame/appsettings.json
```
Then edit `appsettings.json` and replace the `MemoryGameDb` value with your connection string:

| Scenario | Connection string |
|----------|-------------------|
| **LocalDB** (Visual Studio default) | `Server=(localdb)\\MSSQLLocalDB;Database=MemoryGameDb;Trusted_Connection=True;MultipleActiveResultSets=true` |
| **SQL Server Express** | `Server=.\\SQLEXPRESS;Database=MemoryGameDb;Trusted_Connection=True;MultipleActiveResultSets=true` |
| **Named instance** | `Server=YOUR_PC\\INSTANCE;Database=MemoryGameDb;Trusted_Connection=True;MultipleActiveResultSets=true` |
| **SQL auth** | `Server=YOUR_SERVER;Database=MemoryGameDb;User Id=sa;Password=YOUR_PASSWORD;MultipleActiveResultSets=true` |

> **Note:** `appsettings.json` is listed in `.gitignore` to keep credentials out of source control.  
> `appsettings.example.json` shows the required keys without real values.

### 4. Run the application
```bash
cd MemoryGame
dotnet run
```
The terminal prints the local URL (e.g. `http://localhost:5013`).  
Open it in your browser, register two accounts (use a second browser window or incognito), and start a game.

### 5. Run the tests
```bash
cd MemoryGame.Tests
dotnet test
```

---

## 🧪 Tests

The `MemoryGame.Tests` project contains unit tests that run **without a database connection**.

| Test class | What it covers |
|------------|----------------|
| `PasswordHasherTests` | Hash format, verify correct password, reject wrong password, reject malformed stored string, unique salts per call |
| `GameModelTests` | Default `Status` value, nullable defaults, property assignment for `Game`, `Card`, and `Tile` |

```bash
dotnet test MemoryGame.Tests
```

These tests cover the pure-logic classes (`PasswordHasher`, model POCOs) that have no external dependencies. Integration tests against a live database are a natural next step.

---

## 🕹️ Gameplay Flow

1. Player A creates a new game in the lobby.
2. Player B joins the same game.
3. The game starts automatically; players alternate flipping two tiles per turn.
4. Matching pairs score a point; a miss passes the turn to the opponent.
5. When all tiles are matched, the system determines the winner automatically.

---

## 🖼️ Screenshots
<img width="1134" height="710" alt="image" src="https://github.com/user-attachments/assets/af75c6db-cae5-493f-948b-349cf5943e87" />
<img width="1092" height="1005" alt="image" src="https://github.com/user-attachments/assets/3cd9cf71-9317-444b-897c-2e17e0774cca" />
<img width="1027" height="1066" alt="image" src="https://github.com/user-attachments/assets/38803ca6-9f54-4209-8caf-fd221eb7daf6" />
<img width="1079" height="1081" alt="image" src="https://github.com/user-attachments/assets/88c7b8bd-6d95-4992-96ae-f12245ee4355" />

---

## Deployed on
<https://memorygame-sam-b4dbbha5anguhxbt.germanywestcentral-01.azurewebsites.net/>
