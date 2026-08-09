import React, { useState } from 'react';
import { StyleSheet, View } from 'react-native';
import { SegmentedButtons } from 'react-native-paper';
import StockLedgerScreen from './StockLedgerScreen';
import StockOnHandScreen from './StockOnHandScreen';

type Tab = 'onHand' | 'ledger';

/** Container for the two read-only inventory reports (phase 2 of the build plan). */
export default function InventoryScreen() {
  const [tab, setTab] = useState<Tab>('onHand');

  return (
    <View style={styles.container}>
      <SegmentedButtons
        value={tab}
        onValueChange={(value) => setTab(value as Tab)}
        style={styles.segmented}
        buttons={[
          { value: 'onHand', label: 'On Hand' },
          { value: 'ledger', label: 'Ledger' },
        ]}
      />
      <View style={styles.content}>{tab === 'onHand' ? <StockOnHandScreen /> : <StockLedgerScreen />}</View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  segmented: { margin: 12 },
  content: { flex: 1 },
});
