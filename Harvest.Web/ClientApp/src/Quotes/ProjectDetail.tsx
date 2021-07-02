import React, { useCallback } from "react";
import { useFormContext, useWatch } from "react-hook-form";

import { QuoteContent, Rate, Field } from "../types";
import { ValidationErrorMessage, useFormHelpers } from "../Validation";

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
  setEditFields: React.Dispatch<React.SetStateAction<boolean>>;
  addActivity: () => void;
}

export const ProjectDetail = (props: Props) => {

  const { setValue, control, formState: { errors }, register } = useFormContext<QuoteContent>();
  const { getInputValidityStyle } = useFormHelpers("");

  const [acres, acreageRate, acreageRateId, fields] = useWatch({
    control,
    name: ["acres", "acreageRate", "acreageRateId", "fields"]
  }) as [number, number, number, Field[]];

  React.useEffect(() => {
    const rate = props.rates.find((r) => r.id === acreageRateId);

    // rate can be undefinied if they select the default option
    if (!!rate) {
      setValue("acreageRate", rate.price);
      //setValue("acreageRateId", rate.id);
      setValue("acreageRateDescription", rate.description);
    //} else {
    //  setValue("acreageRate", 0);
    //  //setValue("acreageRateId", 0);
    //  setValue("acreageRateDescription", "");
    }
  }, [acreageRateId]);

  return (
    <Row className="align-items-baseline">
      {/* Left Details */}
      <Col md="6">
        <FormGroup>
          <Label for="projectName">Project Name</Label>
          <input
            className={`form-control ${getInputValidityStyle("projectName")}`}
            {...register("projectName")}
          />
        </FormGroup>
        <ValidationErrorMessage name="projectName" />
        <br />
        <Row className="align-items-baseline">
          <Col>
            <FormGroup>
              <Label>Acreage Rate</Label>
              <select
                className={`form-control ${getInputValidityStyle("acreageRateId")}`}
                {...register("acreageRateId")}
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
            <ValidationErrorMessage name="acreageRateId" />
          </Col>
        </Row>
        <br />
        {acreageRateId > 0 && (
          <Row className="align-items-baseline">
            <Col md="4">
              <FormGroup>
                <Label for="acres">Number of Acres</Label>
                <input
                  className={`form-control ${getInputValidityStyle("acres")}`}
                  type="number"
                  {...register("acres", { valueAsNumber: true })}
                />
              </FormGroup>
              <ValidationErrorMessage name="acres" />
            </Col>
            <Col md="4">
              <FormGroup>
                <Label for="rate">Rate</Label>
                <InputGroup>
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>$</InputGroupText>
                  </InputGroupAddon>
                  <input
                    className={`form-control ${getInputValidityStyle("acreageRate")}`}
                    type="number"
                    {...register("acreageRate", { valueAsNumber: true })}
                  />
                </InputGroup>
              </FormGroup>
              <ValidationErrorMessage name="acreageRate" />

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
                      acres * acreageRate
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
        <Location fields={fields as Field[] || []}></Location>
        <br />
        <div id="map" />
      </Col>
    </Row>
  );
};
