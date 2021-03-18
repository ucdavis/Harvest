import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { Project, ProjectWithQuotes } from "../types";

interface RouteParams {
  projectId?: string;
}

export const QuoteContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();

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
    <div className="card">
      <div className="card-body">
        <h5 className="card-title">Field Request #{project.id}</h5>
        <p>PI: {project.principalInvestigator.name}</p>
        <span>
          Created {new Date(project.createdOn).toDateString()} by{" "}
          {project.createdBy.name}
        </span>

        <p className="card-text">
          Some quick example text to build on the card title and make up the
          bulk of the card's content.
        </p>
      </div>
    </div>
  );
};
