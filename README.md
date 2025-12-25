# 🍽️ Restaurant Management System (Back-End)

![.NET Core](https://img.shields.io/badge/.NET%20Core-6.0%2B-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Status](https://img.shields.io/badge/Maintained-Yes-brightgreen?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)

> A robust and scalable RESTful API designed to power the operations of a modern restaurant, handling everything from order processing to email notifications.

---

## 📖 Table of Contents
- [Create Context](#-project-overview)
- [Architecture & Tech Stack](#-architecture--tech-stack)
- [Key Features](#-key-features)
- [Database Structure](#-database-structure)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Contributing](#-contributing)

---

## 🔭 Project Overview

**Back_end_QuanLyNhaHang** is the core server-side application for the Restaurant Management ecosystem. It provides a secure API for client applications (Mobile/Web) to interact with the database, ensuring data consistency and efficient business logic processing.

This project focuses on **performance**, **clean architecture**, and **real-time updates** for restaurant staff and customers.

---

## 🏗️ Architecture & Tech Stack

This project is built using the **MVC (Model-View-Controller)** pattern with a focus on Service-oriented architecture.

| Category | Technology | Description |
| :--- | :--- | :--- |
| **Core Framework** | .NET Core | High-performance, cross-platform framework. |
| **Language** | C# | Strongly typed, modern object-oriented language. |
| **Database** | SQL Server | Enterprise-grade relational database management system. |
| **ORM** | Entity Framework Core | Efficient data access and query handling. |
| **Communication** | SMTP / MailKit | Service for sending transactional emails (Bills, Reset Password). |
| **Assets** | Static Files (wwwroot) | Serving menu images and resources directly. |

---

## 🌟 Key Features

Based on the development modules, the system includes:

### 🛒 Order & Booking Management
* **Order Processing:** Create, update, and cancel orders (`DonHang`) in real-time.
* **Booking History:** APIs to retrieve booking history via phone number lookup.
* **Bill Generation:** Automatic calculation of totals and invoice creation.

### 📧 Notification Services
* **Email Integration:** Automated email triggers for:
    * Booking confirmations.
    * Order cancellation alerts.
    * Digital receipts.

### 🍱 Menu & Inventory
* **Dynamic Menu:** Manage food categories and items.
* **Image Hosting:** Backend handling of menu images (`wwwroot/images`) for fast retrieval by frontend apps.

---

## 🗄️ Database Structure

The project utilizes **Microsoft SQL Server**. Database migrations and schema snapshots are managed via SQL scripts.

* **Current Version:** `QuanLyNhaHang_version13.1.sql`
* **Key Tables:** `DonHang` (Orders), `KhachHang` (Customers), `MonAn` (Menu Items).

> **Note:** Always execute the latest `.sql` file found in the root directory to ensure your local database matches the code logic.

---

## 🚀 Getting Started

Follow these steps to set up the environment locally.

### Prerequisites
* [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) & SSMS
* [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code

### Installation

1.  **Clone the Repository**
    ```bash
    git clone [https://github.com/DuyHieu2004/Back_end_QuanLyNhaHang.git](https://github.com/DuyHieu2004/Back_end_QuanLyNhaHang.git)
    cd Back_end_QuanLyNhaHang
    ```

2.  **Setup Database**
    * Open **SQL Server Management Studio (SSMS)**.
    * Execute the script: `QuanLyNhaHang_version13.1.sql`.
    * *This will create the database and seed initial data.*

3.  **Configure Connection**
    * Open `appsettings.Development.json`.
    * Update the connection string:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=QuanLyNhaHang;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
    ```

4.  **Run the API**
    ```bash
    dotnet run
    ```
    The server will start at `https://localhost:7xxx`.

---

## 🔌 API Documentation

This project uses **Swagger** for API specification and testing.

1.  Run the project.
2.  Navigate to: `https://localhost:<port>/swagger/index.html`
3.  You can test all Endpoints (GET, POST, PUT, DELETE) directly from the browser.

---

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

---

## 👤 Author

**DuyHieu2004**
* Github: [@DuyHieu2004](https://github.com/DuyHieu2004)

---

<p align="center">
  <i>Developed with ❤️ for the Restaurant Industry.</i>
</p>
