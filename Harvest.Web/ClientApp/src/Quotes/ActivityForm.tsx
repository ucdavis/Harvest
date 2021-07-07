import React, { useEffect, useCallback } from "react";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMinusCircle } from "@fortawesome/free-solid-svg-icons";
import { faCopy } from "@fortawesome/free-solid-svg-icons";
import { faCalendarWeek } from "@fortawesome/free-solid-svg-icons";
import { UseFieldArrayReturn, useFormContext, useWatch, useFieldArray } from "react-hook-form";


import { Activity, Rate, RateType, WorkItem, WorkItemImpl, QuoteContent } from "../types";
import { useFormHelpers, ValidationErrorMessage, usePropValuesFromArray } from "../Validation";

import { WorkItemsForm } from "./WorkItemsForm";

interface Props {
  deleteActivity: () => void;
  rates: Rate[];
  path: string;
  defaultValue: Activity;
}

export const ActivityForm = (props: Props) => {

  const { getValues, setValue, register, control } = useFormContext();

  const { getPath, getInputValidityStyle } = useFormHelpers(props.path);

  const getNewWorkItem = (category: RateType) => {
    const path = getPath("workItems");
    const workItems = getValues(path as "activities.0.workitems");
    const newId = Math.max(...(workItems).map((w: WorkItem) => w.id), 0) + 1;
    return new WorkItemImpl(getValues(getPath("id") as "activities.0.id"), newId, category);
  };

  const laborTotal = usePropValuesFromArray(getPath("workItems"), "total", (item: WorkItem) => item.type === "Labor")
    .reduce((acc, total) => acc + total || 0, 0);
  useEffect(() => {
    setValue(getPath("laborTotal") as "activities.0.total", laborTotal);
  }, [laborTotal]);

  const equipmentTotal = usePropValuesFromArray(getPath("workItems"), "total", (item: WorkItem) => item.type === "Equipment")
    .reduce((acc, total) => acc + total || 0, 0);
  useEffect(() => {
    setValue(getPath("equipmentTotal") as "activities.0.total", equipmentTotal);
  }, [equipmentTotal]);

  const otherTotal = usePropValuesFromArray(getPath("workItems"), "total", (item: WorkItem) => item.type === "Other")
    .reduce((acc, total) => acc + total || 0, 0);
  useEffect(() => {
    setValue(getPath("otherTotal") as "activities.0.total", otherTotal);
  }, [otherTotal]);

  useEffect(() => {
    setValue(getPath("total") as "activities.0.total", laborTotal + equipmentTotal + otherTotal);
  }, [laborTotal, equipmentTotal, otherTotal]);

  const workItemsHelper = useFieldArray({ control, name: getPath("workItems") as "", keyName: "fieldId" });

  register("total");
  register("laborTotal");
  register("equipmentTotal");
  register("otherTotal");

  return (
    <div className="card-wrapper mb-4 no-green">
      <div className="card-content">
        <div className="row justify-content-between align-items-end">
          <div className="col-md-12">
            <div className="row justify-content-between">
              <div className="col-md-6">
                <label> Activity Name</label>
              </div>

              <div className="col-md-6 text-right">
                <button className="btn btn-link btn-sm">
                  Adjust Year <FontAwesomeIcon icon={faCalendarWeek} />
                </button>
                <div className="btn-group">
                  <button
                    type="button"
                    className="btn btn-danger dropdown-toggle"
                    data-toggle="dropdown"
                    aria-haspopup="true"
                    aria-expanded="false"
                  >
                    Action
                  </button>
                  <div className="dropdown-menu">
                    <a className="dropdown-item" href="#">
                      Action
                    </a>
                    <a className="dropdown-item" href="#">
                      Another action
                    </a>
                    <a className="dropdown-item" href="#">
                      Something else here
                    </a>
                    <div className="dropdown-divider"></div>
                    <a className="dropdown-item" href="#">
                      Separated link
                    </a>
                  </div>
                </div>
                <button className="btn btn-link btn-sm">
                  Duplicate Activity <FontAwesomeIcon icon={faCopy} />
                </button>
                <button
                  className="btn btn-link btn-sm"
                  onClick={props.deleteActivity}
                >
                  Remove activity <FontAwesomeIcon icon={faMinusCircle} />
                </button>
              </div>
            </div>

            <div>
              <input
                className={`form-control ${getInputValidityStyle("name")}`}
                {...register(getPath("name") as "name")}
                defaultValue={props.defaultValue.name}
              />
            </div>
            <ValidationErrorMessage name={getPath("name")} />

          </div>
        </div>

        <WorkItemsForm
          category="Labor"
          rates={props.rates}
          getNewWorkItem={getNewWorkItem}
          path={getPath("workItems")}
          workItemsHelper={workItemsHelper}
        />
        <WorkItemsForm
          category="Equipment"
          rates={props.rates}
          getNewWorkItem={getNewWorkItem}
          path={getPath("workItems")}
          workItemsHelper={workItemsHelper}
        />
        <WorkItemsForm
          category="Other"
          rates={props.rates}
          getNewWorkItem={getNewWorkItem}
          path={getPath("workItems")}
          workItemsHelper={workItemsHelper}
        />
      </div>
    </div>
  );
};
