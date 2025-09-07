# Booking Appointment API

A backend API system for managing customer appointments and token issuance. Built as part of a technical assessment for Backend Developer positions.

## ✨ Features

- Customer appointment booking
- Automatic token number generation
- Daily appointment queue viewing
- Holiday management
- Maximum daily appointment limits
- Automatic rollover to next day when quota is full

## 🛠 Technology Stack

- .NET 8 - Main framework
- SQL Server - Database system
- Swagger/OpenAPI - API documentation


## 🚀 Installation & Setup

### 1. Clone the Repository
- git clone https://github.com/rizkisundara/booking-appointment-api.git

### 2. Database Setup
- Execute the SQL script available in the Database/Scripts folder to create the database and tables.

### 3. Configure Connection String
- Edit appsettings.json or appsettings.Development.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DB_Booking;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  }
}
```

### 4. Restore NuGet Packages
- dotnet restore
