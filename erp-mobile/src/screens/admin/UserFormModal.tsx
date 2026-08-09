import React, { useEffect, useState } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { Button, HelperText, Modal, Portal, Switch, Text, TextInput } from 'react-native-paper';
import { getApiErrorMessage } from '../../core/api/error';
import { useAuth } from '../../core/auth/auth.context';
import type { Branch } from '../../core/branches/branch.types';
import type { RoleOption } from '../../core/roles/role.types';
import { MultiSelectField } from '../../core/ui/MultiSelectField';
import { SelectField } from '../../core/ui/SelectField';
import { activateUser, createUser, deactivateUser, grantBranch, revokeBranch, updateUser } from '../../core/users/user.api';
import type { AdminUser } from '../../core/users/user.types';

interface UserFormModalProps {
  visible: boolean;
  /** null = create mode, otherwise editing this user. */
  user: AdminUser | null;
  roles: RoleOption[];
  branches: Branch[];
  onDismiss: () => void;
  onSaved: () => void;
}

export default function UserFormModal({ visible, user, roles, branches, onDismiss, onSaved }: UserFormModalProps) {
  const { role: currentUserRole } = useAuth();
  const isEditing = user !== null;

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [roleId, setRoleId] = useState<number | null>(null);
  const [isActive, setIsActive] = useState(true);
  const [branchIds, setBranchIds] = useState<number[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (visible) {
      setUsername(user?.username ?? '');
      setPassword('');
      setRoleId(user?.roleId ?? null);
      setIsActive(user?.isActive ?? true);
      setBranchIds(user?.branches.map((b) => b.id) ?? []);
      setError(null);
    }
  }, [visible, user]);

  // Only a SystemAdmin may assign the SystemAdmin role (enforced server-side in
  // Users/UserRepository.cs) — hide it here too so a non-SystemAdmin doesn't hit a 403.
  const roleOptions = roles
    .filter((r) => currentUserRole === 'SystemAdmin' || r.name !== 'SystemAdmin')
    .map((r) => ({ id: r.id, label: r.name }));

  const branchOptions = branches.map((b) => ({ id: b.id, label: `${b.name} (${b.code})` }));

  const canSave = username.trim().length > 0 && roleId !== null && (isEditing || password.length > 0) && !saving;

  const onSave = async () => {
    setSaving(true);
    setError(null);
    try {
      if (!isEditing) {
        await createUser({ username: username.trim(), password, roleId: roleId!, branchIds });
      } else {
        await updateUser(user.id, { roleId: roleId!, isActive });
        if (isActive !== user.isActive) {
          await (isActive ? activateUser(user.id) : deactivateUser(user.id));
        }
        const before = new Set(user.branches.map((b) => b.id));
        const after = new Set(branchIds);
        const toGrant = branchIds.filter((id) => !before.has(id));
        const toRevoke = [...before].filter((id) => !after.has(id));
        await Promise.all([
          ...toGrant.map((id) => grantBranch(user.id, id)),
          ...toRevoke.map((id) => revokeBranch(user.id, id)),
        ]);
      }
      onSaved();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Unable to save user.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Portal>
      <Modal visible={visible} onDismiss={onDismiss} contentContainerStyle={styles.modal}>
        <Text variant="titleLarge" style={styles.title}>
          {isEditing ? `Edit ${user.username}` : 'New user'}
        </Text>
        <ScrollView style={styles.scroll}>
          <TextInput
            label="Username"
            value={username}
            onChangeText={setUsername}
            autoCapitalize="none"
            editable={!isEditing}
            style={styles.field}
          />
          {!isEditing ? (
            <TextInput
              label="Password"
              value={password}
              onChangeText={setPassword}
              secureTextEntry
              style={styles.field}
            />
          ) : null}
          <SelectField label="Role" value={roleId} options={roleOptions} onChange={setRoleId} />

          {isEditing ? (
            <View style={styles.activeRow}>
              <Text variant="bodyMedium">Active</Text>
              <Switch value={isActive} onValueChange={setIsActive} />
            </View>
          ) : null}

          <MultiSelectField label="Branch access" values={branchIds} options={branchOptions} onChange={setBranchIds} />

          {error ? (
            <HelperText type="error" visible>
              {error}
            </HelperText>
          ) : null}
        </ScrollView>

        <View style={styles.actions}>
          <Button onPress={onDismiss}>Cancel</Button>
          <Button mode="contained" onPress={onSave} loading={saving} disabled={!canSave}>
            Save
          </Button>
        </View>
      </Modal>
    </Portal>
  );
}

const styles = StyleSheet.create({
  modal: { backgroundColor: 'white', margin: 16, borderRadius: 12, padding: 16, maxHeight: '90%' },
  title: { marginBottom: 12 },
  scroll: { flexGrow: 0 },
  field: { marginBottom: 12 },
  activeRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 },
  actions: { flexDirection: 'row', justifyContent: 'flex-end', gap: 8, marginTop: 12 },
});
