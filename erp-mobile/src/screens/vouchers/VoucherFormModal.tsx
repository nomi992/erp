import React, { useState } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { Button, Divider, HelperText, IconButton, Modal, Portal, Text, TextInput } from 'react-native-paper';
import { getApiErrorMessage } from '../../core/api/error';
import type { Account } from '../../core/accounts/account.types';
import { SelectField } from '../../core/ui/SelectField';
import { todayIsoDate } from '../../core/util/date';
import { createVoucher } from '../../core/vouchers/voucher.api';
import type { VoucherLineRequest, VoucherType } from '../../core/vouchers/voucher.types';

interface VoucherFormModalProps {
  visible: boolean;
  voucherType: VoucherType;
  accounts: Account[];
  onDismiss: () => void;
  onCreated: () => void;
}

function emptyLine(): VoucherLineRequest {
  return { accountId: 0, debitAmount: 0, creditAmount: 0, costCenterId: null, taxRateId: null };
}

export default function VoucherFormModal({ visible, voucherType, accounts, onDismiss, onCreated }: VoucherFormModalProps) {
  const [date, setDate] = useState(todayIsoDate());
  const [narration, setNarration] = useState('');
  const [lines, setLines] = useState<VoucherLineRequest[]>([emptyLine(), emptyLine()]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const reset = () => {
    setDate(todayIsoDate());
    setNarration('');
    setLines([emptyLine(), emptyLine()]);
    setError(null);
  };

  const updateLine = (index: number, patch: Partial<VoucherLineRequest>) => {
    setLines((current) => current.map((line, i) => (i === index ? { ...line, ...patch } : line)));
  };

  const removeLine = (index: number) => {
    setLines((current) => current.filter((_, i) => i !== index));
  };

  const totalDebit = lines.reduce((sum, l) => sum + (l.debitAmount || 0), 0);
  const totalCredit = lines.reduce((sum, l) => sum + (l.creditAmount || 0), 0);
  const isBalanced = totalDebit > 0 && Math.abs(totalDebit - totalCredit) < 0.01;
  const linesValid = lines.length >= 2 && lines.every((l) => l.accountId > 0 && (l.debitAmount > 0 || l.creditAmount > 0));

  const canSave = isBalanced && linesValid && !saving;

  const accountOptions = accounts
    .filter((a) => a.isActive)
    .map((a) => ({ id: a.id, label: `${a.code} · ${a.name}`, sublabel: a.type }));

  const onSave = async () => {
    setSaving(true);
    setError(null);
    try {
      await createVoucher({
        voucherType,
        date,
        narration,
        currencyCode: 'USD',
        exchangeRate: 1,
        lines,
      });
      reset();
      onCreated();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Unable to save voucher.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Portal>
      <Modal
        visible={visible}
        onDismiss={() => {
          reset();
          onDismiss();
        }}
        contentContainerStyle={styles.modal}
      >
        <Text variant="titleLarge" style={styles.title}>
          New {voucherType} Voucher
        </Text>
        <ScrollView style={styles.scroll}>
          <TextInput label="Date (YYYY-MM-DD)" value={date} onChangeText={setDate} style={styles.field} />
          <TextInput
            label="Narration"
            value={narration}
            onChangeText={setNarration}
            multiline
            style={styles.field}
          />

          <Text variant="titleSmall" style={styles.linesTitle}>
            Lines
          </Text>
          {lines.map((line, index) => (
            <View key={index} style={styles.lineCard}>
              <View style={styles.lineHeader}>
                <Text variant="labelLarge">Line {index + 1}</Text>
                {lines.length > 2 ? (
                  <IconButton icon="close" size={18} onPress={() => removeLine(index)} />
                ) : null}
              </View>
              <SelectField
                label="Account"
                value={line.accountId || null}
                options={accountOptions}
                onChange={(id) => updateLine(index, { accountId: id })}
              />
              <View style={styles.amountRow}>
                <TextInput
                  label="Debit"
                  value={line.debitAmount ? String(line.debitAmount) : ''}
                  onChangeText={(text) => updateLine(index, { debitAmount: Number(text) || 0, creditAmount: 0 })}
                  keyboardType="decimal-pad"
                  style={styles.amountField}
                />
                <TextInput
                  label="Credit"
                  value={line.creditAmount ? String(line.creditAmount) : ''}
                  onChangeText={(text) => updateLine(index, { creditAmount: Number(text) || 0, debitAmount: 0 })}
                  keyboardType="decimal-pad"
                  style={styles.amountField}
                />
              </View>
            </View>
          ))}
          <Button icon="plus" onPress={() => setLines((current) => [...current, emptyLine()])} style={styles.addLine}>
            Add line
          </Button>

          <Divider style={styles.divider} />
          <View style={styles.totalsRow}>
            <Text variant="bodyMedium">Total debit: {totalDebit.toFixed(2)}</Text>
            <Text variant="bodyMedium">Total credit: {totalCredit.toFixed(2)}</Text>
          </View>
          {!isBalanced && (lines[0].debitAmount || lines[0].creditAmount) ? (
            <HelperText type="error" visible>
              Debit and credit totals must match before saving (double-entry).
            </HelperText>
          ) : null}
          {error ? (
            <HelperText type="error" visible>
              {error}
            </HelperText>
          ) : null}
        </ScrollView>

        <View style={styles.actions}>
          <Button
            onPress={() => {
              reset();
              onDismiss();
            }}
          >
            Cancel
          </Button>
          <Button mode="contained" onPress={onSave} loading={saving} disabled={!canSave}>
            Save as draft
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
  linesTitle: { marginTop: 4, marginBottom: 8 },
  lineCard: { borderWidth: 1, borderColor: '#ddd', borderRadius: 8, padding: 12, marginBottom: 12 },
  lineHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  amountRow: { flexDirection: 'row', gap: 12 },
  amountField: { flex: 1 },
  addLine: { alignSelf: 'flex-start', marginBottom: 8 },
  divider: { marginVertical: 8 },
  totalsRow: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 4 },
  actions: { flexDirection: 'row', justifyContent: 'flex-end', gap: 8, marginTop: 12 },
});
