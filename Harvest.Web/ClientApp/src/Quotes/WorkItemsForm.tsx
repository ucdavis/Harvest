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
import { useFormik, FormikConfig, FieldArray } from "formik";

import { Rate, RateType, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { getInputValidityStyle, ValidationErrorMessage, FormikState } from "../Validation";
import { workItemSchema } from "../schemas";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt } from "@fortawesome/free-solid-svg-icons";

interface Props {
  category: RateType;
  rates: Rate[];
  workItems: WorkItem[];
  getNewWorkItem: (category: RateType) => WorkItem;
  formik: FormikState<any>;
  path: string;
}

interface WorkItemProps {
  category: RateType;
  rates: Rate[];
  workItem: WorkItem;
  path: string;
  formik: FormikState<any>;
  deleteWorkItem: () => void;
}

const WorkItemForm = (props: WorkItemProps) => {
  const { formik, path } = props;

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string | null) => {
    return props.category === "Other" && unit === "Unit";
  };

  React.useEffect(() => {
    const meta = formik.getFieldMeta(`${path}.rateId`);
    if (meta.touched && meta.error !== undefined && meta.error !== "") {
      const rate = props.rates.find((r) => r.id === formik.values.rateId);

      // rate can be undefinied if they select the default option
      if (!!rate) {
        // new rate selected, update the work item with defaults
        formik.setFieldValue(`${path}.rate`, rate.price);
        formik.setFieldValue(`${path}.description`, requiresCustomDescription(rate.unit) ? "" : rate.description);
        formik.setFieldValue(`${path}.unit`, rate.unit);
        formik.setFieldValue(`${path}.total`, 0);
      }
    }  }, [props.workItem.rateId]);

  //const rateItemChanged = (
  //  e: React.ChangeEvent<HTMLSelectElement>,
  //) => {
  //  //let formik do it's validation and casting magic
  //  formik.handleChange(e);

  //  const meta = formik.getFieldMeta("rateId");
  //  if (meta.touched && meta.error !== undefined && meta.error !== "") {
  //    const rate = props.rates.find((r) => r.id === formik.values.rateId);

  //    // rate can be undefinied if they select the default option
  //    if (!!rate) {
  //      // new rate selected, update the work item with defaults
  //      formik.setFieldValue("rate", rate.price);
  //      formik.setFieldValue("description", requiresCustomDescription(rate.unit) ? "" : rate.description);
  //      formik.setFieldValue("unit", rate.unit);
  //      formik.setFieldValue("total", 0);
  //    }
  //  }
  //};

  return (
    <Row
      className="activity-line-item"
      key={`${path}.${props.workItem.id}`}>
      <Col xs="5">
        <FormGroup>
          <select
            className={`form-control ${getInputValidityStyle(formik, `${path}.rateId`)}`}
            id={`${path}.rateId`}
            name={`${path}.rateId`}
            value={props.workItem.rateId}
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
              className={`form-control ${getInputValidityStyle(formik, `${path}.description`)}`}
              id={`${path}.description`}
              name={`${path}.description`}
              value={props.workItem.description}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
            />
          )}
        </FormGroup>
        <ValidationErrorMessage formik={formik} name={`${path}.rateId`} />
        <ValidationErrorMessage formik={formik} name={`${path}.description`} />
      </Col>

      <Col xs="3">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>{props.workItem.unit || ""}</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle(formik, `${path}.quantity`)}`}
            id={`${path}.quantity`}
            name={`${path}.quantity`}
            value={props.workItem.quantity}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
          />
        </InputGroup>
        <ValidationErrorMessage formik={formik} name={`${path}.quantity`} />
      </Col>

      <Col xs="2">
        <InputGroup>
          <InputGroupAddon addonType="prepend">
            <InputGroupText>$</InputGroupText>
          </InputGroupAddon>
          <input
            className={`form-control ${getInputValidityStyle(formik, `${path}.rate`)}`}
            id={`${path}.rate`}
            name={`${path}.rate`}
            value={props.workItem.rate}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
          />
        </InputGroup>
        <ValidationErrorMessage formik={formik} name={`${path}.rate`} />
      </Col>

      <Col xs="1">${formatCurrency(props.workItem.rate * props.workItem.quantity)}</Col>

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
      <FieldArray name={`${props.path}.workItems`}>
        {(arrayHelpers) => (
          <div>
            { props.workItems
              .map((w, i) => ({ workItem: w, i }))
              .filter((item) => item.workItem.type === props.category)
              .map((item) => (
                <WorkItemForm
                  key={`workitem-${item.workItem.id}`}
                  workItem={item.workItem}
                  {...props}
                  path={`${props.path}.workItems.${item.i}`}
                  deleteWorkItem={() => arrayHelpers.remove(item.i)}
                />))}
            <Button
              className="btn-sm"
              color="link"
              onClick={() => arrayHelpers.push(props.getNewWorkItem(props.category))}
            >
              Add {props.category}
            </Button>
          </div>
        )}
      </FieldArray>
    </div >
  );
};
