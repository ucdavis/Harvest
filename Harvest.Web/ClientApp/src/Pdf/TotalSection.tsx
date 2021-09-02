import React from "react";
import { Text, View, StyleSheet } from "@react-pdf/renderer";

import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  acreageTotal: number;
  laborTotal: number;
  equipmentTotal: number;
  otherTotal: number;
  grandTotal: number;
}

const styles = StyleSheet.create({
  projectTotal: {
    padding: 5,
    borderTop: 1,
    borderRight: 1,
    borderBottom: 1,
    borderLeft: 1,
    borderStyle: "solid",
    borderColor: "#cccccc",
  },
  projectTotalTitle: {
    fontSize: 15,
    paddingBottom: 8,
  },
  totalCost: {
    fontSize: 13,
    paddingTop: 8,
  },
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
  },
  col: {
    padding: 1,
  },
});

export const TotalSection = (props: Props) => (
  // This section displays the project total
  <View style={styles.projectTotal}>
    <Text style={styles.projectTotalTitle}>Project Totals</Text>
    {props.acreageTotal > 0 && (
      <View style={styles.row}>
        <Text style={styles.col}>Acreage Fees</Text>
        <Text style={styles.col}>${formatCurrency(props.acreageTotal)}</Text>
      </View>
    )}
    {props.laborTotal > 0 && (
      <View style={styles.row}>
        <Text style={styles.col}>Labor</Text>
        <Text style={styles.col}>${formatCurrency(props.laborTotal)}</Text>
      </View>
    )}
    {props.otherTotal > 0 && (
      <View style={styles.row}>
        <Text style={styles.col}>Equipment</Text>
        <Text style={styles.col}>${formatCurrency(props.equipmentTotal)}</Text>
      </View>
    )}
    {props.otherTotal > 0 && (
      <View style={styles.row}>
        <Text style={styles.col}>Materials / Other</Text>
        <Text style={styles.col}>${formatCurrency(props.otherTotal)}</Text>
      </View>
    )}
    <View style={styles.row}>
      <Text style={styles.totalCost}>Total Cost</Text>
      <Text style={styles.totalCost}>${formatCurrency(props.grandTotal)}</Text>
    </View>
  </View>
);
