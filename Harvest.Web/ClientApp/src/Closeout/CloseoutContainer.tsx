import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Button, UncontrolledTooltip } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck } from "@fortawesome/free-solid-svg-icons";

import { Project, Result } from "../types";
import { UnbilledExpensesContainer } from "../Expenses/UnbilledExpensesContainer";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { usePromiseNotification } from "../Util/Notifications";
import { ProjectProgress } from "../Projects/ProjectProgress";
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
    title: "Closeout Project",
    message: (
      <ul>
        <li>Generates a final invoice if there are any unbilled expenses</li>
        <li>
          Sets project status to either Completed or CloseoutPending based on
          whether there are any pending invoices
        </li>
      </ul>
    ),
  });

  const closeoutProject = async () => {
    const [confirmed] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    const request = fetch(`/api/Invoice/DoCloseout/${projectId}`, {
      method: "POST",
    });

    setNotification(
      request,
      "Closing Out Project",
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
              onClick={closeoutProject}
            >
              Closeout Project <FontAwesomeIcon icon={faCheck} />
            </Button>
            <ShowFor condition={!canCloseout}>
              {/* Not sure if there's a better way to handle indicators as to why something is disabled */}
              <UncontrolledTooltip target="CloseoutButton">
                Project is not awaiting closeout, and it doesn't have an end
                date that falls within the last seven days or later.
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
