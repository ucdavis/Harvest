import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { CommonRouteParams, Expense } from "../types";
import { ExpenseTable } from "./ExpenseTable";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck } from "@fortawesome/free-solid-svg-icons";
import { Button } from "reactstrap";
import LoadingHarvest from "../Shared/loadingHarvest";

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
  hideProjectHeader?: boolean;
  disableEdits?: boolean;
  showAll: boolean;
}

export const PendingExpensesListContainer = (props: Props) => {
  const { team } = useParams<CommonRouteParams>();
  const [expenses, setExpenses] = useState<Expense[] | undefined>(undefined);
  const [showAll] = useState(props.showAll);

  const [notification, setNotification] = usePromiseNotification();
  const [confirmRemoveExpense] = useConfirmationDialog({
    title: "Remove Expense",
    message: "Are you sure you want to remove this un-billed expense?",
  });

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      let url = `/api/${team}/Expense/GetMyPendingExpenses`;
      if (showAll) {
        url = `/api/${team}/Expense/GetAllPendingExpenses`;
      }

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
      if (getIsMounted() && expenses) {
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

  const approveExpense = async (expense: Expense) => {
    // Determine which API endpoint to use based on showAll
    const endpoint = showAll
      ? `/api/${team}/Expense/ApproveExpenses`
      : `/api/${team}/Expense/ApproveMyWorkerExpenses`;

    const request = authenticatedFetch(endpoint, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify([expense.id]),
    });

    setNotification(request, "Approving Expense", async (response) => {
      if (getIsMounted()) {
        if (response.ok) {
          // Remove the approved expense from the list
          if (expenses) {
            let expensesCopy = [...expenses];
            const index = expensesCopy.findIndex(
              (element) => element.id === expense.id
            );
            if (index !== -1) {
              expensesCopy.splice(index, 1);
              setExpenses(expensesCopy);
            }
          }

          return "Expense Approved";
        } else {
          // Handle error cases
          if (response.status === 404) {
            throw new Error("Expense not found");
          } else if (response.status === 400) {
            const errorText = await response.text();
            throw new Error(errorText || "Bad request");
          } else {
            throw new Error("Failed to approve expense");
          }
        }
      }
      return "Expense Approved";
    });
  };

  const approveAllExpenses = async () => {
    if (!expenses || expenses.length === 0) {
      return;
    }

    // Get all expense IDs
    const expenseIds = expenses.map((expense) => expense.id);

    //console.log(expenseIds);

    // Determine which API endpoint to use based on showAll
    const endpoint = showAll
      ? `/api/${team}/Expense/ApproveExpenses`
      : `/api/${team}/Expense/ApproveMyWorkerExpenses`;

    const request = authenticatedFetch(endpoint, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(expenseIds),
    });

    setNotification(
      request,
      "Approving All Expenses",
      async (response) => {
        if (getIsMounted()) {
          const result = (await response.json()) as Expense[];
          //Possibly we could show these and disable the approve buttons

          // Refresh the page data
          let url = `/api/${team}/Expense/GetMyPendingExpenses`;
          if (showAll) {
            url = `/api/${team}/Expense/GetAllPendingExpenses`;
          }

          const refreshResponse = await authenticatedFetch(url);
          if (refreshResponse.ok) {
            const refreshedExpenses: Expense[] = await refreshResponse.json();
            // Calculate how many were actually approved by comparing the counts
            const approvedCount = result.length;
            setExpenses(refreshedExpenses);
            return `${approvedCount} Expense${
              approvedCount === 1 ? "" : "s"
            } Approved`;
          }
        }
        return "Expenses processed";
      },
      async (error) => {
        // Handle error cases
        if (error.status === 404) {
          return "Expenses not found";
        } else if (error.status === 400) {
          const errorText = await error.text();
          return errorText || "Bad request";
        } else {
          return "Failed to approve expenses";
        }
      }
    );
  };

  if (expenses === undefined) {
    return (
      <>
        {" "}
        <div className="p-4 text-center">
          <LoadingHarvest size={64} />
          {/* default color #266041 */}
          <p>Loading Expenses...</p>
        </div>
      </>
    );
  }

  return (
    <div className="projectlisttable-wrapper">
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>
            {showAll ? "All Pending Expenses" : "My Worker's Pending Expenses"}
          </h1>
        </div>
        <div className="col text-right">
          <Button
            id="ApproveAllButton"
            color="primary"
            onClick={approveAllExpenses}
            disabled={
              notification.pending || !expenses || expenses.length === 0
            }
          >
            Approve All <FontAwesomeIcon icon={faCheck} />
          </Button>
        </div>
      </div>

      <ExpenseTable
        expenses={expenses}
        deleteExpense={deleteExpense}
        showActions={!notification.pending}
        showProject={true}
        showApprove={true}
        approveExpense={approveExpense}
        showExport={true}
        showAll={showAll}
      ></ExpenseTable>
    </div>
  );
};
