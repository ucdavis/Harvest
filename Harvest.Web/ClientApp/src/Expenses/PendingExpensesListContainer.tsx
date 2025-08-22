import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { CommonRouteParams, Expense } from "../types";
import { ExpenseTable } from "./ExpenseTable";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck, faPlus } from "@fortawesome/free-solid-svg-icons";
import { Button } from "reactstrap";

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
  hideProjectHeader?: boolean;
  disableEdits?: boolean;
  showAll: boolean;
}

export const PendingExpensesListContainer = (props: Props) => {
  const { team } = useParams<CommonRouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [showAll] = useState(props.showAll);

  const [notification, setNotification] = usePromiseNotification();
  const [confirmRemoveExpense] = useConfirmationDialog({
    title: "Remove Expense",
    message: "Are you sure you want to remove this unbilled expense?",
  });

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      let url = `/api/${team}/Expense/GetPendingExpenses`;
      if (showAll) {
        url = `/api/${team}/Expense/GetAllPendingExpenses`;
      }

      const response = await authenticatedFetch(url);

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        if (getIsMounted()) {
          setExpenses(expenses);
        }
      }
    };

    cb();
  }, [getIsMounted, team, showAll]);

  const deleteExpense = async (expense: Expense) => {
    const [confirmed] = await confirmRemoveExpense();
    if (!confirmed) {
      return;
    }

    const request = authenticatedFetch(
      `/api/${team}/Expense/Delete?expenseId=${expense.id}`,
      {
        method: "POST",
      }
    );
    setNotification(request, "Removing Expense", () => {
      if (getIsMounted()) {
        let expensesCopy = [...expenses];
        const index = expensesCopy.findIndex(
          (element) => element.id === expense.id
        );
        expensesCopy.splice(index, 1);

        setExpenses(expensesCopy);
      }
      return "Expense Removed";
    });
  };

  return (
    <div className="projectlisttable-wrapper">
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>
            {showAll ? "All Pending Expenses" : "My Worker's Pending Expenses"}
          </h1>
        </div>
        {/* <div className="col text-right">
          <Link to={requestUrl} className="btn btn-sm btn-primary ">
            Create New <FontAwesomeIcon icon={faPlus} />
          </Link>
        </div> */}
        <div className="col text-right">
          <Button
            id="ApproveAllButton"
            color="primary"
            onClick={() => alert("TODO")}
          >
            Approve All <FontAwesomeIcon icon={faCheck} />
          </Button>
        </div>
      </div>

      <ExpenseTable
        expenses={expenses}
        deleteExpense={deleteExpense}
        canDeleteExpense={!notification.pending}
        showProject={true}
      ></ExpenseTable>
    </div>
  );
};
