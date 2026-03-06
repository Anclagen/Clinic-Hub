![](http://images.restapi.co.za/pvt/Noroff-64.png)

# Noroff

# Back-end Development Year 2

## Clinic Hub Frontend

Next.js frontend for the Clinic Hub booking system.  
Consumes the REST API and provides public booking, patient login, and basic directory/lookup UI.

### Tech stack

- Next.js 16 (App Router)
- React 19
- TypeScript
- Tailwind CSS
- Zustand (auth/session state)
- React Hook Form + Zod (form handling + validation)
- date-fns + date-fns-tz (date/time handling)
- react-day-picker (calendar UI)

### Features

The frontend provides the following functionality:

- Public appointment booking
- Guest patient booking flow
- Calendar-based slot selection, with dynamic generation based on backend availability and configured booking hours
- Patient login and registration
- Patient appointment overview, cancellation, and rescheduling
- Doctor directory, filters by specialty/clinic, and search by doctor name
- Clinic lookup, with details page showing associated doctors at location
  Authentication state is managed using Zustand and persisted locally to maintain session state between refreshes.

### Requirements

- Node.js (LTS recommended)
- Running backend API (ASP.NET Core) on HTTPS

### Environment variables

Create a `.env` file in the frontend root:

```bash
# API url
NEXT_PUBLIC_API_BASE_URL=https://localhost:7071
NEXT_PUBLIC_TIMEZONE=Europe/Oslo

# Booking slot configuration
#(minutes, increments of 5 recommended, API rejects based on %5 validation )
NEXT_PUBLIC_APPOINTMENT_INTERVAL=15
#(hours, whole numbers, 24h format, haven't supported decimal hours yet)
NEXT_PUBLIC_APPOINTMENT_START=8
NEXT_PUBLIC_APPOINTMENT_END=16
```

### Notes

- `NEXT_PUBLIC_API_BASE_URL` must match the backend HTTPS URL.
- Time values are used for booking slot generation on the client.
- If you change backend ports, update the env value accordingly.
- Timezone handling: Frontend uses `NEXT_PUBLIC_TIMEZONE=Europe/Oslo` together with date-fns-tz to present times consistently to Norwegian users while sending appointment timestamps to the API in ISO format (UTC).

### Install & Run

```bash
# Install dependencies
npm install
# Run development server
npm run dev
```

Open `http://localhost:3000` in your browser to access the frontend.
