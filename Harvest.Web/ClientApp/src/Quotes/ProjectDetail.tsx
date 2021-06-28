import React from "react";

import { QuoteContent, Rate, WorkItemImpl } from "../types";
import { FormikBag, ValidationErrorMessage, getInputValidityStyle } from "../Validation";

import {
  Button,
  Col,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Label,
  Row,
  FormGroup
} from "reactstrap";

import { formatCurrency } from "../Util/NumberFormatting";
import { Location } from "../Fields/Location";

interface Props {
  rates: Rate[];
  formik: FormikBag<QuoteContent>;
  setEditFields: React.Dispatch<React.SetStateAction<boolean>>;
  addActivity: () => void;
}

export const ProjectDetail = (props: Props) => {
  const { formik } = props;
  
  React.useEffect(() => {
    const rate = props.rates.find((r) => r.id === formik.values.acreageRateId);

    // rate can be undefinied if they select the default option
    if (!!rate) {
      formik.setFieldValue("acreageRate", rate.price);
      formik.setFieldValue("acreageRateId", rate.id);
      formik.setFieldValue("acreageRateDescription", rate.description);
    } else {
      formik.setFieldValue("acreageRate", 0);
      formik.setFieldValue("acreageRateId", 0);
      formik.setFieldValue("acreageRateDescription", "");
    }
  }, [formik.values.acreageRateId]);

  return (
    <Row className="align-items-baseline">
      {/* Left Details */}
      <Col md="6">
        <FormGroup>
          <Label for="projectName">Project Name</Label>
          <input
            className={`form-control ${getInputValidityStyle(formik, "projectName")}`}
            id="projectName"
            name="projectName"
            value={formik.values.projectName}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
          />
        </FormGroup>
        <ValidationErrorMessage formik={formik} name="projectName" />
        <br />
        <Row className="align-items-baseline">
          <Col>
            <FormGroup>
              <Label>Acreage Rate</Label>
              <select
                className={`form-control ${getInputValidityStyle(formik, "acreageRate")}`}
                id="acreageRate"
                name="acreageRate"
                value={formik.values.acreageRateId}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
              >
                <option value="0">-- Select Acreage Rate --</option>
                {props.rates
                  .filter((r) => r.type === "Acreage")
                  .map((r) => (
                    <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                      {r.description}
                    </option>
                  ))}
              </select>
            </FormGroup>
            <ValidationErrorMessage formik={formik} name="acreageRateId" />
          </Col>
        </Row>
        <br />
        {formik.values.acreageRateId > 0 && (
          <Row className="align-items-baseline">
            <Col md="4">
              <FormGroup>
                <Label for="acres">Number of Acres</Label>
                <input
                  className={`form-control ${getInputValidityStyle(formik, "acres")}`}
                  type="number"
                  id="acres"
                  name="acres"
                  value={formik.values.acres}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                />
              </FormGroup>
              <ValidationErrorMessage formik={formik} name="acres" />
            </Col>
            <Col md="4">
              <FormGroup>
                <Label for="rate">Rate</Label>
                <InputGroup>
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>$</InputGroupText>
                  </InputGroupAddon>
                  <input
                    className={`form-control ${getInputValidityStyle(formik, "acreageRate")}`}
                    type="number"
                    id="rate"
                    name="rate"
                    value={formik.values.acreageRate}
                    onChange={formik.handleChange}
                    onBlur={formik.handleBlur}
                  />
                </InputGroup>
              </FormGroup>
              <ValidationErrorMessage formik={formik} name="acreageRate" />

            </Col>
            <Col md="4">
              <FormGroup>
                <Label>Total Acreage Fee</Label>
                <InputGroup>
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>$</InputGroupText>
                  </InputGroupAddon>
                  <input
                    className="form-control"
                    type="text"
                    id="rate"
                    readOnly
                    value={formatCurrency(
                      formik.values.acres * formik.values.acreageRate
                    )}
                  />
                </InputGroup>
              </FormGroup>
            </Col>
          </Row>
        )}

        <br />
        <Button
          className="mb-4"
          color="primary"
          size="lg"
          onClick={props.addActivity}
        >
          Add Activity
        </Button>
      </Col>

      {/* Right Details */}
      <Col md="6">
        <Row>
          <Col>
            <Label for="projectLocation">Project Location</Label>
          </Col>
          <Col>
            <button
              className="btn btn-link"
              onClick={(_) => props.setEditFields(true)}
            >
              Edit Fields
            </button>
          </Col>
        </Row>
        <Location fields={formik.values.fields}></Location>
        <br />
        <div id="map" />
      </Col>
    </Row>
  );
};
