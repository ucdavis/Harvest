import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import {
  Project,
  ProjectWithQuote,
  QuoteContent,
  QuoteContentImpl,
  Rate,
} from "../types";

import { ProjectDetail } from "./ProjectDetail";
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

        // TODO: load up existing quote if it exists
        // TODO: how do we handle if different fields have different rates?
        const quoteToUse = new QuoteContentImpl();
        quoteToUse.acreageRate =
          rateJson.find((r) => r.type === "Acreage")?.price || 120;

        setQuote(quoteToUse);
      } else {
        !quoteResponse.ok && console.error(quoteResponse);
        !pricingResponse.ok && console.error(pricingResponse);
      }
    };

    cb();
  }, [projectId]);

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
      alert("saved");
    }
  };

  if (!project) {
    return <div>Loading</div>;
  }

  return (
    <div className="card-wrapper">
      <div className="card-content">
        <div className="quote-info row">
          <div className="col-md-6">
            <h2 id="request-title">Field Request #{project.id}</h2>
            <p className="lede">PI: {project.principalInvestigator.name}</p>
            <p>
              Created {new Date(project.createdOn).toDateString()} by{" "}
              {project.createdBy.name}
            </p>
            <p className="lede">Requirements</p>
            <p>{project.requirements}</p>
          </div>
          <div className="col-md-6 quote-info-box">
            <div className="row">
              <div className="col">
                <p className="lede">Status</p>
                <p className="quote-status">{project.status}</p>
                <p className="lede">Type</p>
                <p>TODO ROW CROPS</p>
              </div>
              <div className="col">
                <p className="lede">Timeline</p>
                <p>
                  {new Date(project.start).toLocaleDateString()} through{" "}
                  {new Date(project.end).toLocaleDateString()}
                </p>
                <p className="lede">Crops</p>
                <p>{project.crop}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="card-green-bg">
        <div className="card-content">
          <div className="quote-details">
            <h2>Quote Details</h2>
            <hr />
            <ProjectDetail quote={quote} updateQuote={setQuote} />
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
