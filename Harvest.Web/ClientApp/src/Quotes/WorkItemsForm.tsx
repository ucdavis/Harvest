import React from "react";
import {
  Button,
  Col,
  FormGroup,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Row,
} from "reactstrap";
import { useForm } from "react-hook-form";
import { yupResolver } from '@hookform/resolvers/yup';

import { Rate, RateType, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { getInputValidityStyle, ValidationErrorMessage } from "../Validation";
import { workItemSchema } from "../schemas";

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

const WorkItemForm = (props: WorkItemProps) => {
  const { workItem } = props;

  const { register, handleSubmit, formState, setValue, watch } = useForm<WorkItem>({
    defaultValues: workItem,
    resolver: yupResolver(workItemSchema),
    mode: "onBlur"
  });

  // registering rateId prior to render in order to get access to onChange event handler
  const registeredRateId = register("rateId");

  // get current values of given fields
  const watchRateId = watch("rateId");
  const watchUnit = watch("unit");
  const watchRate = watch("rate");
  const watchQuantity = watch("quantity");


  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string) => {
    return props.category === "Other" && unit === "Unit";
  };

  const rateItemChanged = (
    e: React.ChangeEvent<HTMLSelectElement>,
  ) => {
    //let react-hook-form do it's validation and casting magic
    registeredRateId.onChange(e);

    
    if (!formState.errors.rateId) {
      const rate = props.rates.find((r) => r.id === watchRateId);

      // rate can be undefinied if they select the default option
      if (!!rate) {
        // new rate selected, update the work item with defaults
        setValue("rate", rate.price);
        setValue("description", requiresCustomDescription(rate.unit) ? "" : rate.description);
        setValue("unit", rate.unit);
        setValue("total", 0);
      }
    }
  };

  return (
    <Row
      className="activity-line-item"
      key={`workItem-${workItem.id}-activity-${workItem.activityId}`}>
      <Col xs="5">
        <FormGroup>
          <select
            className={`form-control ${getInputValidityStyle(formState.errors.rateId)}`}
            id="rateId"
            {...registeredRateId}
            onChange={(e) => rateItemChanged(e)}
          >
            <option value="0">-- Select {props.category} --</option>
            {props.rates.map((r) => (
              <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                {r.description}
              </option>
            ))}
          </select>

          {requiresCustomDescription(watchUnit) && (
            <input
              className={`form-control ${getInputValidityStyle(formState.errors.description)}`}
              id="description"
              {...register("description")}
            />
          )}
        </FormGroup>
        <ValidationErrorMessage error={formState.errors.rateId} />
        <ValidationErrorMessage error={formState.errors.description} />
      </Col>

      <Col xs="3">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>{watchUnit}</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle(formState.errors.quantity)}`}
            id="quantity"
            {...register("quantity")}
          />
        </InputGroup>
        <ValidationErrorMessage error={formState.errors.quantity} />
      </Col>

      <Col xs="2">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>$</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle(formState.errors.rate)}`}
            id="rate"
            {...register("rate")}
          />
        </InputGroup>
        <ValidationErrorMessage error={formState.errors.rate} />
      </Col>

      <Col xs="1">${formatCurrency(watchRate * watchQuantity)}</Col>

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
