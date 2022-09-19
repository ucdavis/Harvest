import React from "react";
import { Page, Text, View, Document, StyleSheet } from "@react-pdf/renderer";

import { TableSection } from "./TableSection";
import { TotalSection } from "./TotalSection";

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
          {formatCurrency(props.quote.acreageRate)} * {props.quote.years}{" "}
          {props.quote.years > 1 ? "years" : "year"} = $
          {formatCurrency(props.quote.acreageTotal)}
        </Text>
      </View>

      {/* Displays activity tables */}
      {props.quote.activities.map((activity) => (
        <View key={`${activity.name}-view`} style={styles.activity}>
          <Text style={styles.activityCost}>
            {activity.name} • Activity Total: ${formatCurrency(activity.total)}
          </Text>
          <TableSection tableItem={activity} />
        </View>
      ))}

      <TotalSection
        acreageTotal={props.quote.acreageTotal}
        laborTotal={props.quote.laborTotal}
        equipmentTotal={props.quote.equipmentTotal}
        otherTotal={props.quote.otherTotal}
        grandTotal={props.quote.grandTotal}
      />
      <Text style={styles.quoteTotal}>
        Quote Total: ${formatCurrency(props.quote.grandTotal)}
      </Text>
    </Page>
  </Document>
);
