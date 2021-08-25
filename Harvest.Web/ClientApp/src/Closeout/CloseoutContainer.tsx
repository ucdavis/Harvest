import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Label, Input, Button, UncontrolledTooltip } from "reactstrap";
import * as yup from "yup";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faCheck } from "@fortawesome/free-solid-svg-icons";

import { Project, Expense, User } from "../types";
import { UnbilledExpensesContainer } from "../Expenses/UnbilledExpensesContainer";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { usePromiseNotification } from "../Util/Notifications";
import { ProjectProgress } from "../Projects/ProjectProgress";
import { useInputValidator } from "../FormValidation";
import { roundToTwo } from "../Util/Calculations"


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
  const history = useHistory();

  const [notification, setNotification] = usePromiseNotification();
  const [finalAcreageExpense, setFinalAcreageExpense] = useState<FinalAcreageExpense>({} as FinalAcreageExpense);

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
      }
    };

    cb();
  }, [projectId]);

  useEffect(() => {
    if (!project) {
      return;
    }

    if (finalAcreageExpense.amount === undefined) {
      setFinalAcreageExpense({ amount: roundToTwo(project.acres * (project.acreageRate.price / 12)) });
    }
  }, [project, finalAcreageExpense, setFinalAcreageExpense]);

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
    const request = fetch(`/Project/DoCloseout/${projectId}`, {
      method: "POST"
    });
    let successMessage = "Closed Out Project";

    setNotification(request, "Closing Out Project", getSuccessMessage);

    // Hack to be able to pass a synchronous function that returns the message, since there doesn't
    // appear to be a way to have the notification's toast.promise "success" argument take a promise.
    const response = await request;
    if (response.ok) {
      successMessage = await response.json() as string;
    }
    function getSuccessMessage() {
      return successMessage;
    }
  }

  if (project === undefined) {
    return <div>Loading ...</div>;
  }

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
            <Button id="CloseoutButton" color="primary" disabled={notification.pending} onClick={closeoutProject}>
              Closeout Project <FontAwesomeIcon icon={faCheck} />
            </Button>
            <UncontrolledTooltip target="CloseoutButton" >
              Generates a final invoice if there are any unbilled expenses. Sets project status to either
              Completed or CloseoutPending based on whether there are any pending invoices.
            </UncontrolledTooltip>

          </div>
        </div>
      </div>
    </div>
  );
};
