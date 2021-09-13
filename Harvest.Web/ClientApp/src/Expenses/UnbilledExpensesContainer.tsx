import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Expense, ExpenseQueryParams } from "../types";
import { ExpenseTable } from "./ExpenseTable";
import { ShowFor } from "../Shared/ShowFor";
import { formatCurrency } from "../Util/NumberFormatting";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface RouteParams {
  projectId?: string;
}

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
}

export const UnbilledExpensesContainer = (props: Props) => {
  const { projectId } = useParams<RouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [total, setTotal] = useState(0);
  const [notification, setNotification] = usePromiseNotification();
  const [confirmRemoveExpense] = useConfirmationDialog({
    title: "Remove Expense",
    message: "Are you sure you want to remove this unbilled expense?",
  });

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get unbilled expenses for the project
    if (projectId === undefined) return;

    const cb = async () => {
      const response = await fetch(`/Expense/GetUnbilled/${projectId}`);

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        if (getIsMounted()) {
          setExpenses(expenses);
          setTotal(expenses.reduce((acc, cur) => acc + cur.total, 0));
        }
      }
    };

    cb();
  }, [projectId, props.newExpenseCount, getIsMounted]);

  const deleteExpense = async (expense: Expense) => {
    if (!(await confirmRemoveExpense())) {
      return;
    }

    const request = fetch(`/Expense/Delete?expenseId=${expense.id}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
    });
    setNotification(request, "Removing Expense", () => {
      if (getIsMounted()) {
        let expensesCopy = [...expenses];
        const index = expensesCopy.findIndex(
          (element) => element.id === expense.id
        );
        expensesCopy.splice(index, 1);

        setExpenses(expensesCopy);
        setTotal(expensesCopy.reduce((acc, cur) => acc + cur.total, 0));
      }
      return "Expense Removed";
    });
  };

  if (expenses.length === 0) {
    return <h3>No un-billed expenses found</h3>;
  }

  return (
    <div>
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h2>
            Un-billed Expenses <small>(${formatCurrency(total)} total)</small>
          </h2>
        </div>
        <div className="col text-right">
          <ShowFor roles={["FieldManager", "Supervisor", "Worker"]}>
            <Link
              to={`/expense/entry/${projectId}?${ExpenseQueryParams.ReturnOnSubmit}=true`}
              className="btn btn btn-primary "
            >
              Enter New <FontAwesomeIcon icon={faPlus} />
            </Link>
          </ShowFor>
        </div>
      </div>

      <ExpenseTable
        expenses={expenses}
        deleteExpense={deleteExpense}
        canDeleteExpense={!notification.pending}
      ></ExpenseTable>
    </div>
  );
};
