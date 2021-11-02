import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Button, UncontrolledTooltip } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck } from "@fortawesome/free-solid-svg-icons";

import { Project, Result } from "../types";
import { UnbilledExpensesContainer } from "../Expenses/UnbilledExpensesContainer";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { usePromiseNotification } from "../Util/Notifications";
import { roundToTwo } from "../Util/Calculations";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { ShowFor } from "../Shared/ShowFor";
import { useIsMounted } from "../Shared/UseIsMounted";

interface RouteParams {
  projectId?: string;
}

interface FinalAcreageExpense {
  amount: number;
}

export const CloseoutContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project | undefined>();
  const [notification, setNotification] = usePromiseNotification();
  const [finalAcreageExpense, setFinalAcreageExpense] =
    useState<FinalAcreageExpense>({ amount: 0 } as FinalAcreageExpense);
  const history = useHistory();
  const [closeoutRequested, setCloseoutRequested] = useState(false);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/api/Project/Get/${projectId}`);
      if (response.ok) {
        const proj: Project = await response.json();
        getIsMounted() && setProject(proj);
      }
    };

    cb();
  }, [projectId, getIsMounted]);

  useEffect(() => {
    if (!project) {
      return;
    }

    if (
      finalAcreageExpense.amount === 0 &&
      project.acreageRate &&
      project.acres
    ) {
      setFinalAcreageExpense({
        amount: roundToTwo(project.acres * (project.acreageRate.price / 12)),
      });
    }
  }, [project, finalAcreageExpense, setFinalAcreageExpense]);

  useEffect(() => {
    if (closeoutRequested) {
      history.push(`/project/details/${projectId}`);
    }
  }, [closeoutRequested, history, projectId]);

  const [getConfirmation] = useConfirmationDialog({
    title: "Initiate Closeout",
    message: (
      <p>
        Upon PI approval, closeout will result in...
        <ul>
          <li>Generating a final invoice if there are any unbilled expenses</li>
          <li>
            Setting project status to either Completed or CloseoutPending based
            on whether there are any pending invoices
          </li>
        </ul>
      </p>
    ),
  });

  const initiateCloseout = async () => {
    const [confirmed] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    const request = fetch(`/api/Invoice/InitiateCloseout/${projectId}`, {
      method: "POST",
    });

    setNotification(
      request,
      "Initiating Project Closeout",
      async (response: Response) => {
        const result = (await response.json()) as Result<number>;
        getIsMounted() && setCloseoutRequested(true);
        return result.message;
      }
    );
  };

  if (project === undefined) {
    return <div>Loading ...</div>;
  }

  const canCloseout =
    project.status === "AwaitingCloseout" || project.status === "Active";

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-content">
        <div className="row">
          <div className="col-md-12">
            <UnbilledExpensesContainer hideProjectHeader={true} />
            <br />
            <Button
              id="CloseoutButton"
              color="primary"
              disabled={!canCloseout || notification.pending}
              onClick={initiateCloseout}
            >
              Initiate Closeout <FontAwesomeIcon icon={faCheck} />
            </Button>
            <ShowFor condition={!canCloseout}>
              {/* Not sure if there's a better way to handle indicators as to why something is disabled */}
              <UncontrolledTooltip target="CloseoutButton">
                Closeout can only be initiated when project is active or
                awaiting closeout.
              </UncontrolledTooltip>
            </ShowFor>
          </div>
        </div>
      </div>
      <div className="card-content">
        <div className="row">
          <div className="col text-right"></div>
        </div>
      </div>
    </div>
  );
};
