import React from "react";
import { Page, Text, View, Document, StyleSheet } from "@react-pdf/renderer";

import { TableSection } from "./TableSection";
import { TotalSection } from "./TotalSection";

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
});

export const InvoicePDF = (props: Props) => {
  const { invoice } = props;
  const acreageTotal = invoice.expenses
    .filter(
      (expense) => expense.type === "Acreage" || expense.type === "Adjustment"
    )
    .reduce((a, b) => a + b.total, 0);
  const laborTotal = invoice.expenses
    .filter((expense) => expense.type === "Labor")
    .reduce((a, b) => a + b.total, 0);
  const equipmentTotal = invoice.expenses
    .filter((expense) => expense.type === "Equipment")
    .reduce((a, b) => a + b.total, 0);
  const otherTotal = invoice.expenses
    .filter((expense) => expense.type === "Other")
    .reduce((a, b) => a + b.total, 0);
  const grandTotal = acreageTotal + laborTotal + equipmentTotal + otherTotal;

  const acreageExpenses = invoice.expenses.filter(
    (expense) => expense.type === "Acreage" || expense.type === "Adjustment"
  );
  const activities = groupBy(
    invoice.expenses.filter(
      (expense) => expense.type !== "Acreage" && expense.type !== "Adjustment"
    ),
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
              <TableSection tableItem={props.invoice} />
            </View>
          );
        })}

        {/* TODO: Need to add accounts section when accounts are displayed */}

        <TotalSection
          acreageTotal={acreageTotal}
          laborTotal={laborTotal}
          equipmentTotal={equipmentTotal}
          otherTotal={otherTotal}
          grandTotal={grandTotal}
        />
      </Page>
    </Document>
  );
};
