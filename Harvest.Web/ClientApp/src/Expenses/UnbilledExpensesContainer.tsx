import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Expense } from "../types";
import { ExpenseTable } from "./ExpenseTable";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface RouteParams {
  projectId?: string;
}

export const UnbilledExpensesContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);

  useEffect(() => {
    // get unbilled expenses for the project
    if (projectId === undefined) return;

    const cb = async () => {
      const response = await fetch(`/Expense/GetUnbilled/${projectId}`);

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        setExpenses(expenses);
      }
    };

    cb();
  }, [projectId]);

  if (expenses.length === 0) {
    return <h3>No un-billed expenses found</h3>;
  }

  return (
    <div>
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>Un-billed Expenses</h1>
        </div>
        <div className="col text-right">
          <Link
            to={`/expense/entry/${projectId}`}
            className="btn btn btn-link "
          >
            Enter New <FontAwesomeIcon icon={faPlus} />
          </Link>
        </div>
      </div>

      <ExpenseTable expenses={expenses}></ExpenseTable>
    </div>
  );
};
