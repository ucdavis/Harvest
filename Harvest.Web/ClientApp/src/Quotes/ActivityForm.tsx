import React from "react";

import { Input } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMinusCircle } from "@fortawesome/free-solid-svg-icons";

import { Activity, Rate, WorkItem, WorkItemImpl } from "../types";

import { WorkItemsForm } from "./WorkItemsForm";

interface Props {
  activity: Activity;
  updateActivity: (activity: Activity) => void;
  deleteActivity: (activity: Activity) => void;
  rates: Rate[];
}

export const ActivityForm = (props: Props) => {
  const updateWorkItems = (workItem: WorkItem) => {
    // TODO: can we get away without needing to spread copy?  do we need to totally splice/replace?
    const allItems = props.activity.workItems;
    const itemIndex = allItems.findIndex(
      (a) => a.id === workItem.id && a.activityId === workItem.activityId
    );
    allItems[itemIndex] = {
      ...workItem,
      total: workItem.rate * workItem.quantity,
    };

    props.updateActivity({ ...props.activity, workItems: allItems });
  };

  const addNewWorkItem = (category: string) => {
    const newId = Math.max(...props.activity.workItems.map((w) => w.id), 0) + 1;
    props.updateActivity({
      ...props.activity,
      workItems: [
        ...props.activity.workItems,
        new WorkItemImpl(props.activity.id, newId, category),
      ],
    });
  };

  const deleteWorkItem = (workItem: WorkItem) => {
    // dump our deleted friend
    const itemsToKeep = props.activity.workItems.filter(
      (w) => w.id !== workItem.id
    );
    props.updateActivity({
      ...props.activity,
      workItems: itemsToKeep,
    });
  };

  return (
    <div className="card-wrapper mb-4 no-green">
      <div className="card-content">
        <div className="row justify-content-between align-items-end">
          <div className="col-md-8">
            <label> Activity Name</label>
            <Input
              type="text"
              id="activityName"
              value={props.activity.name}
              onChange={(e) =>
                props.updateActivity({
                  ...props.activity,
                  name: e.target.value,
                })
              }
            ></Input>
          </div>
          <div className="col-md-4">
            <button
              className="btn btn-link btn-sm"
              onClick={() => props.deleteActivity(props.activity)}
            >
              Remove activity <FontAwesomeIcon icon={faMinusCircle} />
            </button>
          </div>
        </div>

        <WorkItemsForm
          category="labor"
          rates={props.rates.filter((r) => r.type === "Labor")}
          workItems={props.activity.workItems.filter((w) => w.type === "labor")}
          updateWorkItems={updateWorkItems}
          addNewWorkItem={addNewWorkItem}
          deleteWorkItem={deleteWorkItem}
        />
        <WorkItemsForm
          category="equipment"
          rates={props.rates.filter((r) => r.type === "Equipment")}
          workItems={props.activity.workItems.filter(
            (w) => w.type === "equipment"
          )}
          updateWorkItems={updateWorkItems}
          addNewWorkItem={addNewWorkItem}
          deleteWorkItem={deleteWorkItem}
        />
        <WorkItemsForm
          category="other"
          rates={props.rates.filter((r) => r.type === "Other")}
          workItems={props.activity.workItems.filter((w) => w.type === "other")}
          updateWorkItems={updateWorkItems}
          addNewWorkItem={addNewWorkItem}
          deleteWorkItem={deleteWorkItem}
        />
      </div>
    </div>
  );
};
