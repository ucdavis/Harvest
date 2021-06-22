import React from "react";
import { Page, Text, View, Document } from "@react-pdf/renderer";
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

export const ApprovalPDF = (props: Props) => (
  <Document>
    <Page size="A4">
      <View>
        <Text>Quote</Text>
        <Text>
          {" "}
          {props.quote.acreageRateDescription}: {props.quote.acres} @{" "}
          {formatCurrency(props.quote.acreageRate)} = $
          {formatCurrency(props.quote.acreageTotal)}
        </Text>
      </View>
      <View>
        {props.quote.activities.map((activity) => (
          <View key={`${activity.name}-view`}>
            <Text>
              {activity.name} • Activity Total: $
              {formatCurrency(activity.total)}
            </Text>
            {ActivityRateTypes.map((type) => (
              <Table
                key={`${type}-table`}
                data={activity.workItems.filter((w) => w.type === type)}
              >
                <TableHeader>
                  <TableCell>{type}</TableCell>
                  <TableCell>Quantity</TableCell>
                  <TableCell>Rate</TableCell>
                  <TableCell>Total</TableCell>
                </TableHeader>
                <TableBody>
                  <DataTableCell
                    getContent={(workItem) => workItem.description}
                  />
                  <DataTableCell getContent={(workItem) => workItem.quantity} />
                  <DataTableCell getContent={(workItem) => workItem.rate} />
                  <DataTableCell
                    getContent={(workItem) => formatCurrency(workItem.total)}
                  />
                </TableBody>
              </Table>
            ))}
          </View>
        ))}
      </View>
      <View>
        <Text>Project Totals</Text>
        <View>
          <Text>Acreage Fees</Text>
          <Text>${formatCurrency(props.quote.acreageTotal)}</Text>
        </View>
        <View>
          <Text>Labor</Text>
          <Text>${formatCurrency(props.quote.laborTotal)}</Text>
        </View>
        <View>
          <Text>Equipment</Text>
          <Text>${formatCurrency(props.quote.equipmentTotal)}</Text>
        </View>
        <View>
          <Text>Materials / Other</Text>
          <Text>${formatCurrency(props.quote.otherTotal)}</Text>
        </View>
        <View>
          <Text>Total Cost</Text>
          <Text>${formatCurrency(props.quote.grandTotal)}</Text>
        </View>
      </View>
    </Page>
  </Document>
);
