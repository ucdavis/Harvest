import React, { useEffect, useState } from "react";
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

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch("/Rate/Active");

      if (response.ok) {
        const rates: Rate[] = await response.json();

        setRates(rates);

        const defaultExpense: Expense = {
          id: 1,
          type: expenseTypes[0],
          rate: rates.find((r) => r.type === expenseTypes[0]) || rates[0], // For now we just default to the first choice
          description: "",
          quantity: 0,
          total: 0,
        };

        setExpenses([defaultExpense]);
      }
    };

    cb();
  }, []);

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

  if (projectId === undefined) {
    // need to pick the project we want to use
    return (
      <ProjectSelection selectedProject={changeProject}></ProjectSelection>
    );
  }

  return (
    <div className="card-wrapper">
      <div className="card-content">
        <h3>Add Expense for Project #{projectId}</h3>
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
      <div>DEBUG: {JSON.stringify(expenses)}</div>
    </div>
  );
};
