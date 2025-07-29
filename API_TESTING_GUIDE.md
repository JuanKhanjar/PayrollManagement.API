# API Testing Guide - Payroll Management API

This guide provides comprehensive examples for testing all API endpoints of the Payroll Management system.

## 🚀 Getting Started

### Start the Application
```bash
cd PayrollManagement.API
dotnet run --urls="http://localhost:5000"
```

### Verify Application is Running
```bash
curl http://localhost:5000/health
# Expected Response: Healthy
```

## 🔐 Authentication Testing

### 1. Register a New User
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@company.com",
    "password": "SecurePass@123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": "user-id",
    "email": "john.doe@company.com",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

### 2. Login with Default Admin User
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@payroll.com",
    "password": "Admin@123456"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh-token-here",
    "expiresAt": "2025-07-19T10:00:00Z",
    "user": {
      "id": "admin-id",
      "email": "admin@payroll.com",
      "firstName": "System",
      "lastName": "Administrator",
      "roles": ["Administrator"]
    }
  }
}
```

### 3. Get User Profile
```bash
# Save the access token from login response
TOKEN="your-access-token-here"

curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer $TOKEN"
```

### 4. Refresh Access Token
```bash
curl -X POST http://localhost:5000/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your-refresh-token-here"
  }'
```

### 5. Change Password
```bash
curl -X POST http://localhost:5000/api/auth/change-password \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "Admin@123456",
    "newPassword": "NewSecurePass@123"
  }'
```

### 6. Logout
```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your-refresh-token-here"
  }'
```

## 🏢 Department Management Testing

### 1. Get All Departments
```bash
curl -X GET http://localhost:5000/api/departments \
  -H "Authorization: Bearer $TOKEN"
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Departments retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "Human Resources",
      "code": "HR",
      "description": "Human Resources Department",
      "employeeCount": 0,
      "createdAt": "2025-07-19T08:54:24Z"
    }
  ]
}
```

### 2. Get Department by ID
```bash
curl -X GET http://localhost:5000/api/departments/1 \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Create New Department
```bash
curl -X POST http://localhost:5000/api/departments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Engineering",
    "code": "ENG",
    "description": "Software Engineering Department"
  }'
```

### 4. Update Department
```bash
curl -X PUT http://localhost:5000/api/departments/5 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Software Engineering",
    "code": "ENG",
    "description": "Software Development and Engineering Department"
  }'
```

### 5. Delete Department
```bash
curl -X DELETE http://localhost:5000/api/departments/5 \
  -H "Authorization: Bearer $TOKEN"
```

## 👥 Employee Management Testing

### 1. Get All Employees (with Pagination)
```bash
curl -X GET "http://localhost:5000/api/employees?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### 2. Create New Employee
```bash
curl -X POST http://localhost:5000/api/employees \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeCode": "EMP001",
    "employeeNumber": "E001",
    "firstName": "Alice",
    "lastName": "Johnson",
    "email": "alice.johnson@company.com",
    "phoneNumber": "+1-555-0123",
    "position": "Software Developer",
    "departmentId": 2,
    "baseSalary": 75000.00,
    "hireDate": "2025-01-15T00:00:00Z",
    "dateOfBirth": "1990-05-15T00:00:00Z",
    "address": "123 Main St, City, State 12345"
  }'
```

### 3. Get Employee by ID
```bash
curl -X GET http://localhost:5000/api/employees/1 \
  -H "Authorization: Bearer $TOKEN"
```

### 4. Update Employee
```bash
curl -X PUT http://localhost:5000/api/employees/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeCode": "EMP001",
    "employeeNumber": "E001",
    "firstName": "Alice",
    "lastName": "Johnson",
    "email": "alice.johnson@company.com",
    "phoneNumber": "+1-555-0123",
    "position": "Senior Software Developer",
    "departmentId": 2,
    "baseSalary": 85000.00,
    "hireDate": "2025-01-15T00:00:00Z",
    "dateOfBirth": "1990-05-15T00:00:00Z",
    "address": "123 Main St, City, State 12345"
  }'
```

### 5. Search Employees
```bash
curl -X GET "http://localhost:5000/api/employees/search?searchTerm=Alice&page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### 6. Get Employees by Department
```bash
curl -X GET http://localhost:5000/api/employees/department/2 \
  -H "Authorization: Bearer $TOKEN"
```

### 7. Delete Employee
```bash
curl -X DELETE http://localhost:5000/api/employees/1 \
  -H "Authorization: Bearer $TOKEN"
```

## 💰 Payroll Management Testing

