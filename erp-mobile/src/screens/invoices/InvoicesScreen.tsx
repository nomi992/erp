import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, View } from 'react-native';
import { ActivityIndicator, Badge, Divider, FAB, Text } from 'react-native-paper';
import { RightCode } from '../../core/auth/right-code';
import { useAuth } from '../../core/auth/auth.context';
import { getBusinessPartners } from '../../core/business-partners/partner.api';
import type { BusinessPartner } from '../../core/business-partners/partner.types';
import { getInvoiceById, getSalesInvoices } from '../../core/invoices/invoice.api';
import type { Invoice, InvoiceListItem } from '../../core/invoices/invoice.types';
import { flattenProductVariants } from '../../core/products/product.types';
import type { ProductVariantOption } from '../../core/products/product.types';
import { getProducts } from '../../core/products/product.api';
import { getTaxRates } from '../../core/tax-rates/tax-rate.api';
import type { TaxRate } from '../../core/tax-rates/tax-rate.types';
import { getWarehouses } from '../../core/warehouses/warehouse.api';
import type { Warehouse } from '../../core/warehouses/warehouse.types';
import InvoiceDetailModal from './InvoiceDetailModal';
import InvoiceFormModal from './InvoiceFormModal';

const STATUS_COLORS: Record<InvoiceListItem['status'], string> = {
  Draft: '#9E9E9E',
  PendingApproval: '#F2994A',
  Posted: '#27AE60',
  Rejected: '#EB5757',
  Cancelled: '#EB5757',
};

export default function InvoicesScreen() {
  const { hasRight } = useAuth();
  const [items, setItems] = useState<InvoiceListItem[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formVisible, setFormVisible] = useState(false);
  const [detailInvoice, setDetailInvoice] = useState<Invoice | null>(null);

  const [customers, setCustomers] = useState<BusinessPartner[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [variants, setVariants] = useState<ProductVariantOption[]>([]);
  const [taxRates, setTaxRates] = useState<TaxRate[]>([]);

  const load = useCallback(async (page: number, append: boolean) => {
    if (append) {
      setLoadingMore(true);
    } else {
      setLoading(true);
    }
    setError(null);
    try {
      const result = await getSalesInvoices(page);
      setItems((current) => (append ? [...current, ...result.items] : result.items));
      setTotalCount(result.totalCount);
      setPageNumber(page);
    } catch {
      setError('Unable to load invoices. Pull to retry.');
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, []);

  useEffect(() => {
    void load(1, false);
  }, [load]);

  // Reference data for the create form — fetched once, up front, so opening the form is instant.
  useEffect(() => {
    getBusinessPartners('Customer')
      .then(setCustomers)
      .catch(() => {});
    getWarehouses()
      .then(setWarehouses)
      .catch(() => {});
    getProducts()
      .then((products) => setVariants(flattenProductVariants(products)))
      .catch(() => {});
    getTaxRates()
      .then(setTaxRates)
      .catch(() => {});
  }, []);

  const openDetail = async (id: number) => {
    try {
      setDetailInvoice(await getInvoiceById(id));
    } catch {
      // silently ignore — user can just retry the tap
    }
  };

  const canCreate = hasRight(RightCode.SalesInvoicesCreate);
  const hasMore = items.length < totalCount;

  return (
    <View style={styles.container}>
      {loading && items.length === 0 ? (
        <ActivityIndicator style={styles.loading} />
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => String(item.id)}
          refreshControl={<RefreshControl refreshing={loading} onRefresh={() => load(1, false)} />}
          ItemSeparatorComponent={Divider}
          onEndReachedThreshold={0.4}
          onEndReached={() => {
            if (hasMore && !loadingMore && !loading) {
              void load(pageNumber + 1, true);
            }
          }}
          contentContainerStyle={items.length === 0 ? styles.emptyContainer : undefined}
          ListEmptyComponent={
            <Text variant="bodyMedium" style={styles.empty}>
              {error ?? 'No sales invoices yet.'}
            </Text>
          }
          ListFooterComponent={loadingMore ? <ActivityIndicator style={styles.footerLoading} /> : null}
          renderItem={({ item }) => (
            <View style={styles.row} onTouchEnd={() => openDetail(item.id)}>
              <View style={styles.rowMain}>
                <Text variant="titleSmall">{item.invoiceNo}</Text>
                <Text variant="bodySmall" style={styles.muted}>
                  {item.partnerName}
                </Text>
                <Text variant="bodySmall" style={styles.muted}>
                  {new Date(item.date).toLocaleDateString()}
                </Text>
              </View>
              <View style={styles.rowEnd}>
                <Text variant="titleSmall">{item.totalAmount.toFixed(2)}</Text>
                <Badge style={[styles.badge, { backgroundColor: STATUS_COLORS[item.status] }]}>{item.status}</Badge>
              </View>
            </View>
          )}
        />
      )}

      {canCreate ? <FAB icon="plus" style={styles.fab} onPress={() => setFormVisible(true)} label="New invoice" /> : null}

      <InvoiceFormModal
        visible={formVisible}
        customers={customers}
        warehouses={warehouses}
        variants={variants}
        taxRates={taxRates}
        onDismiss={() => setFormVisible(false)}
        onCreated={() => {
          setFormVisible(false);
          void load(1, false);
        }}
      />

      <InvoiceDetailModal invoice={detailInvoice} onDismiss={() => setDetailInvoice(null)} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  loading: { marginTop: 32 },
  footerLoading: { marginVertical: 16 },
  row: { flexDirection: 'row', justifyContent: 'space-between', padding: 16 },
  rowMain: { flex: 1, paddingRight: 12 },
  rowEnd: { alignItems: 'flex-end', gap: 4 },
  muted: { opacity: 0.6, marginTop: 2 },
  badge: { color: 'white' },
  emptyContainer: { flexGrow: 1, justifyContent: 'center' },
  empty: { textAlign: 'center', opacity: 0.6, padding: 24 },
  fab: { position: 'absolute', right: 16, bottom: 16 },
});
