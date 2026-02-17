![](http://images.restapi.co.za/pvt/Noroff-64.png)

# Noroff

# Back-end Development Year 2

#### This folder should be used for the back-end code.

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
