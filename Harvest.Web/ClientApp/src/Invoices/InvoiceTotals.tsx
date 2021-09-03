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
    .filter((expense) => expense.type === "Acreage")
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
    <Card className="card-project-totals mt-4">
      <CardHeader>Invoice Totals</CardHeader>
      <CardBody>
        <div id="total">
          <Row>
            <Col xs="10" sm="10">
              <div>Acreage Fees</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(acreageTotal)}
            </Col>
          </Row>
          <Row>
            <Col xs="10" sm="10">
              <div>Labor</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(laborTotal)}
            </Col>
          </Row>
          <Row>
            <Col xs="10" sm="10">
              <div>Equipment</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(equipmentTotal)}
            </Col>
          </Row>
          <Row>
            <Col xs="10" sm="10">
              <div>Materials / Other</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(otherTotal)}
            </Col>
          </Row>
          <Row className="total-row">
            <Col xs="10" sm="10">
              <h6>Total Cost</h6>
            </Col>
            <Col xs="2" sm="2">
              <span>${formatCurrency(grandTotal)}</span>
            </Col>
          </Row>
        </div>
      </CardBody>
    </Card>
  );
};
