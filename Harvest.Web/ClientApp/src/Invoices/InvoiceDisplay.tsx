import React from "react";
import { Col, Row } from "reactstrap";

import { Invoice } from "../types";
import { ExpenseDisplay } from "../Expenses/ExpenseDisplay";
import { formatCurrency } from "../Util/NumberFormatting";
import { groupBy } from "../Util/ArrayHelpers";
import { InvoiceTotals } from "./InvoiceTotals";

interface Props {
  invoice: Invoice;
}

export const InvoiceDisplay = (props: Props) => {
  const { invoice } = props;
  const acreageExpenses = invoice.expenses.filter(
    (expense) => expense.type === "Acreage" || expense.type === "Adjustment"
  );
  const accounts = invoice.transfers.filter(
    (transfer) => transfer.isProjectAccount === true
  );
  const activities = groupBy(
    invoice.expenses.filter(
      (expense) => expense.type !== "Acreage" && expense.type !== "Adjustment"
    ),
    (expense) => expense.activity || "Activity"
  );

  return (
    <div>
      <h1>Invoice</h1>
      <br />
      {acreageExpenses.map((expense) => (
        <p key={"expense_" + expense.id}>
          <b>
            {expense.description}: {expense.quantity} @{" "}
            {formatCurrency(expense.price)} = ${formatCurrency(expense.total)}
          </b>
        </p>
      ))}

      {activities.map((activityExpenses, i) => {
        const activity = activityExpenses[0].activity || "Activity";
        const activityTotal = activityExpenses.reduce((a, b) => a + b.total, 0);

        return (
          <div
            className="quote-actvitiy-item card-wrapper mb-4 gray-top"
            key={`${activity}-${i}`}
          >
            <div className="card-header">
              <h4>
                <span className="primary-color bold-font">{activity}</span> •
                Activity Total: ${formatCurrency(activityTotal)}
              </h4>
            </div>
            <div className="card-content">
              <ExpenseDisplay expenses={activityExpenses}></ExpenseDisplay>
            </div>
          </div>
        );
      })}

      {accounts?.length > 0 && (
        <div className="card-wrapper gray-top mt-4">
          <div className="card-header">
            <h4 className="primary-color bold-font">Account(s)</h4>
          </div>

          <div className="card-content">
            <div id="accounts123">
              {accounts.map((transfers) => (
                <Row key={"transfer_" + transfers.id}>
                  <Col xs="10" sm="10">
                    <div>{transfers.account}</div>
                  </Col>
                  <Col xs="2" sm="2">
                    ${formatCurrency(transfers.total)}
                  </Col>
                </Row>
              ))}
            </div>
          </div>
        </div>
      )}

      <InvoiceTotals invoice={props.invoice}></InvoiceTotals>
    </div>
  );
};
