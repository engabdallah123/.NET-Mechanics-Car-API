# 🧰 Mechanics Car Maintenance API

A **Car Maintenance Management System API** built with **ASP.NET Core Web API** and **Entity Framework Core**,  
designed to manage all backend operations for a car workshop — including clients, cars, services, orders, workers, invoices, and scheduling.

---

## 📖 Project Overview

This API simulates the backend of a **real-world car maintenance workshop**,  
where **only managers and workers** can access the system to manage daily operations.

It provides full **CRUD (Create, Read, Update, Delete)** operations for every module,  
allowing seamless integration with a frontend app (like Angular) or mobile client.

---

## ⚙️ Key Features

### 👥 Client & Vehicle Management
- Create, update, delete, and fetch client records.
- Register cars and link them to specific clients.
- Retrieve a client’s car history and past maintenance records.

### 🧰 Service Management
- Manage all service types (add, edit, delete).
- Define price, estimated duration, and category for each service.
- Endpoint for listing all available services.

### 🧑‍🔧 Worker Management
- Add and manage workshop employees.
- Assign workers to specific orders.
- Track each worker’s active and completed jobs.

### 🗓️ Station & Scheduling Management
- Four **service stations** available daily.
- Each station has **specific time slots** from **8:00 AM to 11:00 AM**.
- Slots are divided into intervals (e.g., 30 minutes).
- API validates that no new order can be created for **expired slots**.

### 📋 Order Management
- Full CRUD operations for maintenance orders.
- Each order includes:
  - Client & Car reference.
  - Station & Slot.
  - Worker & Service.
  - Current order status (Pending, In Progress, Completed).
- Once completed, the API triggers **invoice creation**.

### 💵 Invoice Management
- Automatically generate invoices when an order is completed.
- Store details such as:
  - Service name
  - Worker
  - Date/time
  - Total price
- Expose endpoints to fetch invoices per client or per day.

---

## ⚙ Tech Stack

| Category | Technology |
|-----------|-------------|
| Framework | ASP.NET Core 9.0 Web API |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Architecture | Repository Pattern + Dependancy inversion |
| Authentication | JWT (JSON Web Token) |
| Language | C# |
| Tools | Visual Studio / Visual Studio Code + GitHub |
| Memory Cache |

---

## 🧠 Business Flow

1. The manager or worker authenticates via JWT login.
2. Client and car data are managed via CRUD APIs.
3. Stations and slots are loaded dynamically.
4. Orders are created with validation for available slots.
5. When the worker marks the order as **Completed**, you can create an invoice.
6. Admin can retrieve invoices, filter orders, and view statistics.

---

🤝 Contact

If you have any feedback, suggestions, or opportunities — feel free to connect with me here on GitHub or on LinkedIn.
www.linkedin.com/in/abdallah-ebrahim-5038272b6

---
#. Configure API
- Make sure the Angular app points to the backend API URL in `environment.ts`:
```ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5299/api'
};
```
## 🌟 Final Words

Mechanics Car Maintenance isn’t just a coding project — it’s a **real-world simulation** of how workshops manage time, workers, and clients efficiently.  
It reflects the power of combining **Angular** and **.NET** to build practical, organized, and scalable systems.

Every line of code in this project was written with the goal of making maintenance management smarter and more efficient 🔧💡

> “Great systems start with small ideas — and grow through passion and precision.”

Thank you for checking out this project 🙏  
If you like it, don’t forget to ⭐ the repository and share your feedback!

**— Developed with ❤️ by Abdallah Ebrahim**


