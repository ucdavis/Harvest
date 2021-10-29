import React from "react";
import { Card, CardBody, CardHeader, Col, Row } from "reactstrap";

import { Invoice } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  invoice: Invoice;
}

export const InvoiceTotals = (props: Props) => {
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

  return (
    <div className="card-wrapper gray-top mt-4">
      <div className="card-header">
        <h4 className="primary-color bold-font">Invoice Totals</h4>
      </div>
      <div className="card-content">
        <div id="total">
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Acreage Fees</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(acreageTotal)}
            </Col>
          </div>
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Labor</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(laborTotal)}
            </Col>
          </div>
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Equipment</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(equipmentTotal)}
            </Col>
          </div>
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Materials / Other</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(otherTotal)}
            </Col>
          </div>
          <Row className="total-row justify-content-between">
            <Col xs="6">
              <h6>Total Cost</h6>
            </Col>
            <Col xs="6" className="text-right">
              <p className="lede">${formatCurrency(grandTotal)}</p>
            </Col>
          </Row>
        </div>
      </div>
    </div>
  );
};
