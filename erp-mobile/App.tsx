import { StatusBar } from 'expo-status-bar';
import React from 'react';
import { PaperProvider } from 'react-native-paper';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { AuthProvider } from './src/core/auth/auth.context';
import { TenancyProvider } from './src/core/tenancy/tenancy.context';
import RootNavigator from './src/navigation/RootNavigator';

export default function App() {
  return (
    <SafeAreaProvider>
      <PaperProvider>
        {/* TenancyProvider wraps AuthProvider so login()/logout() can drive tenancy state directly
            — same dependency direction as AuthService injecting TenancyService on the web. */}
        <TenancyProvider>
          <AuthProvider>
            <RootNavigator />
          </AuthProvider>
        </TenancyProvider>
        <StatusBar style="auto" />
      </PaperProvider>
    </SafeAreaProvider>
  );
}
