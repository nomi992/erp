import React, { useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import { Button, Checkbox, Chip, Divider, List, Modal, Portal, Text } from 'react-native-paper';
import type { SelectOption } from './SelectField';

interface MultiSelectFieldProps {
  label: string;
  values: number[];
  options: SelectOption[];
  onChange: (ids: number[]) => void;
  disabled?: boolean;
}

/** Multi-select variant of SelectField — used for branch access grants on the Users screen. */
export function MultiSelectField({ label, values, options, onChange, disabled }: MultiSelectFieldProps) {
  const [visible, setVisible] = useState(false);
  const [draft, setDraft] = useState<number[]>(values);

  const open = () => {
    if (disabled) {
      return;
    }
    setDraft(values);
    setVisible(true);
  };

  const toggle = (id: number) => {
    setDraft((current) => (current.includes(id) ? current.filter((v) => v !== id) : [...current, id]));
  };

  const confirm = () => {
    onChange(draft);
    setVisible(false);
  };

  const selectedOptions = options.filter((o) => values.includes(o.id));

  return (
    <View style={styles.container}>
      <Text variant="labelLarge" style={styles.label}>
        {label}
      </Text>
      <View style={styles.chips}>
        {selectedOptions.length > 0 ? (
          selectedOptions.map((o) => <Chip key={o.id}>{o.label}</Chip>)
        ) : (
          <Text variant="bodyMedium" style={styles.placeholder}>
            None selected
          </Text>
        )}
        <Chip icon="pencil" onPress={open} disabled={disabled}>
          Edit
        </Chip>
      </View>

      <Portal>
        <Modal visible={visible} onDismiss={() => setVisible(false)} contentContainerStyle={styles.modal}>
          <Text variant="titleMedium" style={styles.title}>
            {label}
          </Text>
          <FlatList
            data={options}
            keyExtractor={(item) => String(item.id)}
            style={styles.list}
            ItemSeparatorComponent={Divider}
            renderItem={({ item }) => (
              <List.Item
                title={item.label}
                description={item.sublabel}
                onPress={() => toggle(item.id)}
                left={() => (
                  <Checkbox
                    status={draft.includes(item.id) ? 'checked' : 'unchecked'}
                    onPress={() => toggle(item.id)}
                  />
                )}
              />
            )}
          />
          <Button mode="contained" onPress={confirm} style={styles.confirm}>
            Done
          </Button>
        </Modal>
      </Portal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { marginBottom: 12 },
  label: { marginBottom: 8 },
  chips: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, alignItems: 'center' },
  placeholder: { opacity: 0.6 },
  modal: { backgroundColor: 'white', margin: 24, borderRadius: 12, maxHeight: '75%', padding: 16 },
  title: { marginBottom: 12 },
  list: { flexGrow: 0, marginBottom: 12 },
  confirm: { alignSelf: 'flex-end' },
});
