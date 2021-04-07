import React, { useMemo, useState } from "react";
import { Card, CardBody, Col, FormGroup, Input, Row } from "reactstrap";
import { Expense, Rate } from "../types";

interface Props {
  expense: Expense;
  expenseTypes: string[];
  rates: Rate[];
  updateExpense: (expense: Expense) => void;
}

export const LineEntry = (props: Props) => {
  // update the matching valid rates whenever the expense type changes
  const validRates = useMemo(() => {
    return props.rates.filter((r) => r.type === props.expense.type);
  }, [props.expense.type, props.rates]);

  return (
    <Card>
      <CardBody>
        <Row>
          <Col xs="4">
            <h6>Category</h6>
          </Col>
          <Col xs="4">
            <h6>Expense</h6>
          </Col>
          <Col xs="2">
            <h6>{props.expense.rate.unit}</h6>
          </Col>
        </Row>
        <Row>
          <Col xs="4">
            <FormGroup>
              <Input
                type="select"
                name="expenseType"
                onChange={(e) =>
                  props.updateExpense({
                    ...props.expense,
                    type: e.target.value,
                    rate:
                      props.rates[
                        props.rates.findIndex((r) => r.type === e.target.value)
                      ],
                  })
                }
              >
                {props.expenseTypes.map((t) => (
                  <option selected={props.expense.type === t}>{t}</option>
                ))}
              </Input>
            </FormGroup>
          </Col>
          <Col xs="4">
            <FormGroup>
              <Input
                type="select"
                name="rateType"
                onChange={(e) =>
                  props.updateExpense({
                    ...props.expense,
                    rate:
                      props.rates[
                        props.rates.findIndex(
                          (r) => r.id === parseInt(e.target.value)
                        )
                      ],
                  })
                }
              >
                {validRates.map((r) => (
                  <option
                    value={r.id}
                    selected={props.expense.rate.id === r.id}
                  >
                    {r.description}
                  </option>
                ))}
              </Input>
            </FormGroup>
          </Col>
          <Col xs="4">
            <FormGroup>
              <Input
                type="number"
                name="quantity"
                value={props.expense.quantity}
                onChange={(e) =>
                  props.updateExpense({
                    ...props.expense,
                    quantity: parseInt(e.target.value),
                    total: props.expense.rate.price * parseFloat(e.target.value)
                  })
                }
              />
            </FormGroup>
          </Col>
        </Row>
      </CardBody>
    </Card>
  );
};
