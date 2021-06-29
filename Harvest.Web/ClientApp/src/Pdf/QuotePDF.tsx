import React from "react";
import { Page, Text, View, Document, StyleSheet } from "@react-pdf/renderer";

import { TablePDF } from "./TablePDF";
import { TotalPDF } from "./TotalPDF";
import { formatCurrency } from "../Util/NumberFormatting";
import { QuoteContent } from "../types";

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
      <TablePDF quote={props.quote} />
      <TotalPDF quote={props.quote} />
      <Text style={styles.quoteTotal}>
        Quote Total: ${formatCurrency(props.quote.grandTotal)}
      </Text>
    </Page>
  </Document>
);
