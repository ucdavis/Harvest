import React from "react";
import { Page, Text, View, Document, StyleSheet } from "@react-pdf/renderer";

import { TablePDF } from "./TableSection";
import { TotalPDF } from "./TotalSection";

import { Invoice } from "../types";
import { groupBy } from "../Util/ArrayHelpers";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  invoice: Invoice;
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
  invoiceTitle: {
    color: "#266041",
    fontSize: 20,
    fontWeight: "bold",
  },
  inoviceTotal: {
    paddingTop: 7,
    fontSize: 14,
    fontWeight: "bold",
    color: "#266041",
  },
});

export const InvoicePDF = (props: Props) => {
  const { invoice } = props;
  const acreageExpenses = invoice.expenses.filter(
    (expense) => expense.type === "Acreage"
  );
  const accounts = invoice.transfers.filter(
    (transfer) => transfer.type === "Debit"
  );
  const activities = groupBy(
    invoice.expenses.filter((expense) => expense.type !== "Acreage"),
    (expense) => expense.activity || "Activity"
  );

  return (
    <Document>
      <Page size="A4" style={styles.page}>
        <View>
          <Text style={styles.invoiceTitle}>Invoice</Text>
          {acreageExpenses.map((expense) => (
            <Text style={styles.acerage} key={"expense_" + expense.id}>
              {" "}
              {expense.description}: {expense.quantity} @{" "}
              {formatCurrency(expense.price)} = ${formatCurrency(expense.total)}
            </Text>
          ))}
        </View>

        {/* Displays activity tables */}
        {activities.map((activityExpenses, i) => {
          const activity = activityExpenses[0].activity || "Activity";
          const activityTotal = activityExpenses.reduce(
            (a, b) => a + b.total,
            0
          );

          return (
            <View key={`${activity}-${i}`} style={styles.activity}>
              <Text style={styles.activityCost}>
                {activity} • Activity Total: ${formatCurrency(activityTotal)}
              </Text>
              <TablePDF tableItem={props.invoice} />
            </View>
          );
        })}
      </Page>
    </Document>
  );
};
