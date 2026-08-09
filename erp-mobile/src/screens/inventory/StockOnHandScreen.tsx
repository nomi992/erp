import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, View } from 'react-native';
import { ActivityIndicator, Badge, Chip, Divider, Switch, Text } from 'react-native-paper';
import { getStockOnHand } from '../../core/stock/stock.api';
import type { StockOnHand } from '../../core/stock/stock.types';
import { getWarehouses } from '../../core/warehouses/warehouse.api';
import type { Warehouse } from '../../core/warehouses/warehouse.types';

export default function StockOnHandScreen() {
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [warehouseId, setWarehouseId] = useState<number | null>(null);
  const [lowStockOnly, setLowStockOnly] = useState(false);
  const [items, setItems] = useState<StockOnHand[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getWarehouses()
      .then(setWarehouses)
      .catch(() => {
        /* filter row just degrades to "All" if this fails — not fatal */
      });
  }, []);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getStockOnHand({
        warehouseId: warehouseId ?? undefined,
        lowStockOnly,
      });
      setItems(data);
    } catch {
      setError('Unable to load stock levels. Pull to retry.');
    } finally {
      setLoading(false);
    }
  }, [warehouseId, lowStockOnly]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <View style={styles.container}>
      <View style={styles.filters}>
        <FlatList
          horizontal
          data={[{ id: null, label: 'All warehouses' }, ...warehouses.map((w) => ({ id: w.id, label: w.name }))]}
          keyExtractor={(item) => String(item.id)}
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.chipRow}
          renderItem={({ item }) => (
            <Chip
              selected={warehouseId === item.id}
              onPress={() => setWarehouseId(item.id)}
              style={styles.chip}
            >
              {item.label}
            </Chip>
          )}
        />
        <View style={styles.lowStockRow}>
          <Text variant="bodyMedium">Low stock only</Text>
          <Switch value={lowStockOnly} onValueChange={setLowStockOnly} />
        </View>
      </View>

      <Divider />

      {loading && items.length === 0 ? (
        <ActivityIndicator style={styles.loading} />
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => `${item.productVariantId}-${item.warehouseId}`}
          refreshControl={<RefreshControl refreshing={loading} onRefresh={load} />}
          ItemSeparatorComponent={Divider}
          contentContainerStyle={items.length === 0 ? styles.emptyContainer : undefined}
          ListEmptyComponent={
            <Text variant="bodyMedium" style={styles.empty}>
              {error ?? 'No stock records match this filter.'}
            </Text>
          }
          renderItem={({ item }) => (
            <View style={styles.row}>
              <View style={styles.rowMain}>
                <Text variant="titleSmall">{item.productName}</Text>
                <Text variant="bodySmall" style={styles.muted}>
                  {item.productVariantName} · {item.warehouseName}
                </Text>
              </View>
              <View style={styles.rowEnd}>
                <Text variant="titleSmall">{item.quantityOnHand}</Text>
                <Text variant="bodySmall" style={styles.muted}>
                  {item.stockValue.toFixed(2)}
                </Text>
                {item.isLowStock ? <Badge style={styles.badge}>Low</Badge> : null}
              </View>
            </View>
          )}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  filters: { paddingTop: 12 },
  chipRow: { paddingHorizontal: 12, gap: 8 },
  chip: { marginRight: 8 },
  lowStockRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 8,
  },
  loading: { marginTop: 32 },
  row: { flexDirection: 'row', justifyContent: 'space-between', padding: 16 },
  rowMain: { flex: 1, paddingRight: 12 },
  rowEnd: { alignItems: 'flex-end' },
  muted: { opacity: 0.6, marginTop: 2 },
  badge: { marginTop: 4 },
  emptyContainer: { flexGrow: 1, justifyContent: 'center' },
  empty: { textAlign: 'center', opacity: 0.6, padding: 24 },
});
