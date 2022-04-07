import React, { useContext, useEffect, useState } from "react";
import { useHistory } from "react-router-dom";
import {
  Activity,
  Expense,
  AdhocProjectModel,
  Rate,
  WorkItemImpl,
  Project,
  CropType,
  ProjectAccount,
  QuoteContent,
  QuoteContentImpl,
} from "../types";
import { ActivityForm } from "../Quotes/ActivityForm";
import { Button, FormGroup, Input, Label } from "reactstrap";
import { Crops } from "../Requests/Crops";
import { SearchPerson } from "../Requests/SearchPerson";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";
import { usePromiseNotification } from "../Util/Notifications";
import AppContext from "../Shared/AppContext";

import { workItemSchema } from "../schemas";
import { checkValidity } from "../Util/ValidationHelpers";
import * as yup from "yup";
import { useIsMounted } from "../Shared/UseIsMounted";
import { validatorOptions } from "../constants";
import { authenticatedFetch } from "../Util/Api";
import { useInputValidator, ValidationProvider } from "use-input-validator";
import { adhocProjectSchema } from "../schemas";
import { AccountsInput } from "../Requests/AccountsInput";

const getDefaultActivity = (id: number) => ({
  id,
  name: "Generic Activity",
  total: 0,
  year: 0,
  adjustment: 0,
  workItems: [
    new WorkItemImpl(id, 1, "Labor"),
    new WorkItemImpl(id, 2, "Equipment"),
    new WorkItemImpl(id, 3, "Other"),
  ],
});

