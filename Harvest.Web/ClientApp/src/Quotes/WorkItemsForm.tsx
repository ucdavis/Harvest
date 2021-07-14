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

import { Rate, RateType, WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt } from "@fortawesome/free-solid-svg-icons";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface Props {
  adjustment: number;
  category: RateType;
  rates: Rate[];
  workItems: WorkItem[];
  updateWorkItems: (workItem: WorkItem) => void;
  addNewWorkItem: (type: RateType) => void;
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
      props.updateWorkItems({
        ...workItem,
        description: requiresCustomDescription(rate.unit)
          ? ""
          : rate.description,
        rateId,
        rate: rate.price,
        unit: rate.unit,
        total: 0,
      });
    }
  };

  // TODO: Determine a better way of working out which other options need extra description text
  const requiresCustomDescription = (unit: string) => {
    return props.category === "Other" && unit === "Unit";
  };

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
      {props.workItems.map((workItem) => (
        <Row
          className="activity-line-item align-items-center"
          key={`workItem-${workItem.id}-activity-${workItem.activityId}`}
        >
          <Col xs="5">
            <FormGroup>
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
              {requiresCustomDescription(workItem.unit) && (
                <Input
                  type="text"
                  name="OtherDescription"
                  value={workItem.description}
                  placeholder="Description"
                  onChange={(e) =>
                    props.updateWorkItems({
                      ...workItem,
                      description: e.target.value,
                    })
                  }
                ></Input>
              )}
            </FormGroup>
          </Col>

          <Col className="col-sm-2">
            <InputGroup>
              <InputGroupAddon addonType="prepend">
                <InputGroupText>{workItem.unit || ""}</InputGroupText>
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

          <Col className="col-sm-2 offset-sm-1">
            ${formatCurrency(workItem.rate || 0)}
            {props.adjustment > 0 && (
              <span className="primary-color">
                {" "}
                + ${formatCurrency(workItem.rate * (props.adjustment / 100))}
              </span>
            )}
          </Col>

          <Col xs="1">${formatCurrency(workItem.total)}</Col>

          <Col xs="1">
            <button
              className="btn btn-link mt-0"
              onClick={() => props.deleteWorkItem(workItem)}
            >
              <FontAwesomeIcon icon={faTrashAlt} />
            </button>
          </Col>
        </Row>
      ))}
      <Button
        className="btn-sm"
        color="link"
        onClick={() => props.addNewWorkItem(props.category)}
      >
        <FontAwesomeIcon className="mr-2" icon={faPlus} />
        Add {props.category}
      </Button>
    </div>
  );
};
