# Payroll Management API

A comprehensive ASP.NET Core Web API application implementing a complete payroll management system following the layered architecture pattern. This application demonstrates enterprise-level software development practices with proper separation of concerns, security, and scalability.

## 🏗️ Architecture Overview

This application follows the **Layered Architecture Pattern** as demonstrated in the Web API guide, implementing:

### 1. **Presentation Layer** (`/Presentation`)
- **Controllers**: RESTful API endpoints for all business operations
- **Middleware**: Custom middleware for exception handling, request logging, and cross-cutting concerns
- **DTOs**: Data Transfer Objects for API communication

### 2. **Application/Business Logic Layer** (`/Application`)
- **Services**: Business logic implementation and orchestration
- **Validators**: Manual validation methods (no external packages like FluentValidation)
- **Mappings**: Static mapping extensions for entity-DTO conversions

### 3. **Infrastructure Layer** (`/Infrastructure`)
- **Data Access**: Entity Framework Core with Repository and Unit of Work patterns
- **External Services**: Caching (Redis/In-Memory), JWT token management
- **Database Context**: ApplicationDbContext with Identity integration

### 4. **Core/Domain Layer** (`/Core`)
- **Entities**: Domain models with business logic
- **Interfaces**: Contracts for services and repositories
- **DTOs**: Data transfer objects
- **Enums**: Domain enumerations

### 5. **Cross-Cutting Concerns** (`/Extensions`)
- **Logging**: Serilog integration with structured logging
- **Caching**: Redis with in-memory fallback
- **Security**: JWT authentication with refresh tokens
- **Configuration**: Service registration and application setup

## 🚀 Features

### Core Functionality
- **Department Management**: CRUD operations for organizational departments
- **Employee Management**: Complete employee lifecycle management
- **Payroll Processing**: Automated payroll calculations and management
- **Payroll Items**: Flexible earnings and deductions system

### Security Features
- **JWT Authentication**: Secure token-based authentication
- **Refresh Tokens**: Automatic token renewal without re-login
- **Role-Based Authorization**: Multiple user roles (Administrator, HR Manager, Payroll Officer, Employee)
- **Password Security**: Strong password requirements and secure storage

### Technical Features
- **Caching**: Redis caching with in-memory fallback
- **Logging**: Comprehensive structured logging with Serilog
- **Health Checks**: Application and database health monitoring
- **Exception Handling**: Global exception handling with structured error responses
- **Request Logging**: Detailed request/response logging for monitoring

## 🛠️ Technology Stack

### Core Framework
- **.NET 10 Preview**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core**: ORM for data access
- **SQLite**: Database (easily switchable to SQL Server/PostgreSQL)

### Authentication & Security
- **ASP.NET Core Identity**: User management and authentication
- **JWT Bearer Tokens**: Stateless authentication
- **Refresh Tokens**: Secure token renewal mechanism

### Caching & Performance
- **Redis**: Distributed caching (with in-memory fallback)
- **Memory Caching**: Local caching for development

### Logging & Monitoring
- **Serilog**: Structured logging framework
- **Health Checks**: Application health monitoring
- **Request Logging**: Detailed HTTP request/response logging

### Development Tools
- **Manual Validation**: Custom validation methods (no external packages)
- **Static Mapping**: Performance-optimized object mapping
- **Extension Methods**: Clean service registration and configuration

## 📁 Project Structure

```
PayrollManagement.API/
├── Core/
│   ├── Entities/           # Domain models
│   ├── Interfaces/         # Service and repository contracts
│   ├── DTOs/              # Data transfer objects
│   └── Enums/             # Domain enumerations
├── Infrastructure/
│   ├── Data/              # Database context and configurations
│   ├── Repositories/      # Data access implementations
│   └── Services/          # External service implementations
├── Application/
│   ├── Services/          # Business logic services
│   ├── Validators/        # Manual validation methods
│   └── Mappings/          # Entity-DTO mapping extensions
├── Presentation/
│   ├── Controllers/       # API controllers
│   └── Middleware/        # Custom middleware
├── Extensions/            # Service registration and configuration
├── Properties/            # Launch settings
└── Program.cs            # Application entry point
```

## 🗄️ Database Schema

### Core Entities

