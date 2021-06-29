import React from "react";
import { Text, View, StyleSheet } from "@react-pdf/renderer";

import { formatCurrency } from "../Util/NumberFormatting";
import { QuoteContent } from "../types";

interface Props {
  quote: QuoteContent;
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

export const TotalPDF = (props: Props) => (
  // This section displays the project total
  <View style={styles.projectTotal}>
    <Text style={styles.projectTotalTitle}>Project Totals</Text>
    <View style={styles.row}>
      <Text style={styles.col}>Acreage Fees</Text>
      <Text style={styles.col}>
        ${formatCurrency(props.quote.acreageTotal)}
      </Text>
    </View>
    <View style={styles.row}>
      <Text style={styles.col}>Labor</Text>
      <Text style={styles.col}>${formatCurrency(props.quote.laborTotal)}</Text>
    </View>
    <View style={styles.row}>
      <Text style={styles.col}>Equipment</Text>
      <Text style={styles.col}>
        ${formatCurrency(props.quote.equipmentTotal)}
      </Text>
    </View>
    <View style={styles.row}>
      <Text style={styles.col}>Materials / Other</Text>
      <Text style={styles.col}>${formatCurrency(props.quote.otherTotal)}</Text>
    </View>
    <View style={styles.row}>
      <Text style={styles.totalCost}>Total Cost</Text>
      <Text style={styles.totalCost}>
        ${formatCurrency(props.quote.grandTotal)}
      </Text>
    </View>
  </View>
);
