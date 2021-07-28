import React, { useEffect, useMemo, useState } from "react";
import { useHistory, useParams } from "react-router-dom";

import {
  Project,
  ProjectWithQuote,
  QuoteContent,
  QuoteContentImpl,
  Rate,
} from "../types";

import { FieldContainer } from "../Fields/FieldContainer";
import { ProjectDetail } from "./ProjectDetail";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { ActivitiesContainer } from "./ActivitiesContainer";
import { QuoteTotals } from "./QuoteTotals";

interface RouteParams {
  projectId?: string;
}

export const QuoteContainer = () => {
  const history = useHistory();
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();

  // TODO: set with in-progress quote details if they exist
  // For now, we just always initialize an empty quote
  const [quote, setQuote] = useState<QuoteContent>(new QuoteContentImpl());
  const [rates, setRates] = useState<Rate[]>([]);

  const [editFields, setEditFields] = useState(false);

  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await fetch(`/Quote/Get/${projectId}`);
      const pricingResponse = await fetch("/Rate/Active");

      if (quoteResponse.ok && pricingResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();
        const rateJson: Rate[] = await pricingResponse.json();
        setProject(projectWithQuote.project);
        setRates(rateJson);

        if (projectWithQuote.project.status !== "Requested") {
          // can only create quote for newly requests projects.
          history.push(`/Project/Details/${projectId}`);
        }

        if (projectWithQuote.quote) {
          // TODO: remove once we standardize on new quote format
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
          const quoteToUse = new QuoteContentImpl();
          quoteToUse.acreageRate =
            rateJson.find((r) => r.type === "Acreage")?.price || 120;

          // alwyas at least 1 year worth of acreage, but use max in case there is more
          quoteToUse.years = Math.max(
            1,
            new Date(projectWithQuote.project.end).getFullYear() -
              new Date(projectWithQuote.project.start).getFullYear()
          );

          setQuote(quoteToUse);
          setEditFields(true); // we have no existing quote, start with editing fields
        }
      } else {
        !quoteResponse.ok && console.error(quoteResponse);
        !pricingResponse.ok && console.error(pricingResponse);
      }
    };

    cb();
  }, [projectId]);

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

  const cropArray = useMemo(() => (project ? project.crop.split(",") : []), [
    project,
  ]);

  const save = async (submit: boolean) => {
    // remove unused workitems and empty activities and apply to state only after successfully saving
    quote.activities.forEach(
      (a) =>
        (a.workItems = a.workItems.filter(
          (w) => w.quantity !== 0 || w.total !== 0
        ))
    );
    quote.activities = quote.activities.filter((a) => a.workItems.length > 0);

    // TODO: add progress and hide info while saving
    const saveResponse = await fetch(
      `/Quote/Save/${projectId}?submit=${submit}`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(quote),
      }
    );

    if (saveResponse.ok) {
      history.push(`/Project/Details/${projectId}`);
    } else {
      alert("Something went wrong, please try again");
    }
  };

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
        <div>Debug: {JSON.stringify(quote)}</div>
      </div>
    );
  }

  return (
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
          </div>
          <QuoteTotals quote={quote}></QuoteTotals>
          <div className="row justify-content-center">
            <button className="btn btn-link mt-4" onClick={() => save(false)}>
              Save Quote
            </button>
            <button className="btn btn-primary mt-4" onClick={() => save(true)}>
              Submit Quote
            </button>
          </div>
        </div>
      </div>

      <div>Debug: {JSON.stringify(quote)}</div>
      <div>Debug Rates: {JSON.stringify(rates)}</div>
    </div>
  );
};