### 1. Get All Payrolls (with Pagination)
```bash
curl -X GET "http://localhost:5000/api/payrolls?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### 2. Create New Payroll
```bash
curl -X POST http://localhost:5000/api/payrolls \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": 1,
    "payPeriodStart": "2025-07-01T00:00:00Z",
    "payPeriodEnd": "2025-07-31T00:00:00Z",
    "payPeriodMonth": 7,
    "payPeriodYear": 2025,
    "baseSalary": 75000.00,
    "overtime": 500.00,
    "bonus": 1000.00,
    "allowances": 200.00,
    "deductions": 300.00,
    "taxDeduction": 1200.00,
    "notes": "July 2025 payroll"
  }'
```

### 3. Get Payroll by ID
```bash
curl -X GET http://localhost:5000/api/payrolls/1 \
  -H "Authorization: Bearer $TOKEN"
```

### 4. Update Payroll
```bash
curl -X PUT http://localhost:5000/api/payrolls/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": 1,
    "payPeriodStart": "2025-07-01T00:00:00Z",
    "payPeriodEnd": "2025-07-31T00:00:00Z",
    "payPeriodMonth": 7,
    "payPeriodYear": 2025,
    "baseSalary": 75000.00,
    "overtime": 750.00,
    "bonus": 1500.00,
    "allowances": 200.00,
    "deductions": 300.00,
    "taxDeduction": 1200.00,
    "notes": "July 2025 payroll - updated"
  }'
```

### 5. Generate Payrolls for All Employees
```bash
curl -X POST http://localhost:5000/api/payrolls/generate \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "payPeriodStart": "2025-08-01T00:00:00Z",
    "payPeriodEnd": "2025-08-31T00:00:00Z",
    "payPeriodMonth": 8,
    "payPeriodYear": 2025
  }'
```

### 6. Process Payroll (Calculate Gross/Net Pay)
```bash
curl -X POST http://localhost:5000/api/payrolls/1/process \
  -H "Authorization: Bearer $TOKEN"
```

### 7. Mark Payroll as Paid
```bash
curl -X POST http://localhost:5000/api/payrolls/1/pay \
  -H "Authorization: Bearer $TOKEN"
```

### 8. Get Payrolls by Status
```bash
curl -X GET "http://localhost:5000/api/payrolls?status=Processed&page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### 9. Get Payrolls by Employee
```bash
curl -X GET "http://localhost:5000/api/payrolls?employeeId=1&page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### 10. Delete Payroll
```bash
curl -X DELETE http://localhost:5000/api/payrolls/1 \
  -H "Authorization: Bearer $TOKEN"
```

## 🔍 Advanced Testing Scenarios

### 1. Complete Employee Lifecycle
```bash
# 1. Create Department
DEPT_RESPONSE=$(curl -s -X POST http://localhost:5000/api/departments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Quality Assurance",
    "code": "QA",
    "description": "Quality Assurance Department"
  }')

# Extract department ID (you'll need to parse JSON)
DEPT_ID=5

# 2. Create Employee
EMP_RESPONSE=$(curl -s -X POST http://localhost:5000/api/employees \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeCode": "QA001",
    "employeeNumber": "Q001",
    "firstName": "Bob",
    "lastName": "Smith",
    "email": "bob.smith@company.com",
    "position": "QA Engineer",
    "departmentId": '$DEPT_ID',
    "baseSalary": 65000.00,
    "hireDate": "2025-07-19T00:00:00Z",
    "dateOfBirth": "1988-03-20T00:00:00Z"
  }')

# Extract employee ID
EMP_ID=2

# 3. Create Payroll for Employee
curl -X POST http://localhost:5000/api/payrolls \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": '$EMP_ID',
    "payPeriodStart": "2025-07-01T00:00:00Z",
    "payPeriodEnd": "2025-07-31T00:00:00Z",
    "payPeriodMonth": 7,
    "payPeriodYear": 2025,
    "baseSalary": 65000.00,
    "overtime": 0.00,
    "bonus": 500.00,
    "allowances": 100.00,
    "deductions": 200.00,
    "taxDeduction": 800.00
  }'

# 4. Process the Payroll
curl -X POST http://localhost:5000/api/payrolls/2/process \
  -H "Authorization: Bearer $TOKEN"

# 5. Mark as Paid
curl -X POST http://localhost:5000/api/payrolls/2/pay \
  -H "Authorization: Bearer $TOKEN"
```

### 2. Bulk Payroll Processing
```bash
# Generate payrolls for all employees
curl -X POST http://localhost:5000/api/payrolls/generate \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "payPeriodStart": "2025-08-01T00:00:00Z",
    "payPeriodEnd": "2025-08-31T00:00:00Z",
    "payPeriodMonth": 8,
    "payPeriodYear": 2025
  }'

# Get all draft payrolls
curl -X GET "http://localhost:5000/api/payrolls?status=Draft" \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Error Handling Testing