#### **Departments**
- Department management with unique codes and names
- Hierarchical organization structure support

#### **Employees**
- Complete employee information management
- Department associations
- Status tracking (Active, Inactive, Terminated)

#### **Payrolls**
- Payroll period management
- Automated calculations (gross pay, deductions, net pay)
- Status workflow (Draft → Processed → Paid)

#### **Payroll Items**
- Flexible earnings and deductions system
- Configurable payroll item types
- Support for various compensation components

#### **Users & Authentication**
- ASP.NET Core Identity integration
- Custom user properties
- Refresh token management

### Entity Relationships
- **Department** → **Employees** (One-to-Many)
- **Employee** → **Payrolls** (One-to-Many)
- **Payroll** → **PayrollItems** (One-to-Many)
- **PayrollItemType** → **PayrollItems** (One-to-Many)
- **ApplicationUser** → **UserRefreshTokens** (One-to-Many)

## 🔧 Configuration

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=payroll.db",
    "RedisConnection": ""
  }
}
```

### JWT Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here-must-be-at-least-32-characters",
    "Issuer": "PayrollManagement.API",
    "Audience": "PayrollManagement.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  }
}
```

### Logging Configuration
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/payroll-api-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 🚀 Getting Started

### Prerequisites
- .NET 10 Preview SDK
- SQLite (included)
- Redis (optional, falls back to in-memory cache)

### Installation & Setup

1. **Clone and Navigate**
   ```bash
   cd PayrollManagement.API
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build Application**
   ```bash
   dotnet build
   ```

4. **Run Application**
   ```bash
   dotnet run --urls="http://localhost:5000"
   ```

5. **Verify Installation**
   ```bash
   curl http://localhost:5000/health
   # Should return: Healthy
   ```

### Default Users
The application automatically creates default users:

- **Administrator**: `admin@payroll.com` / `Admin@123456`
- **HR Manager**: `hr@payroll.com` / `HR@123456`

## 📚 API Endpoints

### Authentication Endpoints
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh-token` - Token refresh
- `POST /api/auth/logout` - User logout
- `POST /api/auth/change-password` - Password change
- `GET /api/auth/profile` - Get user profile
- `PUT /api/auth/profile` - Update user profile

### Department Endpoints
- `GET /api/departments` - List all departments
- `GET /api/departments/{id}` - Get department by ID
- `POST /api/departments` - Create new department
- `PUT /api/departments/{id}` - Update department
- `DELETE /api/departments/{id}` - Delete department

### Employee Endpoints
- `GET /api/employees` - List employees (with pagination and filtering)
- `GET /api/employees/{id}` - Get employee by ID
- `POST /api/employees` - Create new employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Delete employee
- `GET /api/employees/search` - Search employees
- `GET /api/employees/department/{departmentId}` - Get employees by department

### Payroll Endpoints
- `GET /api/payrolls` - List payrolls (with pagination and filtering)
- `GET /api/payrolls/{id}` - Get payroll by ID
- `POST /api/payrolls` - Create new payroll
- `PUT /api/payrolls/{id}` - Update payroll
- `DELETE /api/payrolls/{id}` - Delete payroll
- `POST /api/payrolls/generate` - Generate payrolls for all employees
- `POST /api/payrolls/{id}/process` - Process payroll
- `POST /api/payrolls/{id}/pay` - Mark payroll as paid

### System Endpoints
- `GET /health` - Health check endpoint

## 🔒 Security Features

### Authentication Flow
1. **Registration/Login**: User provides credentials
2. **JWT Generation**: System generates access token and refresh token
3. **API Access**: Client uses access token for API calls
4. **Token Refresh**: Client uses refresh token to get new access token
5. **Logout**: System revokes refresh tokens

### Authorization Levels
- **Administrator**: Full system access
- **HR Manager**: Employee and department management
- **Payroll Officer**: Payroll processing and management
- **Employee**: Read-only access to own information

### Security Best Practices
- Strong password requirements
- JWT token expiration
- Refresh token rotation
- Role-based access control
- Secure password hashing
- Request logging and monitoring

## 📊 Logging & Monitoring

