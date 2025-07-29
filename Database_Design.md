# Payroll Management System - Database Design

## Entity Relationship Diagram (ERD)

This document outlines the database design for the Payroll Management System, following proper ERD principles and UML patterns.

## Entities Overview

### Core Entities
1. **Department** - Organizational units
2. **Employee** - Staff members
3. **Payroll** - Salary calculations and payments
4. **PayrollItem** - Individual payroll components
5. **User** - System users (Identity)
6. **Role** - User roles (Identity)

### Lookup/Reference Entities
1. **PayrollItemType** - Types of payroll items (salary, bonus, deduction, etc.)
2. **EmployeeStatus** - Employee status enumeration
3. **PayrollStatus** - Payroll processing status

## Design Patterns Applied

### 1. Entity Pattern
- Each entity has a primary key (Id)
- Audit fields (CreatedAt, UpdatedAt)
- Soft delete capability where needed

### 2. Aggregate Root Pattern
- Employee is an aggregate root containing payroll data
- Department is an aggregate root for organizational structure

### 3. Value Object Pattern
- Address as value object
- Money/Currency handling for financial fields

### 4. Repository Pattern
- Generic repository interface
- Specific repositories for complex queries

## Relationships and Constraints

### One-to-Many Relationships
- Department → Employee (1:N)
- Employee → Payroll (1:N)
- Payroll → PayrollItem (1:N)
- PayrollItemType → PayrollItem (1:N)

### Many-to-Many Relationships
- User → Role (M:N) - Handled by Identity Framework

## Business Rules and Constraints

### Employee
- Employee code must be unique
- Email must be unique
- Hire date cannot be in the future
- Base salary must be positive

### Payroll
- One payroll record per employee per month
- Gross pay = Base salary + Overtime + Bonus + Allowances
- Net pay = Gross pay - Deductions - Tax deduction
- Cannot modify processed payroll

### Department
- Department code must be unique
- Cannot delete department with active employees

