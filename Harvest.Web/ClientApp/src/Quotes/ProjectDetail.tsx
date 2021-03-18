import React from "react";

import { QuoteContent, WorkItemImpl } from "../types";

import {
  Button,
  Col,
  Container,
  Input,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Label,
  Row,
} from "reactstrap";

interface Props {
  quote: QuoteContent;
  updateQuote: React.Dispatch<React.SetStateAction<QuoteContent>>;
}

export const ProjectDetail = (props: Props) => {
  // TODO: should we do the work here or pass up to parent?
  const addActivity = () => {
    const newActivityId = Math.max(...props.quote.activities.map((a) => a.id), 0) + 1;
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
    <Container>
      <Row>
        {/* Left Details */}
        <Col xs="6">
          <Label for="projectName">
            <h6>Project Name</h6>
          </Label>
          <Input type="text" id="projectName" />
          <br />
          <Row>
            <Col xs="4">
              <Label for="acres">
                <h6>Number of Acres</h6>
              </Label>
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
            <Col xs="4">
              <Label for="rate">
                <h6>Rate</h6>
              </Label>
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
            <Col xs="4">
              <Label>
                <h6>Total Acreage Fee</h6>
              </Label>
              <InputGroup>
                <InputGroupAddon addonType="prepend">
                  <InputGroupText>$</InputGroupText>
                </InputGroupAddon>
                <Input
                  type="number"
                  id="rate"
                  readOnly
                  value={props.quote.acres * props.quote.acreageRate}
                />
              </InputGroup>
            </Col>
          </Row>
          <br />
          <Button color="success" size="lg" onClick={addActivity}>
            Add Activity
          </Button>
        </Col>

        {/* Right Details */}
        <Col xs="6">
          <Label for="projectLocation">Project Location</Label>
          <Input type="text" id="projectLocation" />
          <br />
          <div id="map" />
        </Col>
      </Row>
    </Container>
  );
};
