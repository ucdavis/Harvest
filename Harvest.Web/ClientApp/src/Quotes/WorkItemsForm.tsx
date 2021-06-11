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
import { useFormik, FormikConfig } from "formik";

import { Rate, RateType, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { getInputValidityStyle, ValidationErrorMessage, UseFormikType } from "../Validation";
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

  const formik = useFormik<WorkItem>({
    initialValues: workItem,
    validationSchema: workItemSchema
  } as FormikConfig<WorkItem>);


  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string | null) => {
    return props.category === "Other" && unit === "Unit";
  };

  const rateItemChanged = (
    e: React.ChangeEvent<HTMLSelectElement>,
  ) => {
    //let formik do it's validation and casting magic
    formik.handleChange(e);

    const meta = formik.getFieldMeta("rateId");
    if (meta.touched && meta.error !== undefined && meta.error !== "") {
      const rate = props.rates.find((r) => r.id === formik.values.rateId);

      // rate can be undefinied if they select the default option
      if (!!rate) {
        // new rate selected, update the work item with defaults
        formik.setFieldValue("rate", rate.price);
        formik.setFieldValue("description", requiresCustomDescription(rate.unit) ? "" : rate.description);
        formik.setFieldValue("unit", rate.unit);
        formik.setFieldValue("total", 0);
      }
    }
  };

  return (
    <Row
      className="activity-line-item"
      key={`workItem-${formik.values.id}-activity-${formik.values.activityId}`}>
      <Col xs="5">
        <FormGroup>
          <select
            className={`form-control ${getInputValidityStyle(formik, "rateId")}`}
            id="rateId"
            name="rateId"
            value={formik.values.rateId}
            onChange={(e) => rateItemChanged(e)}
            onBlur={formik.handleBlur}
          >
            <option value="0">-- Select {props.category} --</option>
            {props.rates.map((r) => (
              <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                {r.description}
              </option>
            ))}
          </select>

          {requiresCustomDescription(formik.values.unit) && (
            <input
              className={`form-control ${getInputValidityStyle(formik, "description")}`}
              id="description"
              name="description"
              value={formik.values.description}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
            />
          )}
        </FormGroup>
        <ValidationErrorMessage formik={formik} name="rateId" />
        <ValidationErrorMessage formik={formik} name="description" />
      </Col>

      <Col xs="3">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>{formik.values.unit || ""}</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle(formik, "quantity")}`}
            id="quantity"
            name="quantity"
            value={formik.values.quantity}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
          />
        </InputGroup>
        <ValidationErrorMessage formik={formik} name="quantity" />
      </Col>

      <Col xs="2">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>$</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle(formik, "rate")}`}
            id="rate"
            name="rate"
            value={formik.values.rate}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
          />
        </InputGroup>
        <ValidationErrorMessage formik={formik} name="rate" />
      </Col>

      <Col xs="1">${formatCurrency(formik.values.rate * formik.values.quantity)}</Col>

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
