[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/15MjVqac)

![](http://images.restapi.co.za/pvt/Noroff-64.png)

# Noroff

# Back-end Development Year 2

### Exam Project 2

This repository does not have any startup code. Use the 2 folders

- Backend
- Frontend

for your respective applications.

Instruction for the course assignment is in the LMS (Moodle) system of Noroff.
[https://lms.noroff.no](https://lms.noroff.no)

![](http://images.restapi.co.za/pvt/ca_important.png)

You will not be able to make any submissions after the course assignment deadline. Make sure to make all your commit **BEFORE** the deadline to this repository.

![](http://images.restapi.co.za/pvt/help.png)

If you need help with any instructions for the course assignment, contact your teacher on **Microsoft Teams**.

**REMEMBER** Your Moodle LMS submission must have your repository link **AND** your Github username in the text file.
Questions on brief:

- registration enforce non-sensitive PII or password and dob minimum with option to add additional non-sensitive PII later?
- guest user needs first name, last name, and dob to make an appointment, can I add email as required/optional to allow registration follow up to edit appointment?
- You have email and email address in the brief for patient info I assume the second is a mistake and should be physical address?

References

- https://medium.com/@emreemenekse/a-comprehensive-guide-to-jwt-authentication-in-net-core-8e2d8859b1be

// jwt claims sub changes to nameidentifier when using asp.net core identity, how to fix?

- https://stackoverflow.com/questions/62475109/asp-net-core-jwt-authentication-changes-claims-sub
- https://stackoverflow.com/questions/68252520/httpcontext-user-claims-doesnt-match-jwt-token-sub-changes-to-nameidentifie/68253821#68253821

Seeders
https://medium.com/@samsondavidoff/data-seeding-in-asp-net-core-the-right-way-4c7c1f4b1773

Image error handling
https://dev.to/eidellev/handling-broken-images-in-react-4oo2

Heart rate animation
https://github.com/Hona-08/Heart-rate-monitor-Pure-CSS-Animation

swagger issues dotnet 10
https://stackoverflow.com/questions/79834574/authentication-not-working-in-swagger-with-net-10
https://github.com/dotnet/aspnetcore/issues/64524
https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/configure-and-customize-swaggergen.md#add-security-definitions-and-requirements-for-bearer-authentication
Added a custom operation filter to add the Authorize header to the Swagger UI, and added the security definition for JWT Bearer authentication, between Claude, Gemini and ChatGPT, and some help from StackOverflow were able to get it working.

validation the great rabbit hole
https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern
https://andrewlock.net/handling-web-api-exceptions-with-problemdetails-middleware/
https://docs.fluentvalidation.net/en/latest/aspnet.html

PII
_Based on course material defining non-sensitive PII as publicly available demographic information (including gender and religion), guest users are permitted to store a limited subset of non-sensitive PII. For guest bookings, only first name, last name, email, and optionally birthdate, gender, and religion are stored. Sensitive PII such as government identifiers, tax numbers, insurance numbers, and authentication credentials are restricted to registered patients and never stored for guest users._

Appointment Durations
_Appointment durations are constrained by appointment category. Each category defines a default (or allowed set of) duration(s), preventing arbitrary booking lengths and simplifying scheduling validation. The backend enforces that appointment duration values match the selected category, ensuring data integrity regardless of client behaviour._

Clinics
_Clinic entities contain minimal identifying and display information (name, optional address, optional image) in line with the project scope, and just adding some quality of output to the frontend aesthetics._

# Clinic Appointment Booking System

Scenario
A group of medical clinics has decided to offer patients an online Appointment Booking system. They have approached you to develop this system.
It must take the form of a full-stack web application (Front-end, Back-end, Database) that allows patients to book a doctor's appointment without registering or logging in to the system.

For this Exam Project, you are required to create the following:

| Component           | Required technology                |
| ------------------- | ---------------------------------- |
| Database            | MySQL                              |
| REST API back-end   | ASP.NET Core with Entity Framework |
| Front-end interface | React                              |
| API Documentation   | Swagger                            |

Tip
If your front-end and back-end are on different ports during development, you may encounter CORS errors.
Learn how to configure your system accordingly: Microsoft ASP.NET Core Security

## Instructions

### Database

- A MySQL Database must be created and used for this application. The database must be designed in the 3rd normal form.
- A Code-First development approach should be used to create the database.
- The tables must include relevant columns and data types.
- All relevant relationships between tables must also be created.
- The initial database creation, all database operations and queries must be performed using Entity Framework.

#### DB Requirements

- Each doctor has a Speciality (Some have the same Speciality).
- A patient can have more than one appointment. For each appointment, there would be one category, and an appointment will be specific to one clinic.
- A clinic can have more than one doctor, and each doctor has a specific speciality. Remember to account for many doctors having the same speciality.

### Back-end

- An ASP.NET Core REST API must be created as the back-end of this application.
- Entity Framework must be used as the ORM.
- REST API endpoints must be created to facilitate CRUD operations for all tables in the database.
- All endpoints must return the correct results as JSON objects.
- Validation must exist to prevent duplicate data records from being added to the database.
- Validation must exist to check for existing dependencies before deletion of database records.

If you require more endpoints in your back-end, they can be added to the project (Remember to specify these in your README file too).

### Authentication

- Authentication should be implemented in this project. User tokens should be stored upon successful login, and used for applicable endpoints.
- Users can also use the system without authentication (i.e., no registration or login required), but functionality is limited (see below) and different user data is stored.

### Patients and Users

Users can choose to either register as a Patient, or to use the system without registering (i.e. as a Guest User) - design the database to accommodate this.
When either type of user creates an appointment, some patient information must be stored in the database.
Both registered Patient data and unregistered Guest User data must be stored in the same Database table (don't create separate tables for different types of users).
The following are examples of some Patient data that is commonly captured by online booking systems:

- First name;
- Last name;
- Email;
- Social Security Number;
- Birthdate;
- Gender;
- Tax Number;
- Religion;
- address;
- Driver’s License Number;
- Medical Insurance Member Number.

**Remember**
_For this Clinic Appointment Booking System, ensure that only the Non-Sensitive PII from this list is stored in the database for Guest Users, while all the mentioned data is stored for registered Patients._

### Appointments

When creating an Appointment, validation should be in place to prevent the creation of a conflicting appointment. For example, a patient at a clinic cannot book 2 appointments at the same time.
Registered Patients can log in to:

- Book appointments
- View their existing appointments
- Update/Cancel their existing appointments.

Guest Users should be able to book appointments, however they should not be able log in and maintain their appointments.

###Search
Create a single API endpoint allowing users to search for a doctor’s first name or last name. This search should return a JSON object as a result, containing:

- The doctor’s full name
- The name of the clinic where the doctor is assigned
- The name of the doctor’s speciality

LINQ or raw SQL can be used for the database query in this endpoint. Ensure that adequate validation has been implemented for this endpoint.

## Front-end

Your Exam Project must include a separate Front-end Interface.
The front-end system must only use API endpoints created in the back-end.
The front-end should be implemented as follows:

-The application should display a simple loading element to the user while it processes network requests to the server.
-The application should include a simple header with a navigation bar, as well as a footer displaying the current year on all pages.
-The application does require a registration and login screen for those users who wish to register.
-The appointment booking page should be the first page displayed at the root of the application (‘/’).
-UI component libraries can be optionally used to enhance the application’s look and feel.
-A patient (both registered patient and unregistered guest) should be able to book an appointment. Patient information, such as first and last name, and date of birth, should be supplied, as well as appointment details, such as the doctor chosen (from a dynamically loaded Select element with options from the database), appointment date, and appointment duration (in minutes). Appropriate error messages should be displayed to the user if the appointment form is invalid, including if the date and time chosen for the appointment are already booked by another patient (i.e., Validation).
-A search page where a doctor’s first or last name can be entered and a list of details, such as the full name, clinic name, and the doctor’s speciality, can be populated. If the doctor could not be found, an appropriate message should be displayed to the user.
-The application should have routes for the above functionality using a path of ‘/book’ and ‘/search’ respectively.

Attention should be given to user experience and functionality when designing the front-end.

## Documentation

### API Documentation

The API documentation (Swagger) must include methods and JSON objects. It must be accessible from the endpoint /doc from the API URL.

`For example: http://localhost:3000/doc`

### README

A description of each endpoint must be specified in the README file.
This must be indicated in the project’s README file under the heading "ENDPOINTS".

References
Students must indicate where they have received help or used outside knowledge for their Exam Project. This must be indicated in the project’s README file under the heading "REFERENCES". This includes:

- Acknowledgements of any help received from other students (if the student is working in a mentor group).
- Any code or knowledge that has been sourced from internet forums, textbooks, AI-generated code, etc.

### ENDPOINTS

### REFERENCES
