import React from "react";

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
