import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";

import { Project, ProjectAccount } from "../types";
import { AccountsInput } from "./AccountsInput";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { usePromiseNotification } from "../Util/Notifications";

interface RouteParams {
  projectId?: string;
}

export const AccountChangeContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project | undefined>();
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]);
  const [disabled, setDisabled] = useState<boolean>(true);
  const history = useHistory();

  const [notification, setNotification] = usePromiseNotification();

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

  //TODO: require PI or supervisor access after updating auth policies
  const changeAccounts = async () => {
    const request = fetch(`/Request/ChangeAccount/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ Accounts: accounts }),
    });

    setNotification(request, "Updating Accounts", "Accounts Updated");

    const response = await request;

    if (response.ok) {
      const data = await response.json();
      history.push(`/Project/Details/${data.id}`);
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
                disabled={disabled || notification.pending}
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
