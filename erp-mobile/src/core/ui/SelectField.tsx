import React, { useMemo, useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import { Divider, List, Modal, Portal, Text, TextInput } from 'react-native-paper';

export interface SelectOption {
  id: number;
  label: string;
  sublabel?: string;
}

interface SelectFieldProps {
  label: string;
  value: number | null;
  options: SelectOption[];
  onChange: (id: number) => void;
  placeholder?: string;
  disabled?: boolean;
  error?: string;
}

/**
 * A tap-to-open picker modal, standing in for the web app's PrimeNG dropdowns (Account,
 * Role, Branch, Partner, Warehouse, Product Variant, Unit of Measure, Tax Rate, ...).
 * No dedicated native picker dependency — just a searchable list in a Portal-rendered modal.
 */
export function SelectField({ label, value, options, onChange, placeholder, disabled, error }: SelectFieldProps) {
  const [visible, setVisible] = useState(false);
  const [query, setQuery] = useState('');

  const selected = options.find((o) => o.id === value);
  const filtered = useMemo(() => {
    if (!query.trim()) {
      return options;
    }
    const q = query.trim().toLowerCase();
    return options.filter((o) => o.label.toLowerCase().includes(q) || o.sublabel?.toLowerCase().includes(q));
  }, [options, query]);

  const open = () => {
    if (!disabled) {
      setVisible(true);
    }
  };

  const close = () => {
    setVisible(false);
    setQuery('');
  };

  return (
    <>
      <TextInput
        label={label}
        value={selected?.label ?? ''}
        placeholder={placeholder ?? 'Tap to select'}
        editable={false}
        onPressIn={open}
        error={!!error}
        right={<TextInput.Icon icon="menu-down" onPress={open} forceTextInputFocus={false} />}
        style={styles.input}
      />
      {error ? (
        <Text variant="bodySmall" style={styles.error}>
          {error}
        </Text>
      ) : null}
      <Portal>
        <Modal visible={visible} onDismiss={close} contentContainerStyle={styles.modal}>
          <Text variant="titleMedium" style={styles.title}>
            {label}
          </Text>
          <TextInput
            placeholder="Search…"
            value={query}
            onChangeText={setQuery}
            mode="outlined"
            dense
            style={styles.search}
          />
          <FlatList
            data={filtered}
            keyExtractor={(item) => String(item.id)}
            style={styles.list}
            ItemSeparatorComponent={Divider}
            ListEmptyComponent={
              <Text variant="bodyMedium" style={styles.empty}>
                No matches.
              </Text>
            }
            renderItem={({ item }) => (
              <List.Item
                title={item.label}
                description={item.sublabel}
                onPress={() => {
                  onChange(item.id);
                  close();
                }}
              />
            )}
          />
        </Modal>
      </Portal>
    </>
  );
}

const styles = StyleSheet.create({
  input: { marginBottom: 4 },
  error: { color: '#B3261E', marginBottom: 8, marginLeft: 4 },
  modal: { backgroundColor: 'white', margin: 24, borderRadius: 12, maxHeight: '75%', padding: 16 },
  title: { marginBottom: 12 },
  search: { marginBottom: 8 },
  list: { flexGrow: 0 },
  empty: { textAlign: 'center', padding: 16, opacity: 0.6 },
});
