import React from "react";
import { Card, CardBody, CardHeader, Input } from "reactstrap";

import { Activity } from "../types";

interface Props {
  activity: Activity;
  updateActivity: (activity: Activity) => void;
}

export const ActivityForm = (props: Props) => {
  return (
    <Card>
      <CardHeader>
        <Input
          type="text"
          id="activityName"
          value={props.activity.name}
          onChange={(e) =>
            props.updateActivity({ ...props.activity, name: e.target.value })
          }
        ></Input>
      </CardHeader>
      <CardBody>
        <div>
          <h4>Labor</h4>
          <h4>Equipment</h4>
          <h4>Other</h4>
        </div>
      </CardBody>
    </Card>
  );
};
