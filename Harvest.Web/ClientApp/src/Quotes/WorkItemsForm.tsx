import React, { useEffect } from "react";
import {
  Button,
  Col,
  FormGroup,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Row,
} from "reactstrap";
import { FieldArray } from "formik";

import { Rate, RateType, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { getInputValidityStyle, ValidationErrorMessage, FormikBag } from "../Validation";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt } from "@fortawesome/free-solid-svg-icons";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface Props {
  category: RateType;
  rates: Rate[];
  getNewWorkItem: (category: RateType) => WorkItem;
  formik: FormikBag<any, WorkItem[]>;
}

interface WorkItemProps {
  category: RateType;
  rates: Rate[];
  formik: FormikBag<any, WorkItem>;
  deleteWorkItem: () => void;
}

const WorkItemForm = (props: WorkItemProps) => {
  const { formik } = props;

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string | null) => {
    return props.category === "Other" && unit === "Unit";
  };

  useEffect(() => {
    const meta = formik.getFieldMeta("rateId");
    if (meta.touched && meta.error !== undefined && meta.error !== "") {
      const rate = props.rates.find((r) => r.id === formik.values.rateId);

      // rate can be undefinied if they select the default option
      if (!!rate) {
        // new rate selected, update the work item with defaults
        formik.setFieldValue("rate", rate.price);
        formik.setFieldValue("description", requiresCustomDescription(rate.unit) ? "" : rate.description);
        formik.setFieldValue("unit", rate.unit);
      } else {
        formik.setFieldValue("rate", 0);
        formik.setFieldValue("description", "");
        formik.setFieldValue("unit", "");
      }
    }
  }, [formik.values.rateId]);

  useEffect(() => {
    formik.setFieldValue("total", formik.values.rate * formik.values.quantity);
  }, [formik.values.rate, formik.values.quantity]);

  return (
    <Row
      className="activity-line-item"
      key={formik.fullPath("id")}>
      <Col xs="5">
        <FormGroup>
          <select
            className={`form-control ${getInputValidityStyle(formik, "rateId")}`}
            id={formik.fullPath("rateId")}
            name={formik.fullPath("rateId")}
            value={formik.values.rateId}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
          >
            <option value="0">-- Select {props.category} --</option>
            {props.rates.filter((r) => r.type === props.category).map((r) => (
              <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                {r.description}
              </option>
            ))}
          </select>

          {requiresCustomDescription(formik.values.unit) && (
            <input
              className={`form-control ${getInputValidityStyle(formik, "description")}`}
              id={formik.fullPath("description")}
              name={formik.fullPath("description")}
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
            type="number"
            id={formik.fullPath("quantity")}
            name={formik.fullPath("quantity")}
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
            className={`form-control ${getInputValidityStyle(formik,"rate")}`}
            type="number"
            id={formik.fullPath("rate")}
            name={formik.fullPath("rate")}
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
          onClick={() => props.deleteWorkItem()}>
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
      <FieldArray name={props.formik.path}>
        {(arrayHelpers) => (
          <div>
            { props.formik.values
              .map((w, i) => ({ workItem: w, i }))
              .filter((item) => item.workItem.type === props.category)
              .map((item) => (
                <WorkItemForm
                  key={`workitem-${item.i}-${item.workItem.id}`}
                  category={props.category}
                  rates={props.rates}
                  formik={props.formik.getNestedBag((workItems) => workItems[item.i])}
                  deleteWorkItem={() => arrayHelpers.remove(item.i)}
                />))}
            <Button
              className="btn-sm"
              color="link"
              onClick={() => arrayHelpers.insert(props.formik.values.length, props.getNewWorkItem(props.category))}
            >
              <FontAwesomeIcon className="mr-2" icon={faPlus} />
              Add {props.category}
            </Button>
          </div>
        )}
      </FieldArray>
      <div>Debug: {JSON.stringify(props.formik.values)}</div>
    </div >
  );
};
