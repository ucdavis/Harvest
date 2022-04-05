import React, { useCallback, useContext, useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import toast from "react-hot-toast";
import {
  Activity,
  Expense,
  Rate,
  WorkItemImpl,
  ExpenseQueryParams,
  Project,
} from "../types";
import { ActivityForm } from "../Quotes/ActivityForm";
import { Button } from "reactstrap";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";
import { usePromiseNotification } from "../Util/Notifications";
import AppContext from "../Shared/AppContext";

import {
  useOrCreateValidationContext,
  ValidationProvider,
} from "use-input-validator";
import { workItemSchema } from "../schemas";
import { checkValidity } from "../Util/ValidationHelpers";
import * as yup from "yup";
import { useQuery } from "../Shared/UseQuery";
import { useIsMounted } from "../Shared/UseIsMounted";
import { validatorOptions } from "../constants";
import { authenticatedFetch } from "../Util/Api";
import { convertCamelCase } from "../Util/StringFormatting";

interface RouteParams {
  projectId?: string;
}

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

  const [rates, setRates] = useState<Rate[]>([]);
  const [inputErrors, setInputErrors] = useState<string[]>([]);
  const context = useOrCreateValidationContext(validatorOptions);

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

        // create default activity
      }
    };

    cb();
  }, [getIsMounted]);

  const submit = async () => {
    // TODO: some sort of full screen processing UI

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

    //TODO: Change to new api
    const request = authenticatedFetch(`/api/Project/CreateAdhoc`, {
      method: "POST",
      body: JSON.stringify(expensesBody), //TODO: use correct model
    });

    setNotification(request, "Saving Expenses", "Expenses Saved");

    const response = await request;

    if (response.ok) {
      history.push("/"); //TODO: Open project that was created instead
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

  return (
    <ValidationProvider context={context}>
      <div className="card-wrapper">
        <div className="card-content">
          <h1>Ad-Hoc Project Details</h1>
          <br />
          <div>SOME INPUTS HERE</div>
        </div>

        <br />

        <div className="card-content">
          <h1>Account Details</h1>
          <br />
          <div>SOME INPUTS HERE</div>
        </div>

        <br />
        <div className="card-content">
          <h1>Add Expenses</h1>
          <br />
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
                notification.pending || !isValid() || context.formErrorCount > 0
              }
            >
              Submit Expense
            </button>
          </div>
        </div>
      </div>
    </ValidationProvider>
  );
};
