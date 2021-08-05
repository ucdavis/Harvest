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

import { Rate, RateType, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { useInputValidator } from "../FormValidation";
import { workItemSchema } from "../schemas";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt } from "@fortawesome/free-solid-svg-icons";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface WorkItemsFormProps {
  adjustment: number;
  category: RateType;
  rates: Rate[];
  workItems: WorkItem[];
  updateWorkItems: (workItem: WorkItem) => void;
  addNewWorkItem: (type: RateType) => void;
  deleteWorkItem: (workItem: WorkItem) => void;
}

interface WorkItemFormProps {
  adjustment: number;
  category: RateType;
  rates: Rate[];
  workItem: WorkItem;
  updateWorkItems: (workItem: WorkItem) => void;
  deleteWorkItem: (workItem: WorkItem) => void;
}

const WorkItemForm = (props: WorkItemFormProps) => {
  const { workItem } = props;

  const { onChange, InputErrorMessage, getClassName, onBlur } = useInputValidator<WorkItem>(workItemSchema);

  const rateItemChanged = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const rateId = parseInt(e.target.value);
    const rate = props.rates.find((r) => r.id === rateId);

    // rate can be undefinied if they select the default option
    if (rate !== undefined) {
      // new rate selected, update the work item with defaults
      props.updateWorkItems({
        ...workItem,
        description: requiresCustomDescription(rate.unit)
          ? ""
          : rate.description,
        rateId,
        rate: rate.price,
        unit: rate.unit,
        total: 0,
      });
    }
  };

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string) => {
    return props.category === "Other" && unit === "Unit";
  };

  return <Row
    className="activity-line-item"

  >
    <Col xs="5">
      <FormGroup>
        <Input
          className={getClassName("rateId")}
          type="select"
          name="select"
          defaultValue={workItem.rateId}
          onChange={onChange("rateId", rateItemChanged)}
          onBlur={onBlur("rateId")}
        >
          <option value="0">-- Select {props.category} --</option>
          {props.rates.map((r) => (
            <option key={`rate-${r.type}-${r.id}`} value={r.id}>
              {r.description}
            </option>
          ))}
        </Input>
        <InputErrorMessage name="rateId" />
        {requiresCustomDescription(workItem.unit) && (<>
          <Input
            className={getClassName("description")}
            type="text"
            name="OtherDescription"
            value={workItem.description}
            placeholder="Description"
            onChange={onChange("description", (e) =>
              props.updateWorkItems({
                ...workItem,
                description: e.target.value,
              }))
            }
            onBlur={onBlur("description")}
          ></Input>
          <InputErrorMessage name="description" />
        </>)}
      </FormGroup>
    </Col>

    <Col className="col-sm-2">
      <InputGroup>
        <InputGroupAddon addonType="prepend">
          <InputGroupText>{workItem.unit || ""}</InputGroupText>
        </InputGroupAddon>
        <Input
          className={getClassName("quantity")}
          type="number"
          id="units"
          value={workItem.quantity}
          onChange={onChange("quantity", (e) => 
            props.updateWorkItems({
              ...workItem,
              quantity: parseFloat(e.target.value ?? 0),
            }))
          }
          onBlur={onBlur("quantity")}
        />
      </InputGroup>
      <InputErrorMessage name="quantity" />
    </Col>

    <Col className="col-sm-2 offset-sm-1">
      ${formatCurrency(workItem.rate || 0)}
      {props.adjustment > 0 && (
        <span className="primary-color">
          {" "}
                + ${formatCurrency(workItem.rate * (props.adjustment / 100))}
        </span>
      )}
    </Col>

    <Col xs="1">${formatCurrency(workItem.total)}</Col>

    <Col xs="1">
      <button
        className="btn btn-link mt-0"
        onClick={() => props.deleteWorkItem(workItem)}
      >
        <FontAwesomeIcon icon={faTrashAlt} />
      </button>
    </Col>
  </Row>
};


export const WorkItemsForm = (props: WorkItemsFormProps) => {
  const rateItemChanged = (
    e: React.ChangeEvent<HTMLInputElement>,
    workItem: WorkItem
  ) => {
    const rateId = parseInt(e.target.value);
    const rate = props.rates.find((r) => r.id === rateId);

    // rate can be undefinied if they select the default option
    if (rate !== undefined) {
      // new rate selected, update the work item with defaults
      props.updateWorkItems({
        ...workItem,
        description: requiresCustomDescription(rate.unit)
          ? ""
          : rate.description,
        rateId,
        rate: rate.price,
        unit: rate.unit,
        total: 0,
      });
    }
  };

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string) => {
    return props.category === "Other" && unit === "Unit";
  };

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
      {props.workItems.map((workItem) => (
        <WorkItemForm
          key={`workItem-${workItem.id}-activity-${workItem.activityId}`}
          adjustment={props.adjustment}
          category={props.category}
          rates={props.rates}
          workItem={workItem}
          updateWorkItems={props.updateWorkItems}
          deleteWorkItem={props.deleteWorkItem}
        />))}
      <Button
        className="btn-sm"
        color="link"
        onClick={() => props.addNewWorkItem(props.category)}
      >
        <FontAwesomeIcon className="mr-2" icon={faPlus} />
        Add {props.category}
      </Button>
    </div>
  );
};
