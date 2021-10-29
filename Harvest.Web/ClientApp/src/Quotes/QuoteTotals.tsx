import React from "react";
import { Card, CardBody, CardHeader, Col, Row } from "reactstrap";

import { QuoteContent } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  quote: QuoteContent;
}

export const QuoteTotals = (props: Props) => {
  return (
    <div className="card-wrapper gray-top mt-4">
      <div className="card-header">
        <h4 className="primary-color bold-font">Project Totals</h4>
      </div>
      <div className="card-content">
        <div id="total">
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Acreage Fees</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(props.quote.acreageTotal)}
            </Col>
          </div>
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Labor</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(props.quote.laborTotal)}
            </Col>
          </div>
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Equipment</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(props.quote.equipmentTotal)}
            </Col>
          </div>
          <div className="row justify-content-between">
            <Col xs="6">
              <div>Materials / Other</div>
            </Col>
            <Col xs="4" className="text-right">
              ${formatCurrency(props.quote.otherTotal)}
            </Col>
          </div>
          <Row className="total-row justify-content-between">
            <Col xs="6">
              <h6>Total Cost</h6>
            </Col>
            <Col xs="6" className="text-right">
              <p className="lede">${formatCurrency(props.quote.grandTotal)}</p>
            </Col>
          </Row>
        </div>
      </div>
    </div>
  );
};