export const AdhocProject = () => {
  const history = useHistory();
  const { detail: userDetail } = useContext(AppContext).user; //Only FM can do this, so we don't care about roles like we would in the project request
  const [project, setProject] = useState<Project>({
    id: 0,
    cropType: "Row" as CropType,
    principalInvestigator: userDetail,
  } as Project);
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]);
  const [disabled, setDisabled] = useState<boolean>(true);

  const [rates, setRates] = useState<Rate[]>([]);
  const [inputErrors, setInputErrors] = useState<string[]>([]);
  const {
    context,
    onChange,
    onChangeValue,
    onBlur,
    onBlurValue,
    InputErrorMessage,
    propertyHasErrors,
  } = useInputValidator(adhocProjectSchema, project, validatorOptions);

  // activities are groups of expenses
  const [activities, setActivities] = useState<Activity[]>([
    getDefaultActivity(1),
  ]);

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await authenticatedFetch("/api/Rate/Active");

      if (response.ok) {
        const rates: Rate[] = await response.json();

        getIsMounted() && setRates(rates);
      }
    };

    cb();
  }, [getIsMounted]);

  const submit = async () => {
    const inputErrors = await context.validateAll();
    if (inputErrors.length > 0) {
      return;
    }

    const workItems = activities.flatMap((activity) =>
      activity.workItems.filter((w) => w.rateId !== 0 && w.total > 0)
    );

    const errors =
      workItems.length === 0
        ? ["No expenses were completed"]
        : await checkValidity(workItems, yup.array().of(workItemSchema));
    setInputErrors(errors);
    if (errors.length > 0) {
      return;
    }

    // transform activity workItems to expenses
    // we don't need to send along the whole rate description every time and we shouldn't pass along our internal ids
    const expensesBody = activities.flatMap((activity) =>
      activity.workItems
        .filter((w) => w.rateId !== 0)
        .flatMap(
          (workItem): Expense => ({
            id: 0,
            activity: activity.name,
            description: workItem.description,
            price: workItem.rate,
            type: workItem.type,
            quantity: workItem.quantity,
            markup: workItem.markup,
            total: workItem.total,
            rateId: workItem.rateId,
            rate: null,
          })
        )
    );

    const quote: QuoteContent = {
      projectName: project.name,
      acres: 0,
      acreageRate: 0,
      years: 0,
      acreageTotal: 0,
      activitiesTotal: 0,
      //sum up all activities with a labor type
      laborTotal: activities.reduce(
        (acc, activity) =>
          acc +
          activity.workItems
            .filter((w) => w.type === "Labor")
            .reduce((acc, workItem) => acc + workItem.total, 0),
        0
      ),
      //sum up all activities with a equipment type
      equipmentTotal: activities.reduce(
        (acc, activity) =>
          acc +
          activity.workItems
            .filter((w) => w.type === "Equipment")
            .reduce((acc, workItem) => acc + workItem.total, 0),
        0
      ),
      //sum up all activities with an other type
      otherTotal: activities.reduce(
        (acc, activity) =>
          acc +
          activity.workItems
            .filter((w) => w.type === "Other")
            .reduce((acc, workItem) => acc + workItem.total, 0),
        0
      ),
      //sum up all activities
      grandTotal: activities.reduce((acc, activity) => acc + activity.total, 0),
      fields: [],
      activities: activities.filter((a) => a.workItems.length > 0),
      acreageRateId: null,
      acreageRateDescription: "",
    };

    const adhoc: AdhocProjectModel = {
      project: project,
      expenses: expensesBody,
      accounts: accounts,
      quote: quote,
    };

    //TODO: Change to new api
    const request = authenticatedFetch(`/api/Project/CreateAdhoc`, {
      method: "POST",
      body: JSON.stringify(adhoc), //TODO: use correct model
    });

    setNotification(request, "Saving Expenses", "Expenses Saved");

    const response = await request;

    if (response.ok) {
      const data = await response.json();
      history.push(`/Project/Details/${data.id}`);
    }
  };

  const updateActivity = (activity: Activity) => {
    // TODO: can we get away without needing to spread copy?  do we need to totally splice/replace?
    const activityIndex = activities.findIndex((a) => a.id === activity.id);
    activities[activityIndex] = {
      ...activity,
      total: activity.workItems.reduce(
        (prev, curr) => prev + curr.total || 0,
        0
      ),
    };

    setActivities([...activities]);
  };
  const deleteActivity = (activity: Activity) => {
    setActivities((acts) => acts.filter((a) => a.id !== activity.id));
  };

  const addActivity = () => {
    const newActivityId = Math.max(...activities.map((a) => a.id), 0) + 1;
    setActivities((acts) => [...acts, getDefaultActivity(newActivityId)]);
  };
  // return true if the sum of the activity totals is greater than 0
  const isValid = () =>
    activities.reduce((prev, curr) => prev + curr.total || 0, 0) > 0;

  const handleCropTypeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setProject({ ...project, cropType: e.target.value as CropType });
  };

  return (
    <ValidationProvider context={context}>
      <div className="card-wrapper">
        <div className="card-content">
          <h1>Ad-Hoc Project Details</h1>

          <div
            className="card-wrapper mb-4 no-green"
            style={{ overflow: "visible" }}
          >
            <div className="card-content">
              <div className="row justify-content-between align-items-end">
                <div className="col-md-12">
                  <div className="row justify-content-between mb-2">
                    <div className="col">
                      <FormGroup>
                        <Label>Project Name</Label>
                        <Input
                          type="text"
                          name="text"
                          id="name"
                          value={project.name}
                          onChange={onChange("name", (e) =>
                            setProject({ ...project, name: e.target.value })
                          )}
                          onBlur={onBlur("name")}
                          placeholder="Unique project name"
                        />
                        <InputErrorMessage name="name" />
                      </FormGroup>
                      <FormGroup>
                        <Label>Which type of crop will we grow?</Label>
                        <div className="custom-control custom-radio">
                          <input
                            type="radio"
                            id="rowCropInput"
                            name="rowCropInput"
                            className="custom-control-input"
                            style={{ zIndex: 1 }} //prevent class custom-control-input from blocking mouse clicks
                            value="Row"
                            checked={project.cropType === "Row"}
                            onChange={handleCropTypeChange}
                          />
                          <label
                            className="custom-control-label"
                            htmlFor="rowCropInput"
                          >
                            Row Crops
                          </label>
                        </div>
                        <div className="custom-control custom-radio">
                          <input
                            type="radio"
                            id="treeCropInput"
                            name="treeCropInput"
                            className="custom-control-input"
                            style={{ zIndex: 1 }} //prevent class custom-control-input from blocking mouse clicks
                            value="Tree"
                            checked={project.cropType === "Tree"}
                            onChange={handleCropTypeChange}
                          />
                          <label
                            className="custom-control-label"
                            htmlFor="treeCropInput"
                          >
                            Tree Crops
                          </label>
                        </div>
                      </FormGroup>
                      <FormGroup tag="fieldset">
                        <Label>What crop(s) will we grow?</Label>
                        <Crops
                          crops={project.crop}
                          onChange={onChangeValue("crop", (c) =>
                            setProject({ ...project, crop: c })
                          )}
                          cropType={project.cropType}
                          onBlur={() => onBlurValue("crop")}
                        />
                        <InputErrorMessage name="crop" />
                      </FormGroup>

                      <FormGroup>
                        <Label>Who will be the PI?</Label>
                        <SearchPerson
                          user={project.principalInvestigator}
                          onChange={onChangeValue(
                            "principalInvestigator",
                            (u) =>
                              setProject({
                                ...project,
                                principalInvestigator: u,
                              })
                          )}
                          onBlur={() => onBlurValue("principalInvestigator")}
                        />
                        <InputErrorMessage name="principalInvestigator" />
                        {!propertyHasErrors("principalInvestigator") && (
                          <small id="PIHelp" className="form-text text-muted">
                            Enter PI Email or Kerberos. Click [x] to clear out
                            an existing PI.
                          </small>
                        )}
                      </FormGroup>

                      <FormGroup>
                        <Label>What are the requirements?</Label>
                        <Input
                          type="textarea"
                          name="text"
                          id="requirements"
                          value={project.requirements}
                          onChange={onChange("requirements", (e) =>
                            setProject({
                              ...project,
                              requirements: e.target.value,
                            })
                          )}
                          onBlur={onBlur("requirements")}
                          placeholder="Enter a full description of your requirements"
                        />
                        <InputErrorMessage name="requirements" />
                      </FormGroup>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <br />

        <div className="card-content">
          <h1>Account Details</h1>
          <div
            className="card-wrapper mb-4 no-green"
            style={{ overflow: "visible" }}
          >
            <div className="card-content">
              <div className="row justify-content-between align-items-end">
                <div className="col-md-12">
                  <div className="row justify-content-between mb-2">
                    <div className="col">
                      <AccountsInput
                        accounts={accounts}
                        setAccounts={setAccounts}
                        setDisabled={setDisabled}
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <br />
        <div className="card-content">
          <h1>Add Expenses</h1>
          <div>
            {activities.map((activity) => (
              <ActivityForm
                key={`activity-${activity.id}`}
                activity={activity}
                updateActivity={(activity: Activity) =>
                  updateActivity(activity)
                }
                deleteActivity={(activity: Activity) =>
                  deleteActivity(activity)
                }
                rates={rates}
              />
            ))}
          </div>
          <Button className="mb-4" color="link" size="lg" onClick={addActivity}>
            Add Activity <FontAwesomeIcon icon={faPlus} />
          </Button>
        </div>
        {inputErrors.length > 0 && (
          <div className="card-content">
            <ul>
              {inputErrors.map((error, i) => {
                return (
                  <li className="text-danger" key={`error-${i}`}>
                    {error}
                  </li>
                );
              })}
            </ul>
          </div>
        )}
        <div className="card-content">
          <div className="col">
            <button
              className="btn btn-primary btn-lg btn-block"
              onClick={submit}
              disabled={
                disabled ||
                notification.pending ||
                !isValid() ||
                context.formErrorCount > 0
              }
            >
              Create Ad-hoc Project
            </button>
          </div>
        </div>
      </div>
    </ValidationProvider>
  );
};
