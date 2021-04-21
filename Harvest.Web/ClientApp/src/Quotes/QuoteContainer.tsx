import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import {
  Project,
  ProjectWithQuote,
  QuoteContent,
  QuoteContentImpl,
  Rate,
} from "../types";

import { ProjectDetail } from "./ProjectDetail";
import { RequestHeader } from "../Requests/RequestHeader";
import { ActivitiesContainer } from "./ActivitiesContainer";

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
          setQuote(projectWithQuote.quote);
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
    setQuote((q) => ({ ...q, acreageTotal: q.acreageRate * q.acres }));
  }, [quote.acreageRate, quote.acres]);

  useEffect(() => {
    setQuote((q) => {
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
        activitiesTotal,
        laborTotal,
        equipmentTotal,
        otherTotal,
        grandTotal: activitiesTotal + q.acreageTotal,
      };
    });
  }, [quote.activities]);

  const save = async () => {
    // TODO: add progress and hide info while saving
    const saveResponse = await fetch(`/Quote/Save/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(quote),
    });

    if (saveResponse.ok) {
      window.location.pathname = `/Project/Details/${projectId}`;
    } else {
      alert("Something went wrong, please try again");
    }
  };

  if (!project) {
    return <div>Loading</div>;
  }

  return (
    <div className="card-wrapper">
      <RequestHeader project={project}></RequestHeader>
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
          <h2>Save</h2>
          <button onClick={save}>Save Quote</button>
        </div>
      </div>

      <div>Debug: {JSON.stringify(quote)}</div>
      <div>Debug Rates: {JSON.stringify(rates)}</div>
    </div>
  );
};
