import React, { useState } from 'react';
import { ScrollView, StyleSheet } from 'react-native';
import { Button, Card, Divider, Menu, Text } from 'react-native-paper';
import { useAuth } from '../../core/auth/auth.context';
import { useTenancy } from '../../core/tenancy/tenancy.context';

export default function HomeScreen() {
  const { username, role, logout } = useAuth();
  const { tenantName, branches, currentBranch, selectBranch } = useTenancy();
  const [menuVisible, setMenuVisible] = useState(false);

  return (
    <ScrollView contentContainerStyle={styles.container}>
      <Card style={styles.card}>
        <Card.Content>
          <Text variant="titleLarge">Welcome, {username}</Text>
          <Text variant="bodyMedium" style={styles.muted}>
            {role} · {tenantName}
          </Text>
          <Divider style={styles.divider} />
          <Text variant="labelLarge" style={styles.label}>
            Branch
          </Text>
          {branches.length > 1 ? (
            <Menu
              visible={menuVisible}
              onDismiss={() => setMenuVisible(false)}
              anchor={
                <Button mode="outlined" onPress={() => setMenuVisible(true)}>
                  {currentBranch?.name ?? 'Select branch'}
                </Button>
              }
            >
              {branches.map((branch) => (
                <Menu.Item
                  key={branch.id}
                  title={`${branch.name} (${branch.code})`}
                  onPress={() => {
                    setMenuVisible(false);
                    void selectBranch(branch.id);
                  }}
                />
              ))}
            </Menu>
          ) : (
            <Text variant="bodyMedium">{currentBranch?.name ?? '—'}</Text>
          )}
        </Card.Content>
      </Card>

      <Button mode="text" onPress={() => void logout()} style={styles.logout}>
        Sign out
      </Button>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { padding: 16 },
  card: { marginBottom: 16 },
  muted: { opacity: 0.6, marginBottom: 8 },
  divider: { marginVertical: 12 },
  label: { marginBottom: 8 },
  logout: { alignSelf: 'center' },
});
