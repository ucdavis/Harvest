import React from "react";
import { Text, View, StyleSheet } from "@react-pdf/renderer";
import {
  Table,
  TableHeader,
  TableCell,
  TableBody,
  DataTableCell,
} from "@david.kucsai/react-pdf-table";

import { formatCurrency } from "../Util/NumberFormatting";
import { Activity, Invoice } from "../types";
import { ActivityRateTypes } from "../constants";

interface Props {
  activity: Activity | Invoice;
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
  tableHeader: {
    fontWeight: "bold",
    padding: 3,
  },
  tableCell: {
    padding: 3,
  },
});

export const TablePDF = (props: Props) => (
  // This section displays all the the acitivities associated with the quote
  <View>
    {ActivityRateTypes.map((type) => {
      let data = [];

      if ("workItems" in props.activity) {
        data = props.activity.workItems.filter((w) => w.type === type);
      } else {
        data = props.activity.expenses.filter((w) => w.type === type);
      }

      // Used react-pdf-table to display a table in the pdf
      // Used the data attribute to display data in the DataTableCell instead
      // of manually rendering it with map
      return (
        <Table key={`${type}-table`} data={data}>
          <TableHeader>
            <TableCell style={styles.tableHeader}>{type}</TableCell>
            <TableCell style={styles.tableHeader}>Quantity</TableCell>
            <TableCell style={styles.tableHeader}>Rate</TableCell>
            <TableCell style={styles.tableHeader}>Total</TableCell>
          </TableHeader>
          <TableBody>
            <DataTableCell
              style={styles.tableCell}
              getContent={(workItem) => workItem.description}
            />
            <DataTableCell
              style={styles.tableCell}
              getContent={(workItem) => workItem.quantity}
            />
            <DataTableCell
              style={styles.tableCell}
              getContent={(workItem) => {
                if ("workItems" in props.activity) {
                  return workItem.rate;
                } else {
                  return (
                    workItem.rate &&
                    `(${workItem.rate.unit}) $${formatCurrency(
                      workItem.rate.price
                    )}`
                  );
                }
              }}
            />
            <DataTableCell
              style={styles.tableCell}
              getContent={(workItem) => formatCurrency(workItem.total)}
            />
          </TableBody>
        </Table>
      );
    })}
  </View>
);
