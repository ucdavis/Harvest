import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { CommonRouteParams, Expense, Project } from "../types";
import { ExpenseTable } from "./ExpenseTable";

import { authenticatedFetch } from "../Util/Api";

import { useIsMounted } from "../Shared/UseIsMounted";
import { ProjectHeader } from "../Shared/ProjectHeader"; //TODO add this once it is tested and running

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
}

export const BilledExpensesListContainer = (props: Props) => {
  const { projectId, team, shareId } = useParams<CommonRouteParams>();
  const [expenses, setExpenses] = useState<Expense[] | undefined>(undefined);
  const [project, setProject] = useState<Project>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const url = shareId
        ? `/api/${team}/Expense/GetAllBilled/${projectId}/${shareId}`
        : `/api/${team}/Expense/GetAllBilled/${projectId}`;

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

  useEffect(() => {
    // get project data for the ProjectHeader
    const cb = async () => {
      const url = shareId
        ? `/api/${team}/Project/Get/${projectId}/${shareId}`
        : `/api/${team}/Project/Get/${projectId}`;
      const response = await authenticatedFetch(url);

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

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project?.id || "")}
        hideBack={false}
      />
      <div className="card-content">
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
            showInvoice={true}
          ></ExpenseTable>
        )}
      </div>
    </div>
  );
};
