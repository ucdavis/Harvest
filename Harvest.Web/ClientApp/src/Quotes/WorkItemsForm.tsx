import React from "react";
import {
  Button,
  Col,
  FormGroup,
  Input,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Row,
} from "reactstrap";

import { WorkItem } from "../types";

interface Props {
  category: string;
  workItems: WorkItem[];
  updateWorkItems: (workItem: WorkItem) => void;
  addNewWorkItem: (category: string) => void;
}

export const WorkItemsForm = (props: Props) => {
  return (
    <div>
      <Row>
        <Col xs="4">
          <h6>{props.category}</h6>
        </Col>
        <Col xs="2">
          <h6>Time</h6>
        </Col>
        <Col xs="2">
          <h6>Rate</h6>
        </Col>
        <Col xs="2">
          <h6>Total</h6>
        </Col>
      </Row>
      {props.workItems.map((workItem) => (
        <Row key={`workItem-${workItem.id}-activity-${workItem.activityId}`}>
          <Col xs="4">
            <FormGroup>
              {props.category === "other" ? (
                <Input />
              ) : (
                <Input type="select" name="select">
                  <option>Hello</option>
                  <option>Bye</option>
                </Input>
              )}
            </FormGroup>
          </Col>

          <Col xs="2">
            <InputGroup>
              <InputGroupAddon addonType="prepend">
                <InputGroupText>hr</InputGroupText>
              </InputGroupAddon>
              <Input
                type="number"
                id="units"
                onChange={(e) =>
                  props.updateWorkItems({
                    ...workItem,
                    quantity: parseFloat(e.target.value ?? 0),
                  })
                }
              />
            </InputGroup>
          </Col>

          <Col xs="2">
            <InputGroup>
              <InputGroupAddon addonType="prepend">
                <InputGroupText>$</InputGroupText>
              </InputGroupAddon>
              <Input
                type="number"
                id="rate"
                onChange={(e) =>
                  props.updateWorkItems({
                    ...workItem,
                    rate: parseFloat(e.target.value ?? 0),
                  })
                }
              />
            </InputGroup>
          </Col>

          <Col xs="2">${workItem.rate * workItem.quantity}</Col>

          <Col xs="2">
            <Button color="danger" onClick={() => {}}>
              Delete
            </Button>
          </Col>
        </Row>
      ))}
      <Button color="link" onClick={() => props.addNewWorkItem(props.category)}>
        Add {props.category}
      </Button>
    </div>
  );
};
