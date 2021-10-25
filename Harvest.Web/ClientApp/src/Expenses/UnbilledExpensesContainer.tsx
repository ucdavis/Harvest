import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Expense, ExpenseQueryParams, Project } from "../types";
import { ExpenseTable } from "./ExpenseTable";
import { ShowFor } from "../Shared/ShowFor";
import { formatCurrency } from "../Util/NumberFormatting";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";
import { ProjectHeader } from "../Shared/ProjectHeader";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface RouteParams {
  projectId?: string;
}

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
  hideProjectHeader?: boolean;
}

export const UnbilledExpensesContainer = (props: Props) => {
  const { projectId } = useParams<RouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [project, setProject] = useState<Project>();
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
      const response = await fetch(`/api/Expense/GetUnbilled/${projectId}`);

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

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(`/api/Project/Get/${projectId}`);

      if (response.ok) {
        const project = (await response.json()) as Project;
        getIsMounted() && setProject(project);
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  const deleteExpense = async (expense: Expense) => {
    const [confirmed] = await confirmRemoveExpense();
    if (!confirmed) {
      return;
    }

    const request = fetch(`/api/Expense/Delete?expenseId=${expense.id}`, {
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
              <h3>
                Unbilled Expenses
                <small> (${formatCurrency(total)} total)</small>
              </h3>
            ) : (
              <h3>No Unbilled Expenses</h3>
            )}
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

        {expenses.length > 0 && (
          <ExpenseTable
            expenses={expenses}
            deleteExpense={deleteExpense}
            canDeleteExpense={!notification.pending}
          ></ExpenseTable>
        )}
      </div>
    </div>
  );
};
