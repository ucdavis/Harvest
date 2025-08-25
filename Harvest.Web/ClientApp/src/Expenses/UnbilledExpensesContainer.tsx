import React, { Dispatch, SetStateAction, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import {
  CommonRouteParams,
  Expense,
  ExpenseQueryParams,
  Project,
} from "../types";
import { ExpenseTable } from "./ExpenseTable";
import { ShowFor } from "../Shared/ShowFor";
import { formatCurrency } from "../Util/NumberFormatting";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";
import { ProjectHeader } from "../Shared/ProjectHeader";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
  hideProjectHeader?: boolean;
  disableEdits?: boolean;
  setTotalUnbilled?: Dispatch<SetStateAction<number | undefined>>;
}

export const UnbilledExpensesContainer = (props: Props) => {
  const { projectId, team, shareId } = useParams<CommonRouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [project, setProject] = useState<Project>();
  const [total, setTotal] = useState(0);
  const [notification, setNotification] = usePromiseNotification();
  const [confirmRemoveExpense] = useConfirmationDialog({
    title: "Remove Expense",
    message: "Are you sure you want to remove this unbilled expense?",
  });
  const { newExpenseCount, setTotalUnbilled } = props;

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get unbilled expenses for the project
    if (projectId === undefined) return;
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Expense/GetUnbilled/${projectId}/${shareId}`
      );

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        if (getIsMounted()) {
          setExpenses(expenses);
          const total = expenses.reduce((acc, cur) => acc + cur.total, 0);
          setTotal(total);
          setTotalUnbilled && setTotalUnbilled(total);
        }
      }
    };

    cb();
  }, [
    projectId,
    newExpenseCount,
    getIsMounted,
    setTotalUnbilled,
    team,
    shareId,
  ]);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/Get/${projectId}/${shareId}`
      );

      if (response.ok) {
        const project = (await response.json()) as Project;
        getIsMounted() && setProject(project);
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted, team, shareId]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

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
        const total = expensesCopy.reduce((acc, cur) => acc + cur.total, 0);

        setTotal(total);
        setTotalUnbilled && setTotalUnbilled(total);
      }
      return "Expense Removed";
    });
  };

  return (
    <div className={!props.hideProjectHeader ? "card-wrapper" : ""}>
      {!props.hideProjectHeader && (
        <ProjectHeader
          project={project}
          title={"Field Request #" + (project?.id || "")}
          hideBack={false}
        />
      )}
      <div className="card-content">
        <div className="row justify-content-between mb-3">
          <div className="col">
            {expenses.length ? (
              <>
                <h3>
                  Unbilled Expenses
                  <small> (${formatCurrency(total)} total)</small>
                  {total - project.quoteTotal > 0 && (
                    <p style={{ color: "red" }}>
                      <strong> Warning!</strong> Expenses exceed amount
                      remaining by ${formatCurrency(total - project.quoteTotal)}
                    </p>
                  )}
                </h3>
                <small>You may click on the expense row for more details</small>
              </>
            ) : (
              <h3>No Unbilled Expenses</h3>
            )}
          </div>
          <div className="col text-right">
            <ShowFor
              roles={["FieldManager", "Supervisor", "Worker"]}
              condition={project.status !== "PendingCloseoutApproval"}
            >
              <Link
                to={`/${team}/expense/entry/${projectId}?${ExpenseQueryParams.ReturnOnSubmit}=true`}
                className="btn btn btn-primary "
              >
                Enter New <FontAwesomeIcon icon={faPlus} />
              </Link>
            </ShowFor>
          </div>
        </div>

        {expenses.length > 0 && (
          <ExpenseTable
            expenses={expenses}
            deleteExpense={deleteExpense}
            showActions={
              !notification.pending &&
              project?.status !== "PendingCloseoutApproval"
            }
            showProject={false}
          ></ExpenseTable>
        )}
      </div>
    </div>
  );
};
