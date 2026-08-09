import React from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { Divider, Modal, Portal, Text } from 'react-native-paper';
import type { Voucher } from '../../core/vouchers/voucher.types';

interface VoucherDetailModalProps {
  voucher: Voucher | null;
  onDismiss: () => void;
}

export default function VoucherDetailModal({ voucher, onDismiss }: VoucherDetailModalProps) {
  return (
    <Portal>
      <Modal visible={voucher !== null} onDismiss={onDismiss} contentContainerStyle={styles.modal}>
        {voucher ? (
          <ScrollView>
            <Text variant="titleLarge">{voucher.voucherNo}</Text>
            <Text variant="bodyMedium" style={styles.muted}>
              {voucher.voucherType} · {voucher.status} · {voucher.date}
            </Text>
            {voucher.narration ? <Text style={styles.narration}>{voucher.narration}</Text> : null}

            <Divider style={styles.divider} />

            {voucher.lines.map((line) => (
              <View key={line.id} style={styles.line}>
                <View style={styles.lineMain}>
                  <Text variant="bodyMedium">{line.accountName}</Text>
                  <Text variant="bodySmall" style={styles.muted}>
                    {line.accountCode}
                  </Text>
                </View>
                <View style={styles.lineEnd}>
                  <Text variant="bodyMedium">{line.debitAmount > 0 ? line.debitAmount.toFixed(2) : ''}</Text>
                  <Text variant="bodyMedium">{line.creditAmount > 0 ? line.creditAmount.toFixed(2) : ''}</Text>
                </View>
              </View>
            ))}

            <Divider style={styles.divider} />
            <View style={styles.totalsRow}>
              <Text variant="titleSmall">Total</Text>
              <View style={styles.lineEnd}>
                <Text variant="titleSmall">{voucher.totalDebit.toFixed(2)}</Text>
                <Text variant="titleSmall">{voucher.totalCredit.toFixed(2)}</Text>
              </View>
            </View>
          </ScrollView>
        ) : null}
      </Modal>
    </Portal>
  );
}

const styles = StyleSheet.create({
  modal: { backgroundColor: 'white', margin: 16, borderRadius: 12, padding: 16, maxHeight: '80%' },
  muted: { opacity: 0.6, marginTop: 2, marginBottom: 8 },
  narration: { marginBottom: 8, fontStyle: 'italic' },
  divider: { marginVertical: 8 },
  line: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: 6 },
  lineMain: { flex: 1, paddingRight: 12 },
  lineEnd: { flexDirection: 'row', gap: 16, minWidth: 120, justifyContent: 'flex-end' },
  totalsRow: { flexDirection: 'row', justifyContent: 'space-between' },
});
