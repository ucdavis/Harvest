import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import {
  Project,
  ProjectWithQuote,
  QuoteContent,
  QuoteContentImpl,
  Rate,
} from "../types";

import { FieldContainer } from "../Fields/FieldContainer";
import { ProjectDetail } from "./ProjectDetail";
import { ProjectHeader } from "../Requests/ProjectHeader";
import { ActivitiesContainer } from "./ActivitiesContainer";
import { QuoteTotals } from "./QuoteTotals";
import { clone } from "../Util/state-helpers";

interface RouteParams {
  projectId?: string;
}

export const QuoteContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();

  // TODO: set with in-progress quote details if they exist
  // For now, we just always initialize an empty quote
  const [quote, setQuote] = useState<QuoteContent>(new QuoteContentImpl());
  const [rates, setRates] = useState<Rate[]>([]);

  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await fetch(`/Quote/Get/${projectId}`);
      const pricingResponse = await fetch("/Rate/Active");

      if (quoteResponse.ok && pricingResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();
        const rateJson: Rate[] = await pricingResponse.json();
        setProject(projectWithQuote.project);
        setRates(rateJson);

        if (projectWithQuote.quote) {
          // TODO: remove once we standardize on new quote format
          setQuote({
            ...projectWithQuote.quote,
            fields: projectWithQuote.quote.fields ?? [],
          });
        } else {
          // TODO: how do we handle if different fields have different rates?
          const quoteToUse = new QuoteContentImpl();
          quoteToUse.acreageRate =
            rateJson.find((r) => r.type === "Acreage")?.price || 120;

          setQuote(quoteToUse);
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
      let acreageTotal = q.acreageRate * q.acres;
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
            case "labor":
              laborTotal += workItem.total || 0;
              break;
            case "equipment":
              equipmentTotal += workItem.total || 0;
              break;
            case "other":
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

  const save = async () => {
    // remove unused workitems and empty activities and apply to state only after successfully saving
    const savingQuote = clone(quote);
    savingQuote.activities.forEach((a) => a.workItems = a.workItems.filter((w) => w.quantity !== 0 || w.total !== 0));
    savingQuote.activities = savingQuote.activities.filter(a => a.workItems.length > 0);

    // TODO: add progress and hide info while saving
    const saveResponse = await fetch(`/Quote/Save/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(savingQuote),
    });

    if (saveResponse.ok) {
      setQuote(savingQuote);
      window.location.pathname = `/Project/Details/${projectId}`;
    } else {
      alert("Something went wrong, please try again");
    }
  };

  if (!project) {
    return <div>Loading</div>;
  }

  // TODO: perhaps we might want to go back and modify the fields as well
  if (quote.fields.length === 0) {
    return (
      <FieldContainer
        fields={quote.fields}
        updateFields={(fields) => setQuote({ ...quote, fields })}
      ></FieldContainer>
    );
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader project={project}></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <div className="quote-details">
            <h2>Quote Details</h2>
            <hr />
            <ProjectDetail rates={rates} quote={quote} updateQuote={setQuote} />
            <ActivitiesContainer
              quote={quote}
              rates={rates}
              updateQuote={setQuote}
            />
          </div>
          <QuoteTotals quote={quote}></QuoteTotals>

          <button className="btn btn-primary mt-4" onClick={save}>
            Save Quote
          </button>
        </div>
      </div>

      <div>Debug: {JSON.stringify(quote)}</div>
      <div>Debug Rates: {JSON.stringify(rates)}</div>
    </div>
  );
};
