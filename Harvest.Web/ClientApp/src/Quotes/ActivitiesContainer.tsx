import React, { useEffect } from "react";

import { Activity, QuoteContent, Rate } from "../types";
import { UseFieldArrayReturn, useFormContext, useWatch } from "react-hook-form";
import { ActivityForm } from "./ActivityForm";
import { usePropValuesFromArray, useFormHelpers } from "../Validation";

interface Props {
  rates: Rate[];
  activitiesHelper: UseFieldArrayReturn<QuoteContent, "activities", "fieldId">;
}

export const ActivitiesContainer = (props: Props) => {

  const { control, setValue, register } = useFormContext<QuoteContent>();

  const [acreageTotal, acreageRate, acres] = useWatch<QuoteContent>({
    control: control,
    name: ["acreageTotal", "acreageRate", "acres"]
  }) as [number, number, number];


  const activitiesTotal = usePropValuesFromArray("activities", "total").reduce((acc, total) => acc + total || 0, 0);
  useEffect(() => {
    setValue("activitiesTotal", activitiesTotal);
  }, [activitiesTotal]);

  const laborTotal = usePropValuesFromArray("activities", "laborTotal").reduce((acc, total) => acc + total || 0, 0);
  useEffect(() => {
    register("laborTotal");
    setValue("laborTotal", laborTotal);
  }, [laborTotal]);

  const equipmentTotal = usePropValuesFromArray("activities", "equipmentTotal").reduce((acc, total) => acc + total || 0, 0);
  useEffect(() => {
    setValue("equipmentTotal", equipmentTotal);
  }, [equipmentTotal]);

  const otherTotal = usePropValuesFromArray("activities", "otherTotal").reduce((acc, total) => acc + total || 0, 0);
  useEffect(() => {
    setValue("otherTotal", otherTotal);
  }, [otherTotal]);
  
  useEffect(() => {
    setValue("acreageTotal", acreageRate * acres);
  }, [acreageRate, acres]);

  useEffect(() => {
    setValue("grandTotal", activitiesTotal + acreageTotal);
  }, [activitiesTotal, acreageTotal]);

  register("acreageTotal");
  register("laborTotal");
  register("equipmentTotal");
  register("otherTotal");
  register("grandTotal");

  return (
    <div>
      {props.activitiesHelper.fields.map((field, i) => (
        <ActivityForm
          key={field.fieldId}
          deleteActivity={() => props.activitiesHelper.remove(i)}
          rates={props.rates}
          path={`activities.${i}`}
          defaultValue={field}
        />
      ))}
    </div>
  );
};
