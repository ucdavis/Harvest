import React from "react";
import { Page, Text, View, Document, StyleSheet } from "@react-pdf/renderer";

import { TablePDF } from "./TableSection";
import { TotalPDF } from "./TotalSection";

import { QuoteContent } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  quote: QuoteContent;
}

const styles = StyleSheet.create({
  activity: {
    paddingBottom: 10,
  },
  activityCost: {
    fontSize: 13,
    fontWeight: "bold",
    paddingBottom: 10,
  },
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
  quoteTotal: {
    paddingTop: 7,
    fontSize: 14,
    fontWeight: "bold",
    color: "#266041",
  },
});

export const QuotePDF = (props: Props) => (
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

      {/* Displays activity tables */}
      {props.quote.activities.map((activity) => (
        <View key={`${activity.name}-view`} style={styles.activity}>
          <Text style={styles.activityCost}>
            {activity.name} • Activity Total: ${formatCurrency(activity.total)}
          </Text>
          <TablePDF tableItem={activity} />
        </View>
      ))}

      <TotalPDF quote={props.quote} />
      <Text style={styles.quoteTotal}>
        Quote Total: ${formatCurrency(props.quote.grandTotal)}
      </Text>
    </Page>
  </Document>
);
