import React from "react";
import { View, StyleSheet } from "@react-pdf/renderer";
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
  tableItem: Activity | Invoice;
  activityName?: string;
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

export const TableSection = (props: Props) => (
  // This section displays all the the acitivities associated with the quote
  <View>
    {ActivityRateTypes.map((type) => {
      let data = [];

      if ("workItems" in props.tableItem) {
        data = props.tableItem.workItems.filter((w) => w.type === type);
      } else {
        if (props.activityName !== undefined) {
          data = props.tableItem.expenses.filter(
            (w) => w.type === type && w.activity === props.activityName
          );
        } else {
          data = props.tableItem.expenses.filter((w) => w.type === type);
        }
      }

      if (data.length > 0) {
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
                  if ("workItems" in props.tableItem) {
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
      } else {
        return null;
      }
    })}
  </View>
);
