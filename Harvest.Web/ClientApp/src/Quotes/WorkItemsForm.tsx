import React from "react";
import {
  Button,
  Col,
  FormGroup,
  Input,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Row,
} from "reactstrap";
import { InputElement, useFormState, FormState, StateErrors } from 'react-use-form-state';

import { Rate, RateType, WorkItem, WorkItemSchema } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt } from "@fortawesome/free-solid-svg-icons";

interface Props {
  category: RateType;
  rates: Rate[];
  workItems: WorkItem[];
  updateWorkItems: (workItem: WorkItem) => void;
  addNewWorkItem: (type: RateType) => void;
  deleteWorkItem: (workItem: WorkItem) => void;
}

interface WorkItemProps {
  category: RateType;
  rates: Rate[];
  workItem: WorkItem;
  updateWorkItems: (workItem: WorkItem) => void;
  //addNewWorkItem: (type: RateType) => void;
  deleteWorkItem: (workItem: WorkItem) => void;
}

interface ValidationMessageProps {
  formState: FormState<any, StateErrors<any, string>>;
  name: string;
}

const ValidationErrorMessage = (props: ValidationMessageProps) => {
  const { formState, name } = props;
  return formState.touched[name] && formState.validity[name] === false
    ? (<p className="text-danger">{formState.errors[name]}</p>)
    : (null);
}

const InputValidityStyle = (formState: FormState<any, StateErrors<any, string>>, name: string) => {
  return formState.touched[name] && (formState.validity[name] === false) && "is-invalid";
}

const WorkItemForm = (props: WorkItemProps) => {
  const { workItem } = props;

  const [formState, { text, select, number }] = useFormState<WorkItem>(workItem,
    {
      onChange(e, stateValues, nextStateValues) {
        const result = WorkItemSchema.safeParse({
          ...nextStateValues,
          rate: Number(nextStateValues.rate),
          quantity: Number(nextStateValues.quantity)
        });
        if (result.success) {
          props.updateWorkItems(result.data);
        }
      }
    });
  console.debug(JSON.stringify(formState));

  const validateField = (field: keyof WorkItem) =>
    (value: unknown): string | true => {
      const parsedResult = WorkItemSchema
        .pick({ [field]: true })
        .safeParse({ [field]: value });
      return !parsedResult.success
        ? parsedResult.error.errors[0].message
        : true;
    }

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string | null) => {
    return props.category === "Other" && unit === "Unit";
  };

  const rateItemChanged = (
    e: React.ChangeEvent<InputElement>,
    workItem: WorkItem
  ) => {
    const rateId = parseInt(e.target.value);
    const rate = props.rates.find((r) => r.id === rateId);

    // rate can be undefinied if they select the default option
    if (!!rate) {
      // new rate selected, update the work item with defaults
      formState.setField("rateId", rateId);
      formState.setField("rate", rate.price);
      formState.setField("description", requiresCustomDescription(rate.unit) ? "" : rate.description);
      formState.setField("unit", rate.unit);
      formState.setField("total", 0);
    }
  };

  return (
    <Row
      className="activity-line-item"
      key={`workItem-${workItem.id}-activity-${workItem.activityId}`}>
      <Col xs="5">
        <FormGroup>
          <select
            className={`form-control ${InputValidityStyle(formState, "rateId")}`}
            {...select({
              name: "rateId",
              onChange: (e) => rateItemChanged(e, workItem),
              validate: (value) => validateField("rateId")(parseInt(value))
            })}>
            <option value="0">-- Select {props.category} --</option>
            {props.rates.map((r) => (
              <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                {r.description}
              </option>
            ))}
          </select>

          {requiresCustomDescription(workItem.unit) && (
            <input
              className={`form-control ${InputValidityStyle(formState, "description")}`}
              {...text({
                name: "description",
                validate: validateField("description")
              })}
            ></input>
          )}
        </FormGroup>
        <ValidationErrorMessage formState={formState} name="rateId" />
        <ValidationErrorMessage formState={formState} name="description" />
      </Col>

      <Col xs="3">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>{workItem.unit || ""}</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${InputValidityStyle(formState, "quantity")}`}
            {...number({
              name: "quantity",
              validate: (value) => validateField("quantity")(parseFloat(value))
            })} />
        </InputGroup>
        <ValidationErrorMessage formState={formState} name="quantity" />
      </Col>

      <Col xs="2">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>$</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${InputValidityStyle(formState, "rate")}`}
            {...number({
              name: "rate",
              validate: (value) => validateField("rate")(parseFloat(value))
            })} />
        </InputGroup>
        <ValidationErrorMessage formState={formState} name="rate" />
      </Col>

      <Col xs="1">${formatCurrency(workItem.rate * workItem.quantity)}</Col>

      <Col xs="1">
        <button
          className="btn btn-link"
          onClick={() => props.deleteWorkItem(workItem)}>
          <FontAwesomeIcon icon={faTrashAlt} />
        </button>
      </Col>
    </Row>
  );
}

export const WorkItemsForm = (props: Props) => {


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
      {props.workItems.map((workItem) => <WorkItemForm workItem={workItem} {...props} />)}
      <Button
        className="btn-sm"
        color="link"
        onClick={() => props.addNewWorkItem(props.category)}
      >
        Add {props.category}
      </Button>
    </div>
  );
};
