# Hotel Scheduler

Hotel Scheduler is a desktop Property Management System (PMS) built using .NET 10 MAUI, Entity Framework Core, and SQLite. Designed for hotel front-desk staff and property managers, it provides visual reservation management, guest tracking, room inventory administration, and financial reporting in a standalone desktop experience.

## Key Features

- **Visual Calendar Timeline:** Interactive grid displaying room availability, check-ins, check-outs, and color-coded reservation statuses over a configurable date range.
    
- **Reservation and Line-Item Billing:** Manage multi-night stays with dynamic line-item charge tracking (e.g., Breakfast, Extra Beds, Pets) and status handling.
    
- **Room Inventory Administration:** Inline CRUD management for room units, room types, daily rates, occupant capacities, and maintenance states without popup overlays.
    
- **Guest Directory:** Centralized guest database with stay histories, contact details, and instant booking assignment.
    
- **Financial Reporting and CSV Export:** Date-filtered financial metrics, line-item revenue tracking, and structured CSV data export.
    
- **Role-Based User Management:** Admin and Staff user roles. The first registered account on a fresh database is automatically granted the **Admin** role with exclusive access to the User Management panel.
    
- **Data Protection and Backup:** Native one-click database backup and restoration for the local SQLite database file.
    

## System Architecture and Technologies

- **Framework:** .NET 10 MAUI (Desktop Target)
    
- **Design Pattern:** MVVM (Model-View-ViewModel) with C# 12 Primary Constructors
    
- **Persistence Layer:** Entity Framework Core with SQLite
    
- **UI Utilities:** .NET MAUI Community Toolkit (Popups, FileSaver, Converters)
    
- **Security:** Local User Authentication using BCrypt password hashing
    

## Installation and Execution

### Standalone Executable (Windows)

No prior .NET runtime installation is required to run pre-compiled releases:

1. Navigate to the **Releases** section of the repository.
    
2. Download `HotelScheduler-v1.0.0-win-x64.zip`.
    
3. Extract the contents to a local directory.
    
4. Execute `hotel scheduler.exe`.
    

### Initial Application Setup

1. Launch the application for the first time.
    
2. Register a new account on the registration screen.
    
3. **Note:** The **first user account** registered on a fresh database automatically receives **Admin** privileges. All subsequent user registrations default to **Staff** privileges unless upgraded by an Admin.
    

### Building From Source

#### Prerequisites

- .NET 10 SDK
    
- Visual Studio 2022 or JetBrains Rider with .NET MAUI workload installed
    

#### Build Commands

```
# Clone the repository
git clone https://github.com/yourusername/hotel-scheduler.git
cd hotel-scheduler

# Restore dependencies
dotnet restore

# Build and execute on Windows
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

## License

Copyright (c) 2026. All Rights Reserved.

This software and associated documentation files are proprietary and confidential. Unauthorized copying, modification, distribution, or commercial sublicensing of this software via any medium is strictly prohibited without prior written permission from the copyright owner.
