import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { Project, ProjectAccount } from "../types";
import { AccountsInput } from "./AccountsInput";
import { ProjectHeader } from "./ProjectHeader";

interface RouteParams {
  projectId?: string;
}

export const AccountChangeContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project | undefined>();
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]);
  const [disabled, setDisabled] = useState<boolean>(true);

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        const proj: Project = await response.json();

        setProject(proj);
      }
    };

    cb();
  }, [projectId]);

  if (project === undefined) {
    return <div>Loading ...</div>;
  }

  const changeAccounts = async () => {
    // TODO: take accounts and validate
    // if accounts look good, post them to new .net endpoint in Api/RequestController.cs
    // that endpoint will look something like the accounts portion of ApproveAsync method
    // on success, redirect back to project detail page
    console.log(projectId, accounts);
    const response = await fetch(`/Request/ApproveChange/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ Accounts: accounts }),
    });

    // console.log(response)
    if (response.ok) {
      console.log("ok nice");
    }
  };

  // we have a project, now time to change account
  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <div className="row">
            <div className="col-md-6">
              <h2>Enter your new accounts</h2>
              <AccountsInput
                accounts={accounts}
                setAccounts={setAccounts}
                setDisabled={setDisabled}
              />
              <button
                className="btn btn-primary"
                onClick={() => changeAccounts()}
                disabled={disabled}
              >
                Change Accounts
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
