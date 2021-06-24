import React, { useEffect } from "react";

import { Activity, QuoteContent, Rate } from "../types";
import { FormikState } from "../Validation";
import { FieldArray, FieldArrayRenderProps } from "formik";

import { ActivityForm } from "./ActivityForm";

interface Props {
  rates: Rate[];
  formik: FormikState<QuoteContent>;
}

export const ActivitiesContainer = React.forwardRef<FieldArrayRenderProps, Props>((props, ref) => {
  const { formik } = props;
  const { values } = formik;
  const { activities } = values;

  const activitiesTotal = activities.reduce((acc, activity) => (acc + activity.total), 0);

  useEffect(() => {
    props.formik.setFieldValue("activitiesTotal", activitiesTotal);
    props.formik.setFieldValue("laborTotal", activities
      .reduce((accActivity, curActivity) => (
        accActivity + curActivity.workItems.filter(w => w.type === "Labor")
          .reduce((accWorkItem, curWorkItem) => (accWorkItem + curWorkItem.total),
            0)),
        0));
    props.formik.setFieldValue("equipmentTotal", activities
      .reduce((accActivity, curActivity) => (
          accActivity + curActivity.workItems.filter(w => w.type === "Equipment")
            .reduce((accWorkItem, curWorkItem) => (accWorkItem + curWorkItem.total),
              0)),
        0));
    props.formik.setFieldValue("otherTotal", activities
      .reduce((accActivity, curActivity) => (
          accActivity + curActivity.workItems.filter(w => w.type === "Other")
            .reduce((accWorkItem, curWorkItem) => (accWorkItem + curWorkItem.total),
              0)),
        0));

  }, [activitiesTotal, values.acreageTotal]);

  useEffect(() => {
    props.formik.setFieldValue("acreageTotal", values.acreageRate * values.acres);
  }, [values.acreageRate, values.acres]);

  useEffect(() => {
    props.formik.setFieldValue("grandTotal", values.activitiesTotal + values.acreageTotal);
  }, [values.activitiesTotal, values.acreageTotal]);

  // ugly hack to be able to add an activity from outside ActivitiesContainer
  let arrayHelpersRef: FieldArrayRenderProps;
  React.useImperativeHandle(ref, () => arrayHelpersRef);

  return (
    <FieldArray name="activities">
      {(arrayHelpers) => {
        arrayHelpersRef = arrayHelpers;
        return (
          <div>
            {activities.map((activity, i) => (
              <ActivityForm
                key={`activity-${activity.id}`}
                activity={activity}
                deleteActivity={() => arrayHelpers.remove(i)}
                rates={props.rates}
                formik={formik}
                path={`activities.${i}`} />
            ))}
          </div>
        );
      }}
    </FieldArray>
  );
});
