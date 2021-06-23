import React from "react";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMinusCircle } from "@fortawesome/free-solid-svg-icons";
import { FieldArray } from "formik";

import { Activity, Rate, RateType, WorkItem, WorkItemImpl, QuoteContent } from "../types";
import { getInputValidityStyle, ValidationErrorMessage, FormikState } from "../Validation";

import { WorkItemsForm } from "./WorkItemsForm";

interface Props {
  activity: Activity;
  deleteActivity: () => void;
  rates: Rate[];
  formik: FormikState<QuoteContent>;
  path: string;
}

export const ActivityForm = (props: Props) => {

  const getNewWorkItem = (category: RateType) => {
    const newId = Math.max(...props.activity.workItems.map((w) => w.id), 0) + 1;
    return new WorkItemImpl(props.activity.id, newId, category);
  };

  return (
    <div className="card-wrapper mb-4 no-green">
      <div className="card-content">
        <div className="row justify-content-between align-items-end">
          <div className="col-md-8">
            <div className="input-group">
              <label> Activity Name</label>
              <input
                className={`form-control ${getInputValidityStyle(props.formik, "description")}`}
                id={`${props.path}.name`}
                name={`${props.path}.name`}
                value={props.activity.name}
                onChange={props.formik.handleChange}
                onBlur={props.formik.handleBlur}
              />
            </div>
            <ValidationErrorMessage formik={props.formik} name={`${props.path}.name`} />
          </div>
          <div className="col-md-4">
            <button
              className="btn btn-link btn-sm"
              onClick={() => props.deleteActivity()}
            >
              Remove activity <FontAwesomeIcon icon={faMinusCircle} />
            </button>
          </div>
        </div>

        <WorkItemsForm
          category="Labor"
          rates={props.rates}
          workItems={props.activity.workItems}
          formik={props.formik}
          path={props.path}
          getNewWorkItem={getNewWorkItem}
        />
        <WorkItemsForm
          category="Equipment"
          rates={props.rates}
          workItems={props.activity.workItems}
          formik={props.formik}
          path={props.path}
          getNewWorkItem={getNewWorkItem}
        />
        <WorkItemsForm
          category="Other"
          rates={props.rates}
          workItems={props.activity.workItems}
          formik={props.formik}
          path={props.path}
          getNewWorkItem={getNewWorkItem}
        />
      </div>
    </div>
  );
};
