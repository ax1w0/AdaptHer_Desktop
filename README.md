# AdaptHER — Employee Onboarding Management System

WPF desktop application for managing new employee onboarding in an IT company.

## Overview

**AdaptHER** is a desktop application developed for **HR specialists** to streamline the onboarding process for new employees. The system allows creating training module development requests, building onboarding programs, and analyzing training results through charts and reports.

## Technologies

- **Framework:** .NET Framework 4.7.2
- **UI Platform:** Windows Presentation Foundation (WPF)
- **Language:** C#
- **Database:** MS SQL Server
- **ORM:** Entity Framework
- **Tools:** Microsoft Visual Studio 2019, SQL Server Management Studio

## Key Features

### Onboarding Modules
- View list of module development tasks
- Create new module development requests
- Edit module statuses
- Search and filter modules by department or name
- Assign developers and approvers

### Onboarding Programs
- Create onboarding programs for new employees
- Select employee, department, position
- Assign modules and mentors
- Generate `.xlsx` program reports
- Save reports locally

### Analytics
- View bar charts of program popularity
- View quality performance charts
- Generate PDF reports with charts and tables
- Export data to `.pdf` or `.csv` formats
- Filter analytics by department or role

## Target Users

| Role | Description |
|------|-------------|
| **HR Specialist** | Main user — manages modules, programs, and analytics |
| **HR Manager** | Plans onboarding programs |
| **Developer** | Creates adaptation modules |
| **Approver** | Approves new modules |
| **Database Administrator** | Manages database |

## Database Schema

### Core Entities

| Table | Description |
|-------|-------------|
| **Module** | Adaptation modules (code name, name, dates, status) |
| **Program** | Onboarding programs |
| **Event** | Training events (name, description, duration, required) |
| **Person** | Employee personal data (name, birthday) |
| **Role** | Positions in the company |
| **Department** | Company departments |
| **Status** | Module statuses (New, In Progress, Completed, etc.) |
| **Analytics** | Training results (dates, exercise counts, employment status) |

### Junction Tables

| Table | Purpose |
|-------|---------|
| **Agreed** | Module — Person (approvers) |
| **Developer** | Module — Person (developers) |
| **ModuleEvent** | Module — Event |
| **ModulePosition** | Module — Role |
| **ModuleProgram** | Module — Program |
| **RoleDepartment** | Role — Department |

## Installation

### Prerequisites
- Windows 10 (version 23 or later)
- .NET Framework 4.7.2
- SQL Server 2012 or higher
- Visual Studio 2019/2022 (for development)
