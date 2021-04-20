import React, { useCallback, useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Expense, Rate } from "../types";
import { ProjectSelection } from "./ProjectSelection";
import { LineEntry } from "./LineEntry";

interface RouteParams {
  projectId?: string;
}

// TODO: is it beter to read from rates state or just hard code because of the efficiency?
const expenseTypes = ["Labor", "Equipment", "Other"];

export const ExpenseEntryContainer = () => {
  const history = useHistory();

  const { projectId } = useParams<RouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [rates, setRates] = useState<Rate[]>([]);

  const getDefaultExpense = useCallback(
    (currentRates: Rate[], currentExpenses: Expense[]) => {
      const newId = Math.max(...currentExpenses.map((e) => e.id), 0) + 1;
      const defaultExpense: Expense = {
        id: newId,
        type: expenseTypes[0],
        rate:
          currentRates.find((r) => r.type === expenseTypes[0]) ||
          currentRates[0], // For now we just default to the first choice
        description: "",
        quantity: 0,
        total: 0,
      };

      return defaultExpense;
    },
    []
  );

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch("/Rate/Active");

      if (response.ok) {
        const rates: Rate[] = await response.json();

        setRates(rates);
        const firstExpense = getDefaultExpense(rates, []);
        setExpenses([firstExpense]);
      }
    };

    cb();
  }, [getDefaultExpense]);

  const changeProject = (projectId: number) => {
    // want to go to /expense/entry/[projectId]
    history.push(`/expense/entry/${projectId}`);
  };

  const updateExpense = (expense: Expense) => {
    const allExpenses = [...expenses];
    const idx = expenses.findIndex((a) => a.id === expense.id);
    allExpenses[idx] = { ...expense };

    setExpenses(allExpenses);
  };

  const submitExpenses = async () => {
    // TODO: disable the submit button and maybe just some sort of full screen processing UI

    // transform since we don't need to send along the whole rate description every time and we shouldn't pass along our internal ids
    const expensesBody = expenses.map((exp) => ({
      ...exp,
      id: 0,
      description: exp.rate.description,
      price: exp.rate.price,
      rateId: exp.rate.id,
      rate: null,
    }));

    const response = await fetch(`/Expense/Create/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(expensesBody),
    });

    if (response.ok) {
      window.location.pathname = "/project";
    } else {
      alert("Something went wrong, please try again");
    }
  };

  if (projectId === undefined) {
    // need to pick the project we want to use
    return (
      <ProjectSelection selectedProject={changeProject}></ProjectSelection>
    );
  }

  return (
    <div className="card-wrapper">
      <div className="card-content">
        <h3>Add Expenses for Project #{projectId}</h3>
        {expenses.map((expense) => (
          <LineEntry
            key={`expense-line-${expense.id}`}
            expense={expense}
            updateExpense={updateExpense}
            expenseTypes={expenseTypes}
            rates={rates}
          ></LineEntry>
        ))}
      </div>
      <button
        onClick={() =>
          setExpenses([...expenses, getDefaultExpense(rates, expenses)])
        }
      >
        Add Expense +
      </button>

      <hr />
      <button onClick={submitExpenses}>Submit!</button>
      <div>DEBUG: {JSON.stringify(expenses)}</div>
    </div>
  );
};
