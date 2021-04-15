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

import { Rate, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  category: string;
  rates: Rate[];
  workItems: WorkItem[];
  updateWorkItems: (workItem: WorkItem) => void;
  addNewWorkItem: (category: string) => void;
  deleteWorkItem: (workItem: WorkItem) => void;
}

export const WorkItemsForm = (props: Props) => {
  const rateItemChanged = (
    e: React.ChangeEvent<HTMLInputElement>,
    workItem: WorkItem
  ) => {
    const rateId = parseInt(e.target.value);
    const rate = props.rates.find((r) => r.id === rateId);

    // rate can be undefinied if they select the default option
    if (rate !== undefined) {
      // new rate selected, update the work item with defaults
      props.updateWorkItems({ ...workItem, rateId, rate: rate.price });
    }
  };

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
                <Input
                  type="select"
                  name="select"
                  defaultValue={workItem.rateId}
                  onChange={(e) => rateItemChanged(e, workItem)}
                >
                  <option value="0">-- Select {props.category} --</option>
                  {props.rates.map((r) => (
                    <option key={`rate-${r.type}-${r.id}`} value={r.id}>
                      {r.description}
                    </option>
                  ))}
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
                value={workItem.quantity}
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
                value={workItem.rate}
                onChange={(e) =>
                  props.updateWorkItems({
                    ...workItem,
                    rate: parseFloat(e.target.value ?? 0),
                  })
                }
              />
            </InputGroup>
          </Col>

          <Col xs="2">${formatCurrency(workItem.rate * workItem.quantity)}</Col>

          <Col xs="2">
            <Button
              color="danger"
              onClick={() => props.deleteWorkItem(workItem)}
            >
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
