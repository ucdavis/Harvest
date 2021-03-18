import React from "react";
import { Card, CardBody, CardHeader, Col, Row } from "reactstrap";

import { Activity, QuoteContent } from "../types";

import { ActivityForm } from "./ActivityForm";

interface Props {
  quote: QuoteContent;
  updateQuote: React.Dispatch<React.SetStateAction<QuoteContent>>;
}

export const ActivitiesContainer = (props: Props) => {
  const updateActivity = (activity: Activity) => {
    // TODO: can we get away without copying?  do we need to totally splice/replace?
    const allActivities = props.quote.activities;
    const activityIndex = allActivities.findIndex(a => a.id === activity.id);
    allActivities[activityIndex] = { ...activity };

    props.updateQuote({ ...props.quote, activities: allActivities });
  };
  return (
    <div>
      {props.quote.activities.map((activity) => (
        <ActivityForm
          key={`activity-${activity.id}`}
          activity={activity}
          updateActivity={(activity: Activity) => updateActivity(activity)}
        />
      ))}
      <Card>
        <CardHeader>Project Totals</CardHeader>
        <CardBody>
          <div id="total">
            <h6>TODO: Project Totals</h6>
            <hr />
            Totals here
            <Row>
              <Col xs="10" sm="10">
                <h6>Total Cost</h6>
              </Col>
              <Col xs="2" sm="2">
                $123123
              </Col>
            </Row>
          </div>
        </CardBody>
      </Card>
    </div>
  );
};
