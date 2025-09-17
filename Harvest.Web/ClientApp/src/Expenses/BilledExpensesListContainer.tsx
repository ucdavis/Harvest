import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { CommonRouteParams, Expense } from "../types";
import { ExpenseTable } from "./ExpenseTable";

import { authenticatedFetch } from "../Util/Api";

import { useIsMounted } from "../Shared/UseIsMounted";

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
}

export const BilledExpensesListContainer = (props: Props) => {
  const { projectId, team, shareId } = useParams<CommonRouteParams>();
  const [expenses, setExpenses] = useState<Expense[] | undefined>(undefined);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      let url = `/api/${team}/Expense/GetAllBilled/${projectId}/${shareId}`;

      const response = await authenticatedFetch(url);

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        if (getIsMounted()) {
          setExpenses(expenses);
        }
      } else {
        if (getIsMounted()) {
          setExpenses([]);
        }
      }
    };

    cb();
  }, [getIsMounted, projectId, shareId, team]);

  return (
    <div className="projectlisttable-wrapper">
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>All Billed Expenses</h1>
        </div>
      </div>

      {expenses === undefined ? (
        <div className="text-center">
          <p>Loading expenses...</p>
        </div>
      ) : (
        <ExpenseTable
          expenses={expenses}
          deleteExpense={() => alert("Not supported")}
          showActions={false}
          showProject={false}
          showApprove={false}
          showExport={true}
          showAll={false}
        ></ExpenseTable>
      )}
    </div>
  );
};
