import React from "react";
import { Card, CardBody, CardHeader, Input } from "reactstrap";

import { Activity, WorkItem, WorkItemImpl } from "../types";

import { WorkItemsForm } from "./WorkItemsForm";

interface Props {
  activity: Activity;
  updateActivity: (activity: Activity) => void;
}

export const ActivityForm = (props: Props) => {
  const updateWorkItems = (workItem: WorkItem) => {
    // TODO: can we get away without needing to spread copy?  do we need to totally splice/replace?
    const allItems = props.activity.workItems;
    const itemIndex = allItems.findIndex(
      (a) => a.id === workItem.id && a.activityId === workItem.activityId
    );
    allItems[itemIndex] = { ...workItem };

    props.updateActivity({ ...props.activity, workItems: allItems });
  };

  const addNewWorkItem = (category: string) => {
    props.updateActivity({
      ...props.activity,
      workItems: [
        ...props.activity.workItems,
        new WorkItemImpl(
          props.activity.id,
          props.activity.workItems.length + 1,
          category
        ),
      ],
    });
  };
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
        <WorkItemsForm
          category="labor"
          workItems={props.activity.workItems.filter((w) => w.type === "labor")}
          updateWorkItems={updateWorkItems}
          addNewWorkItem={addNewWorkItem}
        />
        <WorkItemsForm
          category="equipment"
          workItems={props.activity.workItems.filter(
            (w) => w.type === "equipment"
          )}
          updateWorkItems={updateWorkItems}
          addNewWorkItem={addNewWorkItem}
        />
        <WorkItemsForm
          category="other"
          workItems={props.activity.workItems.filter((w) => w.type === "other")}
          updateWorkItems={updateWorkItems}
          addNewWorkItem={addNewWorkItem}
        />
      </CardBody>
    </Card>
  );
};
