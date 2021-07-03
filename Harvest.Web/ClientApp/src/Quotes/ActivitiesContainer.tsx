import React, { useEffect } from "react";

import { Activity, QuoteContent, Rate } from "../types";
import { UseFieldArrayReturn, useFormContext, useWatch } from "react-hook-form";
import { ActivityForm } from "./ActivityForm";

interface Props {
  rates: Rate[];
  activitiesHelper: UseFieldArrayReturn<QuoteContent, "activities", "fieldId">;
}

export const ActivitiesContainer = (props: Props) => {

  const { control, setValue, getValues } = useFormContext<QuoteContent>();

  const [acreageTotal, acreageRate, acres, activities] = useWatch<QuoteContent>({
    control: control,
    name: ["acreageTotal", "acreageRate", "acres", "activities"]
  }) as [number, number, number, Activity[]];

  // can't use a watched workItems collection because it won't reflect the most recent update to a given workItem
  const handleActivityTotalChanged = () => {
    const activities = getValues("activities") || [];
    const activitiesTotal = activities.reduce((acc, workItem) => (acc + workItem.total), 0);
    setValue("activitiesTotal", activitiesTotal);
    setValue("laborTotal", activities
      .reduce<number>((accActivity, curActivity) => accActivity + (curActivity.workItems || []).filter(w => w.type === "Labor")
        .reduce((accWorkItem, curWorkItem) => (accWorkItem + curWorkItem.total), 0), 0));
    setValue("equipmentTotal", activities
      .reduce<number>((accActivity, curActivity) => accActivity + (curActivity.workItems || []).filter(w => w.type === "Equipment")
        .reduce((accWorkItem, curWorkItem) => (accWorkItem + curWorkItem.total), 0), 0));
    setValue("otherTotal", getValues("activities")
      .reduce<number>((accActivity, curActivity) => accActivity + (curActivity.workItems || []).filter(w => w.type === "Other")
        .reduce((accWorkItem, curWorkItem) => (accWorkItem + curWorkItem.total), 0), 0));
    setValue("grandTotal", activitiesTotal + acreageTotal);
  }
  
  useEffect(() => {
    setValue("acreageTotal", acreageRate * acres);
  }, [acreageRate, acres]);

  //useEffect(() => {
  //  setValue("grandTotal", activitiesTotal + acreageTotal);
  //}, [activitiesTotal, acreageTotal]);

  return (
    <div>
      {props.activitiesHelper.fields.map((field, i) => (
        <ActivityForm
          key={field.fieldId}
          deleteActivity={() => props.activitiesHelper.remove(i)}
          rates={props.rates}
          path={`activities.${i}`}
          defaultValue={field}
          onTotalChanged={handleActivityTotalChanged}
        />
      ))}
    </div>
  );
};
