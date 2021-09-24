import React, { useRef } from "react";
import {
  Button,
  Col,
  CustomInput,
  FormGroup,
  Input,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Row,
  UncontrolledTooltip,
} from "reactstrap";
import { Typeahead } from "react-bootstrap-typeahead";

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

  const {
    onChange,
    onChangeValue,
    InputErrorMessage,
    getClassName,
    onBlur,
    onBlurValue,
    resetLocalFields,
  } = useInputValidator(workItemSchema, props.workItem);

  const rateItemChanged = (selected: Rate) => {
    const rateId = selected.id;
    const rate = props.rates.find((r) => r.id === rateId);

    // rate can be undefinied if they select the default option
    if (rate !== undefined) {
        // new rate selected, update the work item with defaults
      props.updateWorkItems({
        ...workItem,
        description: requiresCustomDescription(rate.isPassThrough)
          ? ""
          : rate.description,
        rateId,
        rate: rate.price,
        unit: rate.unit,
        total: 0,
        isPassthrough: rate.isPassThrough,
      });
    } else {
      // reset values to prevent stale data from impacting logic elsewhere
      props.updateWorkItems({
        ...workItem,
        rateId: 0,
        rate: 0,
        unit: "",
        description: "",
        quantity: 0,
        total: 0,
        isPassthrough: false,
      });
    }
  };

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (isPass: boolean) => {
    return isPass;
  };

  const typeaheadRef = useRef<any>(null);
  const selectedRate = props.rates.filter(
    (rate) => rate.id === props.workItem.rateId
  );

  const typeaheadChange = (selected: Rate) => {
    if (selected) {
      onChangeValue("rateId")(selected.id);
      rateItemChanged(selected);
    } else {
      // When clearButton is called it calls the onChange function
      props.updateWorkItems({
        ...workItem,
        rateId: 0,
        rate: 0,
        unit: "",
        description: "",
        quantity: 0,
        total: 0,
        isPassthrough: false,
      });
    }
  };

  const typeaheadBlur = (e: Event) => {
    if (selectedRate.length === 0) {
      typeaheadRef.current.clear();

      props.updateWorkItems({
        ...workItem,
        rateId: 0,
        rate: 0,
        unit: "",
        description: "",
        quantity: 0,
        total: 0,
        isPassthrough: false,
      });
    }

    const target = e.target as HTMLInputElement;
    const rate = props.rates.find((r) => r.description === target.value);
    onBlurValue("rateId", rate?.id);
  };

  return (
    <Row className="activity-line-item">
      <Col xs={props.category === "Other" ? 4 : 5}>
        <FormGroup>
          <Typeahead
            id={`typeahead-${props.category}`}
            clearButton
            ref={typeaheadRef}
            className={getClassName("rateId")}
            placeholder={`-- Select ${props.category} --`}
            labelKey="description"
            options={props.rates}
            selected={selectedRate}
            onChange={(selected) => typeaheadChange(selected[0])}
            onBlur={(e) => typeaheadBlur(e)}
          />
          <InputErrorMessage name="rateId" />
          {requiresCustomDescription(workItem.isPassthrough) && (
            <>
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
                  })
                )}
                onBlur={onBlur("description")}
              ></Input>
              <InputErrorMessage name="description" />
            </>
          )}
        </FormGroup>
      </Col>

      {props.category === "Other" && (
        <Col className="col-sm-1 text-center">
          <CustomInput
            type="switch"
            id="markup"
            checked={workItem.markup}
            onChange={(e) => {
              props.updateWorkItems({
                ...workItem,
                markup: e.target.checked,
              });
            }}
          />
        </Col>
      )}

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
              })
            )}
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

      <Col xs="1" className="trash-col">
        <button
          className="btn btn-link mt-0"
          onClick={() => {
            resetLocalFields();
            props.deleteWorkItem(workItem);
          }}
        >
          <FontAwesomeIcon icon={faTrashAlt} />
        </button>
      </Col>
    </Row>
  );
};

export const WorkItemsForm = (props: WorkItemsFormProps) => {
  return (
    <div className="activity-line">
      <Row>
        {props.category === "Other" ? (
          <>
            <Col xs="4">
              <label>{props.category}</label>
            </Col>
            <Col xs="1">
              <label id="markupLabel">Markup</label>
              <UncontrolledTooltip placement="right" target="markupLabel">
                Adds a 20% parts markup to the total price
              </UncontrolledTooltip>
            </Col>
          </>
        ) : (
          <Col xs="5">
            <label>{props.category}</label>
          </Col>
        )}
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
        />
      ))}
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
