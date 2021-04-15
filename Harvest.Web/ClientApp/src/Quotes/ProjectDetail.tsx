import React from "react";

import { QuoteContent, WorkItemImpl } from "../types";

import {
  Button,
  Col,
  Input,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Label,
  Row,
} from "reactstrap";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  quote: QuoteContent;
  updateQuote: React.Dispatch<React.SetStateAction<QuoteContent>>;
}

export const ProjectDetail = (props: Props) => {
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
          workItems: [
            new WorkItemImpl(newActivityId, 1, "labor"),
            new WorkItemImpl(newActivityId, 2, "equipment"),
            new WorkItemImpl(newActivityId, 3, "other"),
          ],
        },
      ],
    });
  };
  return (
    <Row className="align-items-baseline">
      {/* Left Details */}
      <Col md="6">
        <Label for="projectName">Project Name</Label>
        <Input
          type="text"
          id="projectName"
          value={props.quote.projectName}
          onChange={(e) =>
            props.updateQuote({ ...props.quote, projectName: e.target.value })
          }
        />
        <br />
        <Row className="align-items-baseline">
          <Col md="4">
            <Label for="acres">Number of Acres</Label>
            <Input
              type="number"
              id="acres"
              value={props.quote.acres}
              onChange={(e) =>
                props.updateQuote({
                  ...props.quote,
                  acres: parseInt(e.target.value ?? 0),
                })
              }
            />
          </Col>
          <Col md="4">
            <Label for="rate">Rate</Label>
            <InputGroup>
              <InputGroupAddon addonType="prepend">
                <InputGroupText>$</InputGroupText>
              </InputGroupAddon>
              <Input
                type="number"
                id="rate"
                value={props.quote.acreageRate}
                onChange={(e) =>
                  props.updateQuote({
                    ...props.quote,
                    acreageRate: parseInt(e.target.value ?? 0),
                  })
                }
              />
            </InputGroup>
          </Col>
          <Col md="4">
            <Label>Total Acreage Fee</Label>
            <InputGroup>
              <InputGroupAddon addonType="prepend">
                <InputGroupText>$</InputGroupText>
              </InputGroupAddon>
              <Input
                type="text"
                id="rate"
                readOnly
                value={formatCurrency(props.quote.acres * props.quote.acreageRate)}
              />
            </InputGroup>
          </Col>
        </Row>
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
        <Label for="projectLocation">Project Location</Label>
        <Input type="text" id="projectLocation" />
        <br />
        <div id="map" />
      </Col>
    </Row>
  );
};
