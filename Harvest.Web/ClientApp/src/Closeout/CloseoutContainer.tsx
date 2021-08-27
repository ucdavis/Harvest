import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Label, Input, Button, UncontrolledTooltip } from "reactstrap";
import * as yup from "yup";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faCheck } from "@fortawesome/free-solid-svg-icons";

import { Project, Result } from "../types";
import { UnbilledExpensesContainer } from "../Expenses/UnbilledExpensesContainer";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { usePromiseNotification } from "../Util/Notifications";
import { ProjectProgress } from "../Projects/ProjectProgress";
import { useInputValidator } from "../FormValidation";
import { roundToTwo } from "../Util/Calculations"
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { millisecondsPerDay } from "../Util/Calculations";
import { ShowFor } from "../Shared/ShowFor";


interface RouteParams {
  projectId?: string;
}

interface FinalAcreageExpense {
  amount: number;
}

const finalAcreageExpenseSchema = yup.object().shape({
  amount: yup.number().required().default(0)
    .typeError("Acreage Expense must be a number")
    .positive("Acreage Expense must be positive")
});

export const CloseoutContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project | undefined>();
  const [newExpenseCount, setNewExpenseCount] = useState(0);
  const [notification, setNotification] = usePromiseNotification();
  const [finalAcreageExpense, setFinalAcreageExpense] = useState<FinalAcreageExpense>({ amount: 0 } as FinalAcreageExpense);
  const { getConfirmation } = useConfirmationDialog();
  const history = useHistory();
  const [beyondCloseoutDisplayDate, setBeyondCloseoutDisplayDate] = useState(false);
  const [closeoutRequested, setCloseoutRequested] = useState(false);

  const {
    onChange,
    InputErrorMessage,
    getClassName,
    onBlur,
    formErrorCount,
  } = useInputValidator<FinalAcreageExpense>(finalAcreageExpenseSchema);

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);
      if (response.ok) {
        const proj: Project = await response.json();
        setProject(proj);
        setBeyondCloseoutDisplayDate(new Date().getTime() - new Date(proj.end).getTime() >= (7 * millisecondsPerDay));
      }
    };

    cb();
  }, [projectId]);

  useEffect(() => {
    if (!project) {
      return;
    }

    if (finalAcreageExpense.amount === 0) {
      setFinalAcreageExpense({ amount: roundToTwo(project.acres * (project.acreageRate.price / 12)) });
    }
  }, [project, finalAcreageExpense, setFinalAcreageExpense]);

  useEffect(() => {
    if (closeoutRequested) {
      history.push(`/project/details/${projectId}`);
    }
  }, [closeoutRequested, history, projectId]);

  const addAcreageExpense = async () => {
    const request = fetch(`/Expense/CreateAcreage/${projectId}?amount=${finalAcreageExpense.amount}`, {
      method: "POST"
    });
    setNotification(request, "Adding Acreage Expense", "Acreage Expense Added");
    const response = await request;
    if (response.ok) {
      setNewExpenseCount(newExpenseCount + 1);
    }
  };

  const closeoutProject = async () => {
    if (!await getConfirmation(
      "Closeout Project",
      <ul>
        <li>Generates a final invoice if there are any unbilled expenses</li>
        <li>Sets project status to either Completed or CloseoutPending based on whether there are any pending invoices</li>
      </ul>)) {
      return;
    }

    const request = fetch(`/Invoice/DoCloseout/${projectId}`, {
      method: "POST"
    });

    setNotification(request, "Closing Out Project", async (response: Response) => {
      const result = await response.json() as Result<number>;
      setCloseoutRequested(true);
      return result.message;
    });
  }

  if (project === undefined) {
    return <div>Loading ...</div>;
  }

  const canCloseout = project.status === "AwaitingCloseout" ||
    (project.status === "Active" && beyondCloseoutDisplayDate);

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
              <h2>Prepare final invoice for project closeout</h2>
              <div className="row">
                <div className="col-md-8">
                  <Label for="amount">Final Acreage Expense (defaults to monthly)</Label>
                  <Input
                    className={getClassName("amount")}
                    type="number"
                    id="amount"
                    value={finalAcreageExpense.amount}
                    onChange={onChange("amount", (e) => setFinalAcreageExpense({ amount: parseFloat(e.target.value) }))}
                    onBlur={onBlur("amount")}
                  />
                  <InputErrorMessage name="amount" />
                </div>
                <div className="col-md-4">
                  <Button color="primary" disabled={notification.pending || formErrorCount > 0} onClick={addAcreageExpense}>
                    Add Acreage Expense <FontAwesomeIcon icon={faPlus} />
                  </Button>
                </div>

              </div>
            </div>
            <div className="col-md-6">
              <ProjectProgress project={project} />
            </div>
          </div>
        </div>
      </div>
      <div className="card-content">
        <div className="row">
          <div className="col-md-12">
            <UnbilledExpensesContainer newExpenseCount={newExpenseCount} />
          </div>
        </div>
      </div>
      <div className="card-content card-green-bg">
        <div className="row">
          <div className="col text-right">
            <Button id="CloseoutButton" color="primary" disabled={!canCloseout || notification.pending} onClick={closeoutProject}>
              Closeout Project <FontAwesomeIcon icon={faCheck} />
            </Button>
            <ShowFor condition={!canCloseout}>
              {/* Not sure if there's a better way to handle indicators as to why something is disabled */}
              <UncontrolledTooltip target="CloseoutButton" >
                Project is not awaiting closeout, and it doesn't have an end date that falls within the last seven days or later.
              </UncontrolledTooltip>
            </ShowFor>

          </div>
        </div>
      </div>
    </div>
  );
};
