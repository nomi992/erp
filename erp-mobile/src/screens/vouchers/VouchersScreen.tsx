import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, View } from 'react-native';
import { ActivityIndicator, Badge, Divider, FAB, SegmentedButtons, Text } from 'react-native-paper';
import { getAccounts } from '../../core/accounts/account.api';
import type { Account } from '../../core/accounts/account.types';
import { RightCode } from '../../core/auth/right-code';
import { useAuth } from '../../core/auth/auth.context';
import { getVoucherById, getVouchers } from '../../core/vouchers/voucher.api';
import type { Voucher, VoucherListItem, VoucherType } from '../../core/vouchers/voucher.types';
import VoucherDetailModal from './VoucherDetailModal';
import VoucherFormModal from './VoucherFormModal';

const STATUS_COLORS: Record<VoucherListItem['status'], string> = {
  Draft: '#9E9E9E',
  PendingApproval: '#F2994A',
  Posted: '#27AE60',
  Rejected: '#EB5757',
};

export default function VouchersScreen() {
  const { hasRight } = useAuth();
  const [voucherType, setVoucherType] = useState<VoucherType>('Payment');
  const [items, setItems] = useState<VoucherListItem[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formVisible, setFormVisible] = useState(false);
  const [detailVoucher, setDetailVoucher] = useState<Voucher | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getVouchers({ type: voucherType });
      setItems(data);
    } catch {
      setError('Unable to load vouchers. Pull to retry.');
    } finally {
      setLoading(false);
    }
  }, [voucherType]);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    getAccounts()
      .then(setAccounts)
      .catch(() => {});
  }, []);

  const openDetail = async (id: number) => {
    try {
      const voucher = await getVoucherById(id);
      setDetailVoucher(voucher);
    } catch {
      // silently ignore — user can just retry the tap
    }
  };

  const canCreate = hasRight(RightCode.VouchersCreate);

  return (
    <View style={styles.container}>
      <SegmentedButtons
        value={voucherType}
        onValueChange={(value) => setVoucherType(value as VoucherType)}
        style={styles.segmented}
        buttons={[
          { value: 'Payment', label: 'Payment' },
          { value: 'Receipt', label: 'Receipt' },
        ]}
      />

      <Divider />

      {loading && items.length === 0 ? (
        <ActivityIndicator style={styles.loading} />
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => String(item.id)}
          refreshControl={<RefreshControl refreshing={loading} onRefresh={load} />}
          ItemSeparatorComponent={Divider}
          contentContainerStyle={items.length === 0 ? styles.emptyContainer : undefined}
          ListEmptyComponent={
            <Text variant="bodyMedium" style={styles.empty}>
              {error ?? `No ${voucherType.toLowerCase()} vouchers yet.`}
            </Text>
          }
          renderItem={({ item }) => (
            <View style={styles.row} onTouchEnd={() => openDetail(item.id)}>
              <View style={styles.rowMain}>
                <Text variant="titleSmall">{item.voucherNo}</Text>
                <Text variant="bodySmall" style={styles.muted} numberOfLines={1}>
                  {item.narration || '—'}
                </Text>
                <Text variant="bodySmall" style={styles.muted}>
                  {new Date(item.date).toLocaleDateString()}
                </Text>
              </View>
              <View style={styles.rowEnd}>
                <Text variant="titleSmall">{item.totalDebit.toFixed(2)}</Text>
                <Badge style={[styles.badge, { backgroundColor: STATUS_COLORS[item.status] }]}>{item.status}</Badge>
              </View>
            </View>
          )}
        />
      )}

      {canCreate ? (
        <FAB icon="plus" style={styles.fab} onPress={() => setFormVisible(true)} label={`New ${voucherType}`} />
      ) : null}

      <VoucherFormModal
        visible={formVisible}
        voucherType={voucherType}
        accounts={accounts}
        onDismiss={() => setFormVisible(false)}
        onCreated={() => {
          setFormVisible(false);
          void load();
        }}
      />

      <VoucherDetailModal voucher={detailVoucher} onDismiss={() => setDetailVoucher(null)} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  segmented: { margin: 12 },
  loading: { marginTop: 32 },
  row: { flexDirection: 'row', justifyContent: 'space-between', padding: 16 },
  rowMain: { flex: 1, paddingRight: 12 },
  rowEnd: { alignItems: 'flex-end', gap: 4 },
  muted: { opacity: 0.6, marginTop: 2 },
  badge: { color: 'white' },
  emptyContainer: { flexGrow: 1, justifyContent: 'center' },
  empty: { textAlign: 'center', opacity: 0.6, padding: 24 },
  fab: { position: 'absolute', right: 16, bottom: 16 },
});
