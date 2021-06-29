import React from "react";
import { Page, Text, View, Document, StyleSheet } from "@react-pdf/renderer";
import {
  Table,
  TableHeader,
  TableCell,
  TableBody,
  DataTableCell,
} from "@david.kucsai/react-pdf-table";

import { TablePDF } from "./TablePDF";
import { formatCurrency } from "../Util/NumberFormatting";
import { QuoteContent } from "../types";
import { ActivityRateTypes } from "../constants";

interface Props {
  quote: QuoteContent;
}

const styles = StyleSheet.create({
  page: {
    fontSize: 12,
    padding: 25,
  },
  acerage: {
    fontWeight: "bold",
    paddingTop: 10,
    paddingBottom: 10,
    paddingRight: 10,
  },
  quoteTitle: {
    color: "#266041",
    fontSize: 20,
    fontWeight: "bold",
  },
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
  quoteTotal: {
    paddingTop: 7,
    fontSize: 14,
    fontWeight: "bold",
    color: "#266041",
  },
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
  },
  col: {
    padding: 1,
  },
});

export const ApprovalPDF = (props: Props) => (
  <Document>
    <Page size="A4" style={styles.page}>
      <View>
        <Text style={styles.quoteTitle}>Quote</Text>
        <Text style={styles.acerage}>
          {" "}
          {props.quote.acreageRateDescription}: {props.quote.acres} @{" "}
          {formatCurrency(props.quote.acreageRate)} = $
          {formatCurrency(props.quote.acreageTotal)}
        </Text>
      </View>
      <TablePDF quote={props.quote} />
      {/* This section displays the project total */}
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
          <Text style={styles.col}>
            ${formatCurrency(props.quote.laborTotal)}
          </Text>
        </View>
        <View style={styles.row}>
          <Text style={styles.col}>Equipment</Text>
          <Text style={styles.col}>
            ${formatCurrency(props.quote.equipmentTotal)}
          </Text>
        </View>
        <View style={styles.row}>
          <Text style={styles.col}>Materials / Other</Text>
          <Text style={styles.col}>
            ${formatCurrency(props.quote.otherTotal)}
          </Text>
        </View>
        <View style={styles.row}>
          <Text style={styles.totalCost}>Total Cost</Text>
          <Text style={styles.totalCost}>
            ${formatCurrency(props.quote.grandTotal)}
          </Text>
        </View>
      </View>
      <Text style={styles.quoteTotal}>
        Quote Total: ${formatCurrency(props.quote.grandTotal)}
      </Text>
    </Page>
  </Document>
);
