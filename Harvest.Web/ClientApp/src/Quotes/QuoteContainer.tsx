import React, { useEffect, useMemo, useState } from "react";
import { useHistory, useParams } from "react-router-dom";

import {
  CommonRouteParams,
  Project,
  ProjectWithQuote,
  QuoteContent,
  QuoteContentImpl,
  Rate,
  WorkItemImpl,
} from "../types";

import { FieldContainer } from "../Fields/FieldContainer";
import { ProjectDetail } from "./ProjectDetail";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { ActivitiesContainer } from "./ActivitiesContainer";
import { QuoteTotals } from "./QuoteTotals";

import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useInputValidator, ValidationProvider } from "use-input-validator";
import { quoteContentSchema } from "../schemas";
import { checkValidity } from "../Util/ValidationHelpers";
import { useIsMounted } from "../Shared/UseIsMounted";
import { ShowFor } from "../Shared/ShowFor";
import { validatorOptions } from "../constants";
import { Button } from "reactstrap";

export const QuoteContainer = () => {
  const history = useHistory();
  const { projectId, team } = useParams<CommonRouteParams>();
  const [project, setProject] = useState<Project>();
  const [inputErrors, setInputErrors] = useState<string[]>([]);

  // TODO: set with in-progress quote details if they exist
  // For now, we just always initialize an empty quote
  const [quote, setQuote] = useState<QuoteContent>(new QuoteContentImpl());
  const [rates, setRates] = useState<Rate[]>([]);

  const { formErrorCount, context, validateAll } = useInputValidator(
    quoteContentSchema,
    quote,
    validatorOptions
  );

  const [editFields, setEditFields] = useState(false);

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await authenticatedFetch(
        `/api/Quote/Get/${projectId}`
      );
      const pricingResponse = await authenticatedFetch(
        `/api/${team}/Rate/Active`
      );

      if (quoteResponse.ok && pricingResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();
        const rateJson: Rate[] = await pricingResponse.json();
        if (getIsMounted()) {
          setProject(projectWithQuote.project);
          setRates(rateJson);

          if (
            projectWithQuote.project.status !== "Requested" &&
            projectWithQuote.project.status !== "ChangeRequested" &&
            projectWithQuote.project.status !== "QuoteRejected"
          ) {
            // can only create quote for newly requests projects or change requests.
            history.push(`/${team}/Project/Details/${projectId}`);
          }

          if (projectWithQuote.quote) {
            // quote already exists, so we'll just set it
            setQuote({
              ...projectWithQuote.quote,
              fields: projectWithQuote.quote.fields ?? [],
            });

            if (
              !projectWithQuote.quote.fields ||
              projectWithQuote.quote.fields.length === 0
            ) {
              setEditFields(true);
            }
          } else {
            // quote doesn't exist, create and set default values
            const quoteToUse = new QuoteContentImpl();
            quoteToUse.acreageRate =
              rateJson.find((r) => r.type === "Acreage")?.price || 120;
            quoteToUse.projectName = projectWithQuote.project.name;

            // alwyas at least 1 year worth of acreage, but use max in case there is more
            quoteToUse.years = Math.max(
              1,
              new Date(projectWithQuote.project.end).getFullYear() -
                new Date(projectWithQuote.project.start).getFullYear()
            );

            setQuote(quoteToUse);
            setEditFields(true); // we have no existing quote, start with editing fields
          }
        }
      } else {
        !quoteResponse.ok && console.error(quoteResponse);
        !pricingResponse.ok && console.error(pricingResponse);
      }
    };

    cb();
  }, [history, projectId, getIsMounted]);

  useEffect(() => {
    setQuote((q) => {
      let acreageTotal = q.acreageRate * q.acres * q.years;
      let activitiesTotal = 0;
      let laborTotal = 0;
      let equipmentTotal = 0;
      let otherTotal = 0;

      for (let i = 0; i < q.activities.length; i++) {
        const activity = q.activities[i];

        for (let j = 0; j < activity.workItems.length; j++) {
          const workItem = activity.workItems[j];

          activitiesTotal += workItem.total || 0;

          switch (workItem.type) {
            case "Labor":
              laborTotal += workItem.total || 0;
              break;
            case "Equipment":
              equipmentTotal += workItem.total || 0;
              break;
            case "Other":
              otherTotal += workItem.total || 0;
              break;
          }
        }
      }

      return {
        ...q,
        acreageTotal,
        activitiesTotal,
        laborTotal,
        equipmentTotal,
        otherTotal,
        grandTotal: activitiesTotal + acreageTotal,
      };
    });
  }, [quote.activities, quote.acreageRate, quote.acres]);

  const cropArray = useMemo(
    () => (project ? project.crop.split(",") : []),
    [project]
  );

  const addActivity = () => {
    const newActivityId = Math.max(...quote.activities.map((a) => a.id), 0) + 1;
    setQuote({
      ...quote,
      activities: [
        ...quote.activities,
        {
          id: newActivityId,
          name: "Activity",
          total: 0,
          workItems: [
            new WorkItemImpl(newActivityId, 1, "Labor"),
            new WorkItemImpl(newActivityId, 2, "Equipment"),
            new WorkItemImpl(newActivityId, 3, "Other"),
          ],
          year: 1, // default new activity to no adjustment
          adjustment: 0,
        },
      ],
    });
  };

  const save = async (submit: boolean) => {
    const errors = await validateAll();
    if (errors.length > 0) {
      return;
    }
    // remove unused workitems and empty activities and apply to state only after successfully saving
    quote.activities.forEach(
      (a) =>
        (a.workItems = a.workItems.filter(
          (w) => w.quantity !== 0 || w.total !== 0
        ))
    );
    quote.activities = quote.activities.filter((a) => a.workItems.length > 0);

    if (submit) {
      const errors = await checkValidity(quote, quoteContentSchema);
      setInputErrors(errors);
      if (errors.length > 0) {
        return;
      }
    }

    const request = authenticatedFetch(
      `/api/Quote/Save/${projectId}?submit=${submit}`,
      {
        method: "POST",
        body: JSON.stringify(quote),
      }
    );

    if (submit) {
      setNotification(request, "Submitting Quote", "Quote Submitted");
    } else {
      setNotification(request, "Saving Quote", "Quote Saved");
    }

    // TODO: add progress and hide info while saving
    const saveResponse = await request;

    if (saveResponse.ok) {
      history.push(`/${team}/Project/Details/${projectId}`);
    }
  };

  const isValid = () => quote.grandTotal > 0;

  if (!project) {
    return <div>Loading</div>;
  }

  // TODO: we might want to move this all into a separate component
  if (editFields) {
    return (
      <div>
        <div className="card-wrapper">
          <ProjectHeader
            project={project}
            title={"Field Request #" + (project?.id || "")}
          ></ProjectHeader>

          <div className="card-green-bg">
            <div className="card-content">
              <div className="row">
                <div className="col-md-6">
                  <h3>Choose a location</h3>
                  Instructions:
                  <ol>
                    <li>
                      Draw your field boundaries using the rectangle or polygon
                      tool in the upper-right
                    </li>
                    <li>Fill in the field details and click "Confirm"</li>
                    <li>
                      You can add in as many fields as you like, or click an
                      existing field for more info and actions
                    </li>
                    <li>
                      To edit a field, click on it in the map and choose either
                      edit or remove.
                    </li>
                    <li>When you are finished, click confirm below</li>
                  </ol>
                  <button
                    className="btn btn-primary"
                    onClick={(_) => setEditFields(false)}
                  >
                    Confirm Field Locations
                  </button>
                </div>
              </div>
            </div>
          </div>

          <FieldContainer
            crops={cropArray}
            fields={quote.fields}
            project={project}
            updateFields={(fields) => setQuote({ ...quote, fields })}
          ></FieldContainer>
        </div>
      </div>
    );
  }

  return (
    <ValidationProvider context={context}>
      <div className="card-wrapper">
        <ProjectHeader
          project={project}
          title={"Field Request #" + (project?.id || "")}
        ></ProjectHeader>
        <div className="card-green-bg">
          <div className="card-content">
            <div className="quote-details">
              <h2>Quote Details</h2>
              <hr />
              <ProjectDetail
                rates={rates}
                quote={quote}
                updateQuote={setQuote}
                setEditFields={setEditFields}
              />
              <ActivitiesContainer
                quote={quote}
                rates={rates}
                updateQuote={setQuote}
              />
              <br />
              <Button
                className="mb-4"
                color="primary"
                size="lg"
                onClick={addActivity}
              >
                Add Activity
              </Button>
            </div>
            <QuoteTotals quote={quote}></QuoteTotals>
            <div className="row justify-content-center">
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
            <div className="row justify-content-center mb-4">
              {/* Supervisor can only see save option, so it becomes a primary button.  FM can do both save and submit */}
              <ShowFor roles={["Supervisor"]}>
                <button
                  className="btn btn-primary mt-4"
                  onClick={() => save(false)}
                  disabled={notification.pending || formErrorCount > 0}
                >
                  Save Quote
                </button>
              </ShowFor>
              <ShowFor roles={["FieldManager"]}>
                <button
                  className="btn btn-link mt-4"
                  onClick={() => save(false)}
                  disabled={notification.pending || formErrorCount > 0}
                >
                  Save Quote
                </button>
                <button
                  className="btn btn-primary mt-4"
                  onClick={() => save(true)}
                  disabled={
                    notification.pending || !isValid() || formErrorCount > 0
                  }
                >
                  Submit Quote
                </button>
              </ShowFor>
            </div>
          </div>
        </div>
      </div>
    </ValidationProvider>
  );
};
