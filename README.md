# 🎮 MemoryGame – Databasteknik och Webbaserade System

A turn-based multiplayer **Memory Game** built with **ASP.NET Core MVC** and **SQL Server** as part of the course  
📘 *Databasteknik och webbaserade system* at IT-Högskolan Sverige AB.

---

## 🏗️ Project Overview
**MemoryGame** is a web-based multiplayer game where two players take turns flipping cards to find matching pairs.  
The project demonstrates the use of relational databases, MVC architecture, CRUD operations, and session handling in a full-stack ASP.NET web application.

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

## 📂 Project Structure
<img width="532" height="785" alt="image" src="https://github.com/user-attachments/assets/e00c15e5-1bed-4da8-8aa0-2cb460fbfb0c" />

## ✨ Create the database
Run the SQL script named 01-createSchema.sql in SQL Server Management Studio (SSMS) or from Visual Studio’s SQL editor to create all tables and constraints.

## 🕹️ Gameplay Flow
Player A creates a new game in the lobby.

Player B joins the same game.

The game starts automatically and each player takes turns flipping two tiles.

Matching pairs score points; if not, the turn passes to the opponent.

When all tiles are matched, the system automatically determines the winner.

## 🖼️ Screenshots
<img width="1134" height="710" alt="image" src="https://github.com/user-attachments/assets/af75c6db-cae5-493f-948b-349cf5943e87" />
<img width="1092" height="1005" alt="image" src="https://github.com/user-attachments/assets/3cd9cf71-9317-444b-897c-2e17e0774cca" />
<img width="1027" height="1066" alt="image" src="https://github.com/user-attachments/assets/38803ca6-9f54-4209-8caf-fd221eb7daf6" />
<img width="1079" height="1081" alt="image" src="https://github.com/user-attachments/assets/88c7b8bd-6d95-4992-96ae-f12245ee4355" />





