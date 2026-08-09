import { MaterialCommunityIcons } from '@expo/vector-icons';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import React from 'react';
import { useAuth } from '../core/auth/auth.context';
import { RightCode } from '../core/auth/right-code';
import UsersScreen from '../screens/admin/UsersScreen';
import HomeScreen from '../screens/home/HomeScreen';
import InventoryScreen from '../screens/inventory/InventoryScreen';
import InvoicesScreen from '../screens/invoices/InvoicesScreen';
import VouchersScreen from '../screens/vouchers/VouchersScreen';

const Tab = createBottomTabNavigator();

type IconName = keyof typeof MaterialCommunityIcons.glyphMap;

function tabIcon(name: IconName) {
  return ({ color, size }: { color: string; size: number }) => (
    <MaterialCommunityIcons name={name} color={color} size={size} />
  );
}

export default function MainTabs() {
  const { hasRight } = useAuth();
  // Users.View gates the whole Admin tab, same as the web app's route guard (app.routes.ts).
  const showAdminTab = hasRight(RightCode.UsersView);

  return (
    <Tab.Navigator screenOptions={{ headerShown: true }}>
      <Tab.Screen name="Home" component={HomeScreen} options={{ tabBarIcon: tabIcon('home') }} />
      <Tab.Screen name="Vouchers" component={VouchersScreen} options={{ tabBarIcon: tabIcon('cash-multiple') }} />
      <Tab.Screen
        name="Invoices"
        component={InvoicesScreen}
        options={{ tabBarIcon: tabIcon('file-document-outline') }}
      />
      <Tab.Screen name="Inventory" component={InventoryScreen} options={{ tabBarIcon: tabIcon('warehouse') }} />
      {showAdminTab ? (
        <Tab.Screen name="Admin" component={UsersScreen} options={{ tabBarIcon: tabIcon('account-cog') }} />
      ) : null}
    </Tab.Navigator>
  );
}
