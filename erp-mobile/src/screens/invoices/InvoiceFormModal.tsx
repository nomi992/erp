import React, { useMemo, useState } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { Button, HelperText, IconButton, Modal, Portal, SegmentedButtons, Text, TextInput } from 'react-native-paper';
import { getApiErrorMessage } from '../../core/api/error';
import type { BusinessPartner } from '../../core/business-partners/partner.types';
import { createInvoice } from '../../core/invoices/invoice.api';
import type { PaymentMode } from '../../core/invoices/invoice.types';
import type { ProductVariantOption } from '../../core/products/product.types';
import type { TaxRate } from '../../core/tax-rates/tax-rate.types';
import { SelectField } from '../../core/ui/SelectField';
import { todayIsoDate } from '../../core/util/date';
import type { Warehouse } from '../../core/warehouses/warehouse.types';

const NO_TAX = 0;

interface InvoiceLineDraft {
  variantId: number;
  qty: number;
  unitAmount: number;
  taxRateId: number;
}

interface InvoiceFormModalProps {
  visible: boolean;
  customers: BusinessPartner[];
  warehouses: Warehouse[];
  variants: ProductVariantOption[];
  taxRates: TaxRate[];
  onDismiss: () => void;
  onCreated: () => void;
}

function emptyLine(): InvoiceLineDraft {
  return { variantId: 0, qty: 1, unitAmount: 0, taxRateId: NO_TAX };
}

