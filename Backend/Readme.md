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

| Method | Path           | Description                                                   | Access |
| :----- | :------------- | :------------------------------------------------------------ | :----- |
| GET    | `/admins`      | Retrieves a paged list of all administrative accounts         | Admin  |
| GET    | `/admins/{id}` | Retrieves a specific admin profile by ID                      | Admin  |
| POST   | `/admins`      | Registers a new administrative user with hashed credentials   | Admin  |
| PATCH  | `/admins/{id}` | Partially updates admin info (Username, Email, or Password)   | Admin  |
| DELETE | `/admins/{id}` | Deletes an admin; denied if deleting the last remaining admin | Admin  |

**Full Documentation**: Available at `/doc` (Swagger) when running locally.

## PROJECT LOGIC & ARCHITECTURE

### Data Privacy (PII)

- Guest users store a minimal subset of personal data required to create and manage an appointment. This includes Firstname, Lastname, Email, and DateOfBirth.
- Sensitive identifiers (such as social security numbers, insurance identifiers, and authentication credentials) are restricted to registered patients only.
- Email is required for guest bookings to allow appointment communication and to reduce duplicate or abusive bookings.

In a production system, email verification could be used to confirm ownership before converting a guest patient into a registered account.

### Appointment Constraints

The system enforces several constraints to ensure valid appointment scheduling:

- Appointment booking times must be divided into 5-minute increments, eg. 8:00, 8:05, 8:10, etc. This is validated on the backend and rejected if not met.
- The API allows for booking anytime, the frontend has environment variables to configure the visible booking hours and slot intervals. No requirement was given to enforce this, it was done purely for user experience frontend side, but the backend will accept any valid time as long as it meets the 5-minute increment rule.
- Appointments cannot be booked in the past, and the API validates this to prevent scheduling errors.
- Each appointment must be associated with a valid patient, doctor, category, and clinic, ensuring data integrity and proper scheduling. I didn't get clinic based on doctor as potentially a doctor could change clinics in the future. I only enforce based on the doctors current clinic at the time of booking, and allow for changing the clinic later if needed, but have enforced that a doctor have no future appointments when changing clinics to prevent scheduling conflicts.

### The "Date" Challenge

Handling timezones across a MySQL database and a React frontend was a significant focus. All dates are synchronized using UTC to prevent scheduling offsets between the client and the server. After getting this correct I didn't attempt to enforce clinic opening hours server side as that could get messy with timezone conversions and the frontend already has configuration for this, so I left it as a user experience feature on the frontend to only show available slots within the configured hours, but the backend will accept any valid time as long as it meets the 5-minute increment rule and is not in the past.
