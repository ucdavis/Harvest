import React from "react";
import { Card, CardBody, CardHeader, Col, Row } from "reactstrap";

import { Activity, QuoteContent, Rate } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

import { ActivityForm } from "./ActivityForm";

interface Props {
  rates: Rate[];
  quote: QuoteContent;
  updateQuote: React.Dispatch<React.SetStateAction<QuoteContent>>;
}

export const ActivitiesContainer = (props: Props) => {
  const updateActivity = (activity: Activity) => {
    // TODO: can we get away without needing to spread copy?  do we need to totally splice/replace?
    const allActivities = props.quote.activities;
    const activityIndex = allActivities.findIndex((a) => a.id === activity.id);
    allActivities[activityIndex] = { ...activity };

    props.updateQuote({ ...props.quote, activities: [...allActivities] });
  };
  return (
    <div>
      {props.quote.activities.map((activity) => (
        <ActivityForm
          key={`activity-${activity.id}`}
          activity={activity}
          updateActivity={(activity: Activity) => updateActivity(activity)}
          rates={props.rates}
        />
      ))}
      <Card className="card-project-totals">
        <CardHeader>Project Totals</CardHeader>
        <CardBody>
          <div id="total">
            <h6>Project Totals</h6>
            <hr />
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
                $
                {formatCurrency(
                  props.quote.activities.reduce(
                    (prev, curr) =>
                      prev +
                      curr.workItems
                        .filter((w) => w.type === "labor")
                        .reduce((p, c) => c.rate * c.quantity + p, 0),
                    0
                  )
                )}
              </Col>
            </Row>
            <Row>
              <Col xs="10" sm="10">
                <div>Equipment</div>
              </Col>
              <Col xs="2" sm="2">
                $
                {formatCurrency(
                  props.quote.activities.reduce(
                    (prev, curr) =>
                      prev +
                      curr.workItems
                        .filter((w) => w.type === "equipment")
                        .reduce((p, c) => c.rate * c.quantity + p, 0),
                    0
                  )
                )}
              </Col>
            </Row>
            <Row>
              <Col xs="10" sm="10">
                <div>Materials / Other</div>
              </Col>
              <Col xs="2" sm="2">
                $
                {formatCurrency(
                  props.quote.activities.reduce(
                    (prev, curr) =>
                      prev +
                      curr.workItems
                        .filter((w) => w.type === "other")
                        .reduce((p, c) => c.rate * c.quantity + p, 0),
                    0
                  )
                )}
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
    </div>
  );
};