export default function InvoiceFormModal({
  visible,
  customers,
  warehouses,
  variants,
  taxRates,
  onDismiss,
  onCreated,
}: InvoiceFormModalProps) {
  const [partnerId, setPartnerId] = useState<number | null>(null);
  const [warehouseId, setWarehouseId] = useState<number | null>(null);
  const [date, setDate] = useState(todayIsoDate());
  const [paymentMode, setPaymentMode] = useState<PaymentMode>('Cash');
  const [paymentTermDays, setPaymentTermDays] = useState('0');
  const [narration, setNarration] = useState('');
  const [lines, setLines] = useState<InvoiceLineDraft[]>([emptyLine()]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const reset = () => {
    setPartnerId(null);
    setWarehouseId(null);
    setDate(todayIsoDate());
    setPaymentMode('Cash');
    setPaymentTermDays('0');
    setNarration('');
    setLines([emptyLine()]);
    setError(null);
  };

  const customerOptions = customers
    .filter((c) => c.isActive)
    .map((c) => ({ id: c.id, label: `${c.code} · ${c.name}` }));
  const warehouseOptions = warehouses.filter((w) => w.isActive).map((w) => ({ id: w.id, label: w.name }));
  const variantOptions = variants.map((v) => ({ id: v.variantId, label: v.label, sublabel: v.variantCode }));
  const taxRateOptions = [
    { id: NO_TAX, label: 'No tax' },
    ...taxRates.filter((t) => t.isActive).map((t) => ({ id: t.id, label: `${t.name} (${t.percentage}%)` })),
  ];

  const updateLine = (index: number, patch: Partial<InvoiceLineDraft>) => {
    setLines((current) => current.map((line, i) => (i === index ? { ...line, ...patch } : line)));
  };

  const pickVariant = (index: number, variantId: number) => {
    const variant = variants.find((v) => v.variantId === variantId);
    updateLine(index, { variantId, unitAmount: variant?.salePrice ?? 0 });
  };

  const removeLine = (index: number) => setLines((current) => current.filter((_, i) => i !== index));

  const totals = useMemo(() => {
    return lines.reduce(
      (acc, line) => {
        const net = line.qty * line.unitAmount;
        const rate = taxRates.find((t) => t.id === line.taxRateId)?.percentage ?? 0;
        const tax = (net * rate) / 100;
        return { net: acc.net + net, tax: acc.tax + tax };
      },
      { net: 0, tax: 0 },
    );
  }, [lines, taxRates]);

  const linesValid = lines.length > 0 && lines.every((l) => l.variantId > 0 && l.qty > 0 && l.unitAmount >= 0);
  const canSave = partnerId !== null && warehouseId !== null && linesValid && !saving;

  const onSave = async () => {
    setSaving(true);
    setError(null);
    try {
      await createInvoice({
        invoiceType: 'SalesInvoice',
        externalReferenceNo: null,
        partnerId: partnerId!,
        referenceInvoiceId: null,
        warehouseId: warehouseId!,
        date,
        paymentMode,
        paymentTermDays: paymentMode === 'Credit' ? Number(paymentTermDays) || 0 : 0,
        requestedDeliveryDate: null,
        narration: narration || null,
        lines: lines.map((line) => {
          const variant = variants.find((v) => v.variantId === line.variantId)!;
          return {
            productVariantId: line.variantId,
            unitOfMeasureId: variant.unitOfMeasureId,
            qty: line.qty,
            unitAmount: line.unitAmount,
            taxRateId: line.taxRateId === NO_TAX ? null : line.taxRateId,
            referenceInvoiceLineId: null,
          };
        }),
      });
      reset();
      onCreated();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Unable to save invoice.'));
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
          New Sales Invoice
        </Text>
        <ScrollView style={styles.scroll}>
          <SelectField label="Customer" value={partnerId} options={customerOptions} onChange={setPartnerId} />
          <SelectField label="Warehouse" value={warehouseId} options={warehouseOptions} onChange={setWarehouseId} />
          <TextInput label="Date (YYYY-MM-DD)" value={date} onChangeText={setDate} style={styles.field} />

          <SegmentedButtons
            value={paymentMode}
            onValueChange={(value) => setPaymentMode(value as PaymentMode)}
            style={styles.field}
            buttons={[
              { value: 'Cash', label: 'Cash' },
              { value: 'Credit', label: 'Credit' },
            ]}
          />
          {paymentMode === 'Credit' ? (
            <TextInput
              label="Payment term (days)"
              value={paymentTermDays}
              onChangeText={setPaymentTermDays}
              keyboardType="number-pad"
              style={styles.field}
            />
          ) : null}

          <TextInput label="Narration" value={narration} onChangeText={setNarration} multiline style={styles.field} />

          <Text variant="titleSmall" style={styles.linesTitle}>
            Lines
          </Text>
          {lines.map((line, index) => (
            <View key={index} style={styles.lineCard}>
              <View style={styles.lineHeader}>
                <Text variant="labelLarge">Line {index + 1}</Text>
                {lines.length > 1 ? <IconButton icon="close" size={18} onPress={() => removeLine(index)} /> : null}
              </View>
              <SelectField
                label="Product"
                value={line.variantId || null}
                options={variantOptions}
                onChange={(id) => pickVariant(index, id)}
              />
              <View style={styles.amountRow}>
                <TextInput
                  label="Qty"
                  value={String(line.qty)}
                  onChangeText={(text) => updateLine(index, { qty: Number(text) || 0 })}
                  keyboardType="decimal-pad"
                  style={styles.amountField}
                />
                <TextInput
                  label="Unit price"
                  value={String(line.unitAmount)}
                  onChangeText={(text) => updateLine(index, { unitAmount: Number(text) || 0 })}
                  keyboardType="decimal-pad"
                  style={styles.amountField}
                />
              </View>
              <SelectField
                label="Tax"
                value={line.taxRateId}
                options={taxRateOptions}
                onChange={(id) => updateLine(index, { taxRateId: id })}
              />
              <Text variant="bodySmall" style={styles.lineTotal}>
                Line total: {(line.qty * line.unitAmount).toFixed(2)}
              </Text>
            </View>
          ))}
          <Button icon="plus" onPress={() => setLines((current) => [...current, emptyLine()])} style={styles.addLine}>
            Add line
          </Button>

          <View style={styles.totalsRow}>
            <Text variant="bodyMedium">Net: {totals.net.toFixed(2)}</Text>
            <Text variant="bodyMedium">Tax: {totals.tax.toFixed(2)}</Text>
            <Text variant="titleSmall">Total: {(totals.net + totals.tax).toFixed(2)}</Text>
          </View>

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
  lineTotal: { textAlign: 'right', opacity: 0.7, marginTop: 4 },
  addLine: { alignSelf: 'flex-start', marginBottom: 8 },
  totalsRow: { gap: 2, marginBottom: 8, alignItems: 'flex-end' },
  actions: { flexDirection: 'row', justifyContent: 'flex-end', gap: 8, marginTop: 12 },
});
