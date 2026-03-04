![](http://images.restapi.co.za/pvt/Noroff-64.png)

# Noroff

# Back-end Development Year 2

## Clinic Hub API

A .NET 10 Web API. This was built and run using version 10.0.100 of the .NET SDK.

### Technologies

- **Runtime**: .NET 10
- **ORM**: Entity Framework Core with MySQL
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI

## Setup Instructions

1. Clone the repository and navigate to the backend directory.

2. Using the `appsettings.example.json` file as a template, create your own `appsettings.json` file in the root of the project and add your own values.

```json
{
  "JwtSettings": {
    "SecretKey": " MySecretKey",
    "Issuer": "MyIssuer",
    "Audience": "MyAudience",
    "ExpiryMinutes": 600
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AdminSettings": {
    "DefaultUsername": "admin",
    "DefaultEmail": "admin@clinic.com",
    "DefaultPassword": "ChangeMe123!"
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=exam_project_2;user=root;password=password123"
  }
}
```

4. Ensure you have a MySQL server running and details of your created database are correctly set in the `appsettings.json` file. This should match the ConnectionStrings:DefaultConnection value.

```json
"ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=dev_house;user=root;password=superSecurePassword"
  }
```

5. Opening a terminal in the backend directory, install the required dependencies using the .NET CLI:

```bash
dotnet restore
```

6. Apply database migrations to set up the database schema:

```bash
dotnet ef database update
```

If you don't have the EF tools installed, you can do so with:

```bash
dotnet tool install --global dotnet-ef
```

If the migrations are not present, you can create them using:

```bash
dotnet ef migrations add Initial
```

Then run the update command again:

```bash
dotnet ef database update
```

7. The application includes a seeding mechanism to populate the database with test data for better frontend development and testing. You can run the seeding process with the following command:

```bash
dotnet run -- --seed
```

8. Finally, start the server with:

```bash
dotnet run
```

9.

Run the following to seed the database with test data:
dotnet run -- --seed

Run the following to start the server:
dotnet run

5. Install the required dependencies using the .NET CLI

```bash
dotnet restore
```

6. Apply database migrations to set up the database schema

```bash
dotnet ef database update
```

If you don't have the EF tools installed, you can do so with:

```bash
dotnet tool install --global dotnet-ef
```

If the migrations are not present, you can create them using:

```bash
dotnet ef migrations add Initial
```

Then run the update command again:

```bash
dotnet ef database update
```

7. Run the application

```bash
dotnet run
```

8. The application includes a seeding mechanism to populate the database with test data for better frontend development and testing. You can run the seeding process with the following command:

```bash
dotnet run -- --seed
```

9. Finally, start the server with:

```bash
dotnet run
```

10. The API is currently configured to run on `https://localhost:7071` and the Swagger documentation is available at `https://localhost:7071/doc` when running locally.

## ERD

![ERD](ERD.jpg)

## Endpoints

### Auth

| Method | Path                | Description                                                                       | Access |
| :----- | :------------------ | :-------------------------------------------------------------------------------- | :----- |
| POST   | `/auth/login`       | Authenticates a user and returns a JWT token                                      | Public |
| POST   | `/auth/register`    | Registers a new patient account, or converts a guest profile to a patient account | Public |
| POST   | `/auth/admin/login` | Authenticates administrative staff                                                | Public |

### Patients

| Method | Path                        | Description                                        | Access        |
| :----- | :-------------------------- | :------------------------------------------------- | :------------ |
| GET    | `/patients`                 | Retrieves a paged list of active patients          | Admin         |
| GET    | `/patients/{id}`            | Retrieves a specific patient profile by ID         | Admin / Owner |
| POST   | `/patients`                 | Allow admin to create basic guest profiles         | Admin         |
| POST   | `/patients/change-password` | Securely updates the authenticated user's password | Patient       |
| PATCH  | `/patients/{id}`            | Partially updates a patient's profile              | Admin / Owner |
| DELETE | `/patients/{id}`            | Permanently deletes a patient record               | Admin         |
| DELETE | `/patients/anonymize/{id}`  | Anonymizes a patient's profile (soft delete)       | Admin         |

### Appointments

| Method | Path                         | Description                                                         | Access        |
| :----- | :--------------------------- | :------------------------------------------------------------------ | :------------ |
| GET    | `/appointments`              | Retrieves a paged and filtered list of all appointments             | Admin         |
| GET    | `/appointments/me`           | Retrieves a paged list of the authenticated patient's appointments  | Patient       |
| GET    | `/appointments/{id}`         | Retrieves details for a specific appointment                        | Admin / Owner |
| GET    | `/appointments/booked-times` | Retrieves a doctor's booked time slots within a specific date range | Public        |
| POST   | `/appointments`              | Books a new appointment                                             | Public        |
| PATCH  | `/appointments/{id}`         | Updates an existing appointment's schedule or doctor                | Admin / Owner |
| DELETE | `/appointments/{id}`         | Cancels and deletes a specific appointment                          | Admin / Owner |

### Doctors

| Method | Path              | Description                                                    | Access |
| :----- | :---------------- | :------------------------------------------------------------- | :----- |
| GET    | `/doctors`        | Retrieves a paged list of doctors for the public directory     | Public |
| GET    | `/doctors/search` | Searches for doctors using name-based tokenization and filters | Public |
| GET    | `/doctors/{id}`   | Retrieves detailed profile information for a doctor            | Public |
| POST   | `/doctors`        | Registers a new doctor into the system                         | Admin  |
| PATCH  | `/doctors/{id}`   | Updates an existing doctor's profile                           | Admin  |
| DELETE | `/doctors/{id}`   | Removes a doctor from the directory                            | Admin  |

### Clinics

| Method | Path            | Description                          | Access |
| :----- | :-------------- | :----------------------------------- | :----- |
| GET    | `/clinics`      | Retrieves all clinics                | Public |
| GET    | `/clinics/{id}` | Retrieves a clinic by ID             | Public |
| POST   | `/clinics`      | Creates a new clinic into the system | Admin  |
| PATCH  | `/clinics/{id}` | Partially updates a clinic profile   | Admin  |
| DELETE | `/clinics/{id}` | Deletes a clinic                     | Admin  |

### Categories

| Method | Path               | Description                                      | Access |
| :----- | :----------------- | :----------------------------------------------- | :----- |
| GET    | `/categories`      | Retrieves a paged list of appointment categories | Public |
| GET    | `/categories/{id}` | Retrieves a specific appointment category        | Public |
| POST   | `/categories`      | Creates a new appointment category               | Admin  |
| PATCH  | `/categories/{id}` | Partially updates an appointment category        | Admin  |
| DELETE | `/categories/{id}` | Deletes an appointment category                  | Admin  |

### Specialities

| Method | Path                 | Description                                        | Access |
| :----- | :------------------- | :------------------------------------------------- | :----- |
| GET    | `/specialities`      | Retrieves a paged list of all medical specialities | Public |
| GET    | `/specialities/{id}` | Retrieves a specific speciality by ID              | Public |
| POST   | `/specialities`      | Creates a new medical speciality                   | Admin  |
| PATCH  | `/specialities/{id}` | Updates a speciality                               | Admin  |
| DELETE | `/specialities/{id}` | Deletes a speciality                               | Admin  |

**Full Documentation**: Available at `/doc` (Swagger) when running locally.

## PROJECT LOGIC & ARCHITECTURE

### Data Privacy (PII)

Based on course material defining non-sensitive PII as publicly available demographic information, guest users are permitted to store a limited subset of data (name, email, birthdate and gender). Sensitive PII and authentication credentials are strictly restricted to registered patients.

### Appointment Constraints

Appointment durations are enforced by the backend based on the selected Category. This ensures that a "General Checkup" cannot be accidentally (or maliciously) booked for 5 hours, maintaining system integrity regardless of frontend behavior.

### The "Date" Challenge

Handling timezones across a MySQL database and a React frontend was a significant focus. All dates are synchronized using UTC to prevent scheduling offsets between the client and the server.
