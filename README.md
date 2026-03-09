# Online Query Management System

## Overview
A web application built with ASP.NET Core MVC using Repository Pattern 
to manage user queries and employee responses.

## Tech Stack
- ASP.NET Core MVC
- PostgreSQL
- Repository Pattern
- Bootstrap
- Kendo UI

## Features
Core Functionalities:
1. User Management:
○ Registration: Users can register with basic details (companyname, email,
password).
○ Login: Secure authentication to access the system.
2. Query Management:
○ Create Query: Registered users can submit queries with details like:
■ Title
■ Description
■ Priority (Low, Medium, High)
○ Query Status: Queries can have statuses: Open, In Progress, Solved.
3. Dashboard (Admin and Employee View):
○ Query Statistics:
■ Total queries submitted all and today.
■ Number of solved queries all and today.
■ Number of pending queries all and today.
○ Employee Efficiency Tracking:
■ Number of queries resolved by each employee.

Roles:
1. Admin:
○ Manages users and queries.
○ Views dashboard with all statistics.
2. Employee:
○ Views unsolved queries.
○ Updates query status (from Open to Solved and submit comments).
○ Tracks personal performance metrics.
3. User:
○ Submits, edit, delete(only when unsolved) and tracks their own queries.
○ Checks the status of their submitted queries.
