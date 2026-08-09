import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, View } from 'react-native';
import { ActivityIndicator, Badge, Chip, Divider, FAB, Text } from 'react-native-paper';
import { RightCode } from '../../core/auth/right-code';
import { useAuth } from '../../core/auth/auth.context';
import { getBranches } from '../../core/branches/branch.api';
import type { Branch } from '../../core/branches/branch.types';
import { getRoles } from '../../core/roles/role.api';
import type { RoleOption } from '../../core/roles/role.types';
import { getUsers } from '../../core/users/user.api';
import type { AdminUser } from '../../core/users/user.types';
import UserFormModal from './UserFormModal';

export default function UsersScreen() {
  const { hasRight } = useAuth();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [roles, setRoles] = useState<RoleOption[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);
  const [creating, setCreating] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getUsers();
      setUsers(data);
    } catch {
      setError('Unable to load users. Pull to retry.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    getRoles()
      .then(setRoles)
      .catch(() => {});
    getBranches()
      .then(setBranches)
      .catch(() => {});
  }, []);

  const canCreate = hasRight(RightCode.UsersCreate);
  const canEdit = hasRight(RightCode.UsersEdit);

  return (
    <View style={styles.container}>
      {loading && users.length === 0 ? (
        <ActivityIndicator style={styles.loading} />
      ) : (
        <FlatList
          data={users}
          keyExtractor={(item) => String(item.id)}
          refreshControl={<RefreshControl refreshing={loading} onRefresh={load} />}
          ItemSeparatorComponent={Divider}
          contentContainerStyle={users.length === 0 ? styles.emptyContainer : undefined}
          ListEmptyComponent={
            <Text variant="bodyMedium" style={styles.empty}>
              {error ?? 'No users yet.'}
            </Text>
          }
          renderItem={({ item }) => (
            <View style={styles.row} onTouchEnd={() => canEdit && setEditingUser(item)}>
              <View style={styles.rowMain}>
                <Text variant="titleSmall">{item.username}</Text>
                <Text variant="bodySmall" style={styles.muted}>
                  {item.role}
                </Text>
                <View style={styles.chipRow}>
                  {item.branches.map((b) => (
                    <Chip key={b.id} compact style={styles.chip}>
                      {b.code}
                    </Chip>
                  ))}
                </View>
              </View>
              <Badge style={[styles.badge, { backgroundColor: item.isActive ? '#27AE60' : '#9E9E9E' }]}>
                {item.isActive ? 'Active' : 'Inactive'}
              </Badge>
            </View>
          )}
        />
      )}

      {canCreate ? <FAB icon="plus" style={styles.fab} onPress={() => setCreating(true)} label="New user" /> : null}

      <UserFormModal
        visible={creating}
        user={null}
        roles={roles}
        branches={branches}
        onDismiss={() => setCreating(false)}
        onSaved={() => {
          setCreating(false);
          void load();
        }}
      />

      <UserFormModal
        visible={editingUser !== null}
        user={editingUser}
        roles={roles}
        branches={branches}
        onDismiss={() => setEditingUser(null)}
        onSaved={() => {
          setEditingUser(null);
          void load();
        }}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  loading: { marginTop: 32 },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', padding: 16 },
  rowMain: { flex: 1, paddingRight: 12 },
  muted: { opacity: 0.6, marginTop: 2, marginBottom: 6 },
  chipRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 6 },
  chip: { marginRight: 4 },
  badge: { color: 'white' },
  emptyContainer: { flexGrow: 1, justifyContent: 'center' },
  empty: { textAlign: 'center', opacity: 0.6, padding: 24 },
  fab: { position: 'absolute', right: 16, bottom: 16 },
});
