import React from "react";

import { QuoteContent, Rate, WorkItemImpl } from "../types";

import { Button, Col, Input, Label, Row } from "reactstrap";

import { formatCurrency } from "../Util/NumberFormatting";
import { Location } from "../Fields/Location";
import { useInputValidator } from "../FormValidation";
import { quoteContentSchema } from "../schemas";

interface Props {
  rates: Rate[];
  quote: QuoteContent;
  updateQuote: React.Dispatch<React.SetStateAction<QuoteContent>>;
  setEditFields: React.Dispatch<React.SetStateAction<boolean>>;
}

export const ProjectDetail = (props: Props) => {
  const {
    onChange,
    InputErrorMessage,
    getClassName,
    onBlur,
  } = useInputValidator<QuoteContent>(quoteContentSchema);

  // TODO: should we do the work here or pass up to parent?
  const addActivity = () => {
    const newActivityId =
      Math.max(...props.quote.activities.map((a) => a.id), 0) + 1;
    props.updateQuote({
      ...props.quote,
      activities: [
        ...props.quote.activities,
        {
          id: newActivityId,
          name: "Activity",
          total: 0,
          workItems: [
            new WorkItemImpl(newActivityId, 1, "Labor"),
            new WorkItemImpl(newActivityId, 2, "Equipment"),
            new WorkItemImpl(newActivityId, 3, "Other"),
          ],
          year: 1, // default new activity to no adjustment
          adjustment: 0,
        },
      ],
    });
  };

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
        acreageRateId: 0,
        acreageRateDescription: "",
      });
    }
  };
  return (
    <Row className="align-items-baseline">
      {/* Left Details */}
      <Col md="6">
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
              value={props.quote.acreageRateId}
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
        {props.quote.acreageRateId > 0 && (
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

        <br />
        <Button
          className="mb-4"
          color="primary"
          size="lg"
          onClick={addActivity}
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
