import React from "react";

import { QuoteContent, Rate } from "../types";

import { Col, Input, Label, Row } from "reactstrap";

import { formatCurrency } from "../Util/NumberFormatting";
import { Location } from "../Fields/Location";
import { useInputValidator } from "use-input-validator";
import { quoteContentSchema } from "../schemas";
import { validatorOptions } from "../constants";

interface Props {
  rates: Rate[];
  quote: QuoteContent;
  updateQuote: React.Dispatch<React.SetStateAction<QuoteContent>>;
  setEditFields: React.Dispatch<React.SetStateAction<boolean>>;
}

export const ProjectDetail = (props: Props) => {
  const { onChange, InputErrorMessage, getClassName, onBlur } =
    useInputValidator(quoteContentSchema, props.quote, validatorOptions);

  const setAcreageRate = (rate: Rate | undefined) => {
    if (rate) {
      props.updateQuote({
        ...props.quote,
        acreageRate: rate.price,
        acreageRateId: rate.id,
        acreageRateDescription: rate.description,
      });
    } else {
      props.updateQuote({
        ...props.quote,
        acreageRate: 0,
        acreageRateId: null,
        acreageRateDescription: "",
      });
    }
  };
  return (
    <Row className="align-items-baseline">
      {/* Left Details */}
      <Col lg="6" md="8">
        <Label for="projectName">Project Name</Label>
        <Input
          className={getClassName("projectName")}
          type="text"
          id="projectName"
          value={props.quote.projectName}
          onChange={onChange("projectName", (e) =>
            props.updateQuote({ ...props.quote, projectName: e.target.value })
          )}
          onBlur={onBlur("projectName")}
        />
        <InputErrorMessage name="projectName" />
        <br />
        <Row className="align-items-baseline">
          <Col>
            <Label>Acreage Rate</Label>
            <Input
              className={getClassName("acreageRateId")}
              type="select"
              name="acreageRate"
              value={props.quote.acreageRateId || undefined}
              onChange={onChange("acreageRateId", (e) =>
                setAcreageRate(
                  props.rates.find((r) => r.id === parseInt(e.target.value))
                )
              )}
              onBlur={onBlur("acreageRateId")}
            >
              <option value="0">-- Select Acreage Rate --</option>
              {props.rates
                .filter((r) => r.type === "Acreage")
                .map((r) => (
                  <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                    {r.description}
                  </option>
                ))}
            </Input>
            <InputErrorMessage name="acreageRateId" />
          </Col>
        </Row>
        <br />
        {(props.quote.acreageRateId || 0) > 0 && (
          <Row className="align-items-baseline">
            <Col md="3">
              <Label for="acres">Number of Acres</Label>
              <Input
                className={getClassName("acres")}
                type="number"
                id="acres"
                value={props.quote.acres}
                onChange={onChange("acres", (e) =>
                  props.updateQuote({
                    ...props.quote,
                    acres: parseFloat(e.target.value),
                  })
                )}
                onBlur={onBlur("acres")}
              />
              <InputErrorMessage name="acres" />
            </Col>
            <Col md="2">
              <Label>Rate</Label> <br />$
              {formatCurrency(props.quote.acreageRate)}
            </Col>
            <Col md="2">
              <Label>Years</Label>
              <br />
              {props.quote.years}
            </Col>
            <Col md="4">
              <Label>Total Acreage Fee</Label>
              <br />$
              {formatCurrency(
                props.quote.acres * props.quote.acreageRate * props.quote.years
              )}
            </Col>
          </Row>
        )}
      </Col>

      {/* Right Details */}
      <Col md="4" lg="6">
        <Row>
          <Col>
            <Label for="projectLocation">Project Location</Label>
          </Col>
          <Col className="text-right">
            <button
              className="btn btn-link"
              onClick={(_) => props.setEditFields(true)}
            >
              Edit Fields
            </button>
          </Col>
        </Row>
        <Location fields={props.quote.fields}></Location>
        <br />
        <div id="map" />
      </Col>
    </Row>
  );
};
