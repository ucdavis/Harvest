import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import {
  Project,
  ProjectWithQuotes,
  QuoteContent,
  QuoteContentImpl,
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

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Quote/Get/${projectId}`);

      if (response.ok) {
        const projectWithQuotes: ProjectWithQuotes = await response.json();
        setProject(projectWithQuotes.project);
      } else {
        console.error(response);
      }
    };

    cb();
  }, [projectId]);

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
            <ActivitiesContainer quote={quote} updateQuote={setQuote} />
          </div>
        </div>
      </div>

      <div>Debug: {JSON.stringify(quote)}</div>
    </div>
  );
};