#### Test Invalid Authentication
```bash
curl -X GET http://localhost:5000/api/departments \
  -H "Authorization: Bearer invalid-token"
# Expected: 401 Unauthorized
```

#### Test Invalid Data
```bash
curl -X POST http://localhost:5000/api/departments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "code": "TOOLONG",
    "description": ""
  }'
# Expected: 400 Bad Request with validation errors
```

#### Test Duplicate Data
```bash
# Try to create department with existing code
curl -X POST http://localhost:5000/api/departments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Duplicate HR",
    "code": "HR",
    "description": "This should fail"
  }'
# Expected: 400 Bad Request - duplicate code
```

#### Test Not Found
```bash
curl -X GET http://localhost:5000/api/departments/999 \
  -H "Authorization: Bearer $TOKEN"
# Expected: 404 Not Found
```

## 📊 Performance Testing

### 1. Pagination Testing
```bash
# Test large page sizes
curl -X GET "http://localhost:5000/api/employees?page=1&pageSize=100" \
  -H "Authorization: Bearer $TOKEN"

# Test invalid pagination
curl -X GET "http://localhost:5000/api/employees?page=0&pageSize=-1" \
  -H "Authorization: Bearer $TOKEN"
```

### 2. Search Performance
```bash
# Test search with various terms
curl -X GET "http://localhost:5000/api/employees/search?searchTerm=john" \
  -H "Authorization: Bearer $TOKEN"

curl -X GET "http://localhost:5000/api/employees/search?searchTerm=@company.com" \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Concurrent Requests
```bash
# Test multiple simultaneous requests
for i in {1..10}; do
  curl -X GET http://localhost:5000/api/departments \
    -H "Authorization: Bearer $TOKEN" &
done
wait
```

## 🔧 Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check if token is valid and not expired
   - Ensure Bearer prefix in Authorization header

2. **400 Bad Request**
   - Validate JSON format
   - Check required fields
   - Verify data types and constraints

3. **404 Not Found**
   - Verify endpoint URL
   - Check if resource exists
   - Ensure correct HTTP method

4. **500 Internal Server Error**
   - Check application logs
   - Verify database connection
   - Check for missing dependencies

### Debugging Tips

1. **Enable Detailed Logging**
   ```bash
   # Check logs in the application output
   # Look for structured log entries
   ```

2. **Test Health Endpoint**
   ```bash
   curl http://localhost:5000/health
   ```

3. **Verify Database State**
   ```bash
   # Check if database file exists
   ls -la payroll.db*
   ```

## 📝 Test Data Setup

### Create Complete Test Dataset
```bash
# Set your token
TOKEN="your-access-token-here"

# Create departments
curl -X POST http://localhost:5000/api/departments -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"name": "Engineering", "code": "ENG", "description": "Engineering Department"}'
curl -X POST http://localhost:5000/api/departments -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"name": "Sales", "code": "SALES", "description": "Sales Department"}'
curl -X POST http://localhost:5000/api/departments -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"name": "Marketing", "code": "MKT", "description": "Marketing Department"}'

# Create employees
curl -X POST http://localhost:5000/api/employees -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"employeeCode": "ENG001", "employeeNumber": "E001", "firstName": "John", "lastName": "Doe", "email": "john.doe@company.com", "position": "Senior Developer", "departmentId": 5, "baseSalary": 90000, "hireDate": "2025-01-01T00:00:00Z", "dateOfBirth": "1985-06-15T00:00:00Z"}'
curl -X POST http://localhost:5000/api/employees -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"employeeCode": "SALES001", "employeeNumber": "S001", "firstName": "Jane", "lastName": "Smith", "email": "jane.smith@company.com", "position": "Sales Manager", "departmentId": 6, "baseSalary": 75000, "hireDate": "2025-02-01T00:00:00Z", "dateOfBirth": "1988-09-20T00:00:00Z"}'
curl -X POST http://localhost:5000/api/employees -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"employeeCode": "MKT001", "employeeNumber": "M001", "firstName": "Mike", "lastName": "Johnson", "email": "mike.johnson@company.com", "position": "Marketing Specialist", "departmentId": 7, "baseSalary": 60000, "hireDate": "2025-03-01T00:00:00Z", "dateOfBirth": "1990-12-10T00:00:00Z"}'

# Generate payrolls for all employees
curl -X POST http://localhost:5000/api/payrolls/generate -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"payPeriodStart": "2025-07-01T00:00:00Z", "payPeriodEnd": "2025-07-31T00:00:00Z", "payPeriodMonth": 7, "payPeriodYear": 2025}'
```

This comprehensive testing guide covers all aspects of the Payroll Management API, from basic CRUD operations to complex business workflows and error scenarios.

