import React from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { Divider, Modal, Portal, Text } from 'react-native-paper';
import type { Invoice } from '../../core/invoices/invoice.types';

interface InvoiceDetailModalProps {
  invoice: Invoice | null;
  onDismiss: () => void;
}

export default function InvoiceDetailModal({ invoice, onDismiss }: InvoiceDetailModalProps) {
  return (
    <Portal>
      <Modal visible={invoice !== null} onDismiss={onDismiss} contentContainerStyle={styles.modal}>
        {invoice ? (
          <ScrollView>
            <Text variant="titleLarge">{invoice.invoiceNo}</Text>
            <Text variant="bodyMedium" style={styles.muted}>
              {invoice.partnerName} · {invoice.warehouseName}
            </Text>
            <Text variant="bodyMedium" style={styles.muted}>
              {invoice.status} · {invoice.paymentStatus} · {invoice.paymentMode}
            </Text>
            <Text variant="bodyMedium" style={styles.muted}>
              {invoice.date} · due {invoice.dueDate}
            </Text>
            {invoice.narration ? <Text style={styles.narration}>{invoice.narration}</Text> : null}

            <Divider style={styles.divider} />

            {invoice.lines.map((line) => (
              <View key={line.id} style={styles.line}>
                <View style={styles.lineMain}>
                  <Text variant="bodyMedium">{line.productName}</Text>
                  <Text variant="bodySmall" style={styles.muted}>
                    {line.productVariantName} · {line.qty} {line.unitOfMeasureCode} @ {line.unitAmount.toFixed(2)}
                  </Text>
                </View>
                <Text variant="bodyMedium">{line.lineTotal.toFixed(2)}</Text>
              </View>
            ))}

            <Divider style={styles.divider} />
            <View style={styles.totalsRow}>
              <Text variant="bodyMedium">Net: {invoice.totalNet.toFixed(2)}</Text>
              <Text variant="bodyMedium">Tax: {invoice.totalTax.toFixed(2)}</Text>
              <Text variant="titleSmall">Total: {invoice.totalAmount.toFixed(2)}</Text>
              <Text variant="bodySmall" style={styles.muted}>
                Outstanding: {invoice.outstandingAmount.toFixed(2)}
              </Text>
            </View>
          </ScrollView>
        ) : null}
      </Modal>
    </Portal>
  );
}

const styles = StyleSheet.create({
  modal: { backgroundColor: 'white', margin: 16, borderRadius: 12, padding: 16, maxHeight: '80%' },
  muted: { opacity: 0.6, marginTop: 2 },
  narration: { marginVertical: 8, fontStyle: 'italic' },
  divider: { marginVertical: 8 },
  line: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: 6 },
  lineMain: { flex: 1, paddingRight: 12 },
  totalsRow: { alignItems: 'flex-end', gap: 2 },
});
