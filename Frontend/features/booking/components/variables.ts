export const appointmentInterval = process.env.NEXT_PUBLIC_APPOINTMENT_INTERVAL
  ? Number(process.env.NEXT_PUBLIC_APPOINTMENT_INTERVAL)
  : 15;
export const appointmentStart = process.env.NEXT_PUBLIC_APPOINTMENT_START
  ? Number(process.env.NEXT_PUBLIC_APPOINTMENT_START)
  : 8;
export const appointmentEnd = process.env.NEXT_PUBLIC_APPOINTMENT_END
  ? Number(process.env.NEXT_PUBLIC_APPOINTMENT_END)
  : 16;
