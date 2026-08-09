import { Platform } from 'react-native';

/**
 * The backend's plain-HTTP launch profile (see
 * `erp backend/erp backend/Properties/launchSettings.json` — the "https" profile listens on
 * both 7002 (TLS, self-signed dev cert) and 5023 (plain HTTP)). We default to the HTTP port
 * because React Native's networking stack won't trust the ASP.NET Core dev cert out of the
 * box, unlike a browser where you can just click through the warning.
 *
 * - Android emulator: 10.0.2.2 is the emulator's alias for the host machine's localhost.
 * - iOS simulator: localhost works directly (it shares the host's network namespace).
 * - Physical device (either OS): neither localhost nor 10.0.2.2 reach your dev machine — set
 *   EXPO_PUBLIC_API_URL in a .env file to your machine's LAN IP instead, e.g.
 *   http://192.168.1.20:5023 (see .env.example).
 */
const DEV_API_HOST = Platform.select({
  android: 'http://10.0.2.2:5023',
  default: 'http://localhost:5023',
});

export const API_URL = process.env.EXPO_PUBLIC_API_URL ?? DEV_API_HOST ?? 'http://localhost:5023';
