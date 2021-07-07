import React, { useEffect, useCallback } from "react";
import { Button, Col, FormGroup, InputGroup, InputGroupAddon, InputGroupText, Row, } from "reactstrap";
import { useFormContext, useFieldArray, useWatch, useFormState, UseFieldArrayReturn } from "react-hook-form";
import get from "lodash/get";


import { Rate, RateType, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { ValidationErrorMessage, useFormHelpers } from "../Validation";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt } from "@fortawesome/free-solid-svg-icons";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface Props {
  category: RateType;
  rates: Rate[];
  getNewWorkItem: (category: RateType) => WorkItem;
  path: string;
  workItemsHelper: UseFieldArrayReturn<Record<string, any>, "", "fieldId">;
}

interface WorkItemProps {
  category: RateType;
  rates: Rate[];
  path: string;
  deleteWorkItem: () => void;
  defaultValue: WorkItem;
}

const WorkItemForm = (props: WorkItemProps) => {

  const { setValue, control, register } = useFormContext();

  const { getPath, getInputValidityStyle } = useFormHelpers(props.path);

  const [rateId, rate, quantity, unit, total] = useWatch({
    control,
    name: [getPath("rateId") as "", getPath("rate") as "", getPath("quantity") as "", getPath("unit") as "", getPath("total") as ""]
  }) as [number, number, number, string, number];

  const { dirtyFields, touchedFields } = useFormState({ control });

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string | null) => {
    return props.category === "Other" && unit === "Unit";
  };

  useEffect(() => {
    if (get(dirtyFields, getPath("rateId")) !== undefined || get(touchedFields, getPath("rateId")) !== undefined) {
      const rate = props.rates.find((r) => r.id === rateId);

      // rate can be undefinied if they select the default option
      if (!!rate) {
        // new rate selected, update the work item with defaults
        setValue(getPath("rate") as "", rate.price);
        setValue(getPath("description") as "", requiresCustomDescription(rate.unit) ? "" : rate.description);
        setValue(getPath("unit") as "", rate.unit);
      } else {
        setValue(getPath("rate") as "", 0);
        setValue(getPath("description") as "", "");
        setValue(getPath("unit") as "", "");
      }
    }
  }, [rateId]);

  useEffect(() => {
    setValue(getPath("total") as "", rate * quantity);
  }, [rate, quantity]);


  register(getPath("id") as "");
  register(getPath("description") as "");
  register(getPath("activityId") as "");
  register(getPath("type") as "");
  register(getPath("rate") as "");
  register(getPath("unit") as "");

  return (
    <Row
      className="activity-line-item"
      key={getPath("id")}>
      <Col xs="5">
        <FormGroup>
          <select
            className={`form-control ${getInputValidityStyle(getPath("rateId"))}`}
            {...register(getPath("rateId") as "", { valueAsNumber: true })}
            defaultValue={props.defaultValue.rateId}
          >
            <option value="0">-- Select {props.category} --</option>
            {props.rates.filter((r) => r.type === props.category).map((r) => (
              <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                {r.description}
              </option>
            ))}
          </select>

          {requiresCustomDescription(unit) && (
            <input
              className={`form-control ${getInputValidityStyle("description")}`}
              {...register(getPath("description") as "")}
              defaultValue={props.defaultValue.description}
            />
          )}
        </FormGroup>
        <ValidationErrorMessage name={getPath("rateId")} />
        <ValidationErrorMessage name={getPath("description")} />
      </Col>

      <Col xs="3">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>{unit || ""}</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle("quantity")}`}
            type="number"
            {...register(getPath("quantity") as "", { valueAsNumber: true })}
            defaultValue={props.defaultValue.quantity}
          />
        </InputGroup>
        <ValidationErrorMessage name={getPath("quantity")} />
      </Col>

      <Col xs="2">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>$</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle("rate")}`}
            type="number"
            {...register(getPath("rate") as "", { valueAsNumber: true })}
            defaultValue={props.defaultValue.rate}
          />
        </InputGroup>
        <ValidationErrorMessage name={getPath("rate")} />
      </Col>

      <Col xs="1">${formatCurrency(rate * quantity)}</Col>

      <Col xs="1">
        <button
          className="btn btn-link"
          onClick={() => props.deleteWorkItem()}>
          <FontAwesomeIcon icon={faTrashAlt} />
        </button>
      </Col>
    </Row>
  );
}

export const WorkItemsForm = (props: Props) => {

  const { control, getValues } = useFormContext();

  const { getPath } = useFormHelpers(props.path);

  return (
    <div className="activity-line">
      <Row>
        <Col xs="5">
          <label>{props.category}</label>
        </Col>
        <Col xs="3">
          <label>{props.category === "Labor" ? "Time" : "Unit"}</label>
        </Col>
        <Col xs="2">
          <label>Rate</label>
        </Col>
        <Col xs="2">
          <label>Total</label>
        </Col>
      </Row>
      {props.workItemsHelper.fields.map((field, i) => ({ field, i, path: getPath(i.toString()) }))
        .filter(item => (item.field as unknown as WorkItem).type === props.category)
        .map((item) => (<div key={item.field.fieldId}>
          <WorkItemForm
            category={props.category}
            rates={props.rates}
            deleteWorkItem={() => props.workItemsHelper.remove(item.i)}
            path={item.path}
            defaultValue={item.field as unknown as WorkItem}
          />
        </div>))}
      <Button
        className="btn-sm"
        color="link"
        onClick={() => props.workItemsHelper.append(props.getNewWorkItem(props.category))}
      >
        <FontAwesomeIcon className="mr-2" icon={faPlus} />
        Add {props.category}
      </Button>
    </div >
  );
};
