import React, { useEffect } from "react";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMinusCircle } from "@fortawesome/free-solid-svg-icons";
import { faCopy } from "@fortawesome/free-solid-svg-icons";
import { faCalendarWeek } from "@fortawesome/free-solid-svg-icons";
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

  const workItemsTotal = props.activity.workItems.reduce((acc, workItem) => (acc + workItem.total), 0);
  
  useEffect(() => {
    props.formik.setFieldValue(`${props.path}.total`, workItemsTotal);
  }, [workItemsTotal]);

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
                className={`form-control ${getInputValidityStyle(props.formik, `${props.path}.name`)}`}
                id={`${props.path}.name`}
                name={`${props.path}.name`}
                value={props.activity.name}
                onChange={props.formik.handleChange}
                onBlur={props.formik.handleBlur}
              />
            </div>
            <ValidationErrorMessage formik={props.formik} name={`${props.path}.name`} />

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
