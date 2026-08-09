import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, View } from 'react-native';
import { ActivityIndicator, Button, Chip, Divider, Text } from 'react-native-paper';
import { getStockLedger } from '../../core/stock/stock.api';
import type { StockLedgerEntry } from '../../core/stock/stock.types';
import { getWarehouses } from '../../core/warehouses/warehouse.api';
import type { Warehouse } from '../../core/warehouses/warehouse.types';

const PAGE_SIZE = 25;

const MOVEMENT_LABELS: Record<StockLedgerEntry['movementType'], string> = {
  PurchaseReceipt: 'Purchase receipt',
  PurchaseReturnIssue: 'Purchase return',
  SaleIssue: 'Sale',
  SaleReturnReceipt: 'Sale return',
  TransferOut: 'Transfer out',
  TransferIn: 'Transfer in',
  AdjustmentIncrease: 'Adjustment +',
  AdjustmentDecrease: 'Adjustment -',
  OpeningBalance: 'Opening balance',
};

export default function StockLedgerScreen() {
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [warehouseId, setWarehouseId] = useState<number | null>(null);
  const [entries, setEntries] = useState<StockLedgerEntry[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getWarehouses()
      .then(setWarehouses)
      .catch(() => {});
  }, []);

  const load = useCallback(
    async (page: number, append: boolean) => {
      if (append) {
        setLoadingMore(true);
      } else {
        setLoading(true);
      }
      setError(null);
      try {
        const result = await getStockLedger({
          warehouseId: warehouseId ?? undefined,
          pageNumber: page,
          pageSize: PAGE_SIZE,
        });
        setEntries((current) => (append ? [...current, ...result.items] : result.items));
        setTotalCount(result.totalCount);
        setPageNumber(page);
      } catch {
        setError('Unable to load stock movements. Pull to retry.');
      } finally {
        setLoading(false);
        setLoadingMore(false);
      }
    },
    [warehouseId],
  );

  useEffect(() => {
    void load(1, false);
  }, [load]);

  const hasMore = entries.length < totalCount;

  return (
    <View style={styles.container}>
      <FlatList
        horizontal
        data={[{ id: null, label: 'All warehouses' }, ...warehouses.map((w) => ({ id: w.id, label: w.name }))]}
        keyExtractor={(item) => String(item.id)}
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.chipRow}
        style={styles.chipList}
        renderItem={({ item }) => (
          <Chip selected={warehouseId === item.id} onPress={() => setWarehouseId(item.id)} style={styles.chip}>
            {item.label}
          </Chip>
        )}
      />

      <Divider />

      {loading && entries.length === 0 ? (
        <ActivityIndicator style={styles.loading} />
      ) : (
        <FlatList
          data={entries}
          keyExtractor={(item) => String(item.id)}
          refreshControl={<RefreshControl refreshing={loading} onRefresh={() => load(1, false)} />}
          ItemSeparatorComponent={Divider}
          onEndReachedThreshold={0.4}
          onEndReached={() => {
            if (hasMore && !loadingMore && !loading) {
              void load(pageNumber + 1, true);
            }
          }}
          contentContainerStyle={entries.length === 0 ? styles.emptyContainer : undefined}
          ListEmptyComponent={
            <Text variant="bodyMedium" style={styles.empty}>
              {error ?? 'No stock movements yet.'}
            </Text>
          }
          ListFooterComponent={loadingMore ? <ActivityIndicator style={styles.footerLoading} /> : null}
          renderItem={({ item }) => {
            const qty = item.quantityIn > 0 ? `+${item.quantityIn}` : `-${item.quantityOut}`;
            return (
              <View style={styles.row}>
                <View style={styles.rowMain}>
                  <Text variant="titleSmall">{item.productName}</Text>
                  <Text variant="bodySmall" style={styles.muted}>
                    {item.productVariantName} · {item.warehouseName}
                  </Text>
                  <Text variant="bodySmall" style={styles.muted}>
                    {MOVEMENT_LABELS[item.movementType]} · {new Date(item.transactionDate).toLocaleDateString()}
                  </Text>
                </View>
                <View style={styles.rowEnd}>
                  <Text variant="titleSmall">{qty}</Text>
                  <Text variant="bodySmall" style={styles.muted}>
                    bal: {item.runningQuantity}
                  </Text>
                </View>
              </View>
            );
          }}
        />
      )}

      {!loading && hasMore && !loadingMore ? (
        <Button onPress={() => load(pageNumber + 1, true)} style={styles.loadMore}>
          Load more
        </Button>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  chipList: { flexGrow: 0, paddingVertical: 12 },
  chipRow: { paddingHorizontal: 12 },
  chip: { marginRight: 8 },
  loading: { marginTop: 32 },
  footerLoading: { marginVertical: 16 },
  row: { flexDirection: 'row', justifyContent: 'space-between', padding: 16 },
  rowMain: { flex: 1, paddingRight: 12 },
  rowEnd: { alignItems: 'flex-end' },
  muted: { opacity: 0.6, marginTop: 2 },
  emptyContainer: { flexGrow: 1, justifyContent: 'center' },
  empty: { textAlign: 'center', opacity: 0.6, padding: 24 },
  loadMore: { margin: 8 },
});
