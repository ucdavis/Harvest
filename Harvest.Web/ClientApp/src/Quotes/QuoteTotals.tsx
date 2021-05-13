import React from "react";
import { Card, CardBody, CardHeader, Col, Row } from "reactstrap";

import { QuoteContent } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  quote: QuoteContent;
}

export const QuoteTotals = (props: Props) => {
  return (
    <Card className="card-project-totals mt-4">
      <CardHeader>Project Totals</CardHeader>
      <CardBody>
        <div id="total">
          <Row>
            <Col xs="10" sm="10">
              <div>Acreage Fees</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(props.quote.acreageTotal)}
            </Col>
          </Row>
          <Row>
            <Col xs="10" sm="10">
              <div>Labor</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(props.quote.laborTotal)}
            </Col>
          </Row>
          <Row>
            <Col xs="10" sm="10">
              <div>Equipment</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(props.quote.equipmentTotal)}
            </Col>
          </Row>
          <Row>
            <Col xs="10" sm="10">
              <div>Materials / Other</div>
            </Col>
            <Col xs="2" sm="2">
              ${formatCurrency(props.quote.otherTotal)}
            </Col>
          </Row>
          <Row className="total-row">
            <Col xs="10" sm="10">
              <h6>Total Cost</h6>
            </Col>
            <Col xs="2" sm="2">
              <span>${formatCurrency(props.quote.grandTotal)}</span>
            </Col>
          </Row>
        </div>
      </CardBody>
    </Card>
  );
};
