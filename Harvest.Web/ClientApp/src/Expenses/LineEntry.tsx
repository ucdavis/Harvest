import React, { useMemo, useState } from "react";
import { Card, CardBody, Col, FormGroup, Input, Row } from "reactstrap";
import { Expense, Rate } from "../types";

interface Props {
  expense: Expense;
  expenses: Expense[];
  expenseTypes: string[];
  rates: Rate[];
  setDisabled: (value: boolean) => void;
  updateExpense: (expense: Expense) => void;
}

export const LineEntry = (props: Props) => {
  // update the matching valid rates whenever the expense type changes
  const [error, setError] = useState<string>("Hourly rate is empty or 0");
  const validRates = useMemo(() => {
    return props.rates.filter((r) => r.type === props.expense.type);
  }, [props.expense.type, props.rates]);

  const updateExpenseType = (type: string) => {
    const rate = props.rates[props.rates.findIndex((r) => r.type === type)];

    props.updateExpense({
      ...props.expense,
      type,
      rate,
      quantity: 0,
      total: 0,
    });
  };

  const updateHourlyRate = (value: string) => {
    const newQuantity = parseFloat(value);

    props.updateExpense({
      ...props.expense,
      quantity: parseFloat(value),
      total: props.expense.rate.price * parseFloat(value),
    });

    if (isNaN(newQuantity) || newQuantity <= 0) {
      setError("Hourly rate is empty or below 0");
    } else {
      setError("");
    }

    props.setDisabled(false);

    for (let i = 0; i < props.expenses.length; i++) {
      if (isNaN(props.expenses[i].quantity)  || props.expenses[i].quantity <= 0) {
        props.setDisabled(true);
        return;
      }
    }
  };

  const updateRateType = (rateId: number) => {
    // since we are just updating the rate type, if quantity is entered we'll use it and recalculate total
    const rate = props.rates[props.rates.findIndex((r) => r.id === rateId)];

    props.updateExpense({
      ...props.expense,
      rate,
      total: props.expense.quantity * rate.price,
    });
  };

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
                defaultValue={props.expense.type}
                onChange={(e) => updateExpenseType(e.target.value)}
              >
                {props.expenseTypes.map((t) => (
                  <option key={t}>{t}</option>
                ))}
              </Input>
            </FormGroup>
          </Col>
          <Col xs="4">
            <FormGroup>
              <Input
                type="select"
                name="rateType"
                defaultValue={props.expense.rate.id}
                onChange={(e) => updateRateType(parseInt(e.target.value))}
              >
                {validRates.map((r) => (
                  <option key={r.id} value={r.id}>
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
                step="any"
                name="quantity"
                defaultValue={props.expense.quantity || ""}
                placeholder={`${props.expense.rate.unit} total`}
                onChange={(e) => updateHourlyRate(e.target.value)}
              />
            </FormGroup>
          </Col>
        </Row>
        <div style={{ color: "red" }}>{error}</div>
      </CardBody>
    </Card>
  );
};
