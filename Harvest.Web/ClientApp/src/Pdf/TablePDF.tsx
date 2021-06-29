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
import { QuoteContent } from "../types";
import { ActivityRateTypes } from "../constants";

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
    {props.quote.activities.map((activity) => (
      <View key={`${activity.name}-view`} style={styles.activity}>
        <Text style={styles.activityCost}>
          {activity.name} • Activity Total: ${formatCurrency(activity.total)}
        </Text>
        {ActivityRateTypes.map((type) => (
          // Used react-pdf-table to display a table in the pdf
          // Used the data attribute to display data in the DataTableCell instead
          // of manually rendering it with map
          <Table
            key={`${type}-table`}
            data={activity.workItems.filter((w) => w.type === type)}
          >
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
                getContent={(workItem) => workItem.rate}
              />
              <DataTableCell
                style={styles.tableCell}
                getContent={(workItem) => formatCurrency(workItem.total)}
              />
            </TableBody>
          </Table>
        ))}
      </View>
    ))}
  </View>
);