### Structured Logging
The application uses Serilog for structured logging with:
- **Request/Response Logging**: All HTTP requests and responses
- **Business Logic Logging**: Service operations and business events
- **Error Logging**: Detailed exception information
- **Performance Logging**: Request timing and performance metrics

### Health Checks
- **Database Health**: Entity Framework connection status
- **Application Health**: General application status
- **Redis Health**: Cache service availability (if configured)

### Log Levels
- **Information**: General application flow
- **Warning**: Unexpected situations that don't stop execution
- **Error**: Error events that might still allow the application to continue
- **Fatal**: Very severe error events that might cause the application to abort

## 🧪 Testing

### Manual Testing
The application includes comprehensive logging and health checks for manual testing:

1. **Health Check**: `GET /health`
2. **Authentication**: Register and login with test users
3. **CRUD Operations**: Test all entity operations
4. **Business Logic**: Test payroll processing workflows

### API Testing Tools
- **Postman**: Import API collection for comprehensive testing
- **curl**: Command-line testing examples provided
- **Swagger**: API documentation (can be re-enabled)

## 🔧 Development Guidelines

### Code Organization
- **Separation of Concerns**: Each layer has distinct responsibilities
- **Dependency Injection**: All dependencies are injected
- **Interface Segregation**: Small, focused interfaces
- **Single Responsibility**: Each class has one reason to change

### Best Practices Implemented
- **Repository Pattern**: Data access abstraction
- **Unit of Work Pattern**: Transaction management
- **Service Layer Pattern**: Business logic encapsulation
- **DTO Pattern**: Data transfer optimization
- **Extension Methods**: Clean configuration and setup

### Validation Strategy
- **Data Annotations**: Basic property validation
- **Manual Validation**: Custom business rule validation
- **Service Layer Validation**: Complex business logic validation
- **No External Dependencies**: Pure .NET validation approach

## 🚀 Deployment Considerations

### Environment Configuration
- **Development**: SQLite database, in-memory cache, detailed logging
- **Production**: SQL Server/PostgreSQL, Redis cache, optimized logging

### Scalability Features
- **Stateless Design**: JWT-based authentication
- **Caching Strategy**: Redis for distributed caching
- **Database Optimization**: Proper indexing and relationships
- **Logging Strategy**: Structured logging for monitoring

### Security Hardening
- **HTTPS Enforcement**: Production HTTPS redirection
- **CORS Configuration**: Environment-specific CORS policies
- **Token Security**: Secure token generation and validation
- **Input Validation**: Comprehensive input validation

## 📈 Performance Optimizations

### Database Performance
- **Entity Framework Optimization**: Proper includes and projections
- **Indexing Strategy**: Unique indexes on business keys
- **Query Optimization**: Efficient LINQ queries
- **Connection Management**: Proper DbContext lifecycle

### Caching Strategy
- **Distributed Caching**: Redis for scalable caching
- **Cache Patterns**: Cache-aside pattern implementation
- **Cache Invalidation**: Proper cache key management
- **Fallback Strategy**: In-memory cache fallback

### API Performance
- **Pagination**: Efficient data pagination
- **Filtering**: Server-side filtering and searching
- **Response Optimization**: Minimal data transfer
- **Async Operations**: Non-blocking I/O operations

## 🤝 Contributing

### Development Setup
1. Install .NET 10 Preview SDK
2. Clone the repository
3. Run `dotnet restore`
4. Run `dotnet build`
5. Run `dotnet run`

### Code Standards
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Implement proper error handling
- Write comprehensive logs

## 📄 License

This project is created for educational and demonstration purposes, showcasing enterprise-level ASP.NET Core Web API development with layered architecture.

## 🙏 Acknowledgments

This application demonstrates the practical implementation of the Web API architecture guide, showing how theoretical concepts translate into a real-world, production-ready application with:

- **Complete Layered Architecture**: Proper separation of concerns
- **Enterprise Patterns**: Repository, Unit of Work, Service Layer
- **Security Best Practices**: JWT, Role-based authorization, secure validation
- **Performance Optimization**: Caching, efficient queries, proper indexing
- **Monitoring & Logging**: Comprehensive observability
- **Scalability**: Stateless design, distributed caching, proper configuration

The codebase serves as a reference implementation for building robust, scalable, and maintainable Web APIs using ASP.NET Core and modern development practices.

