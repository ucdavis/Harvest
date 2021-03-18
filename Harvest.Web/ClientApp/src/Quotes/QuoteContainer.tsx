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
          <h4>Requirements</h4>
          <p>{project.requirements}</p>
        </p>
        <div>
          <h4>Status</h4>
          <p>{project.status}</p>
          <h4>Type</h4>
          <p>TODO ROW CROPS</p>
          <h4>Timeline</h4>
          <p>
            {new Date(project.start).toLocaleDateString()} through{" "}
            {new Date(project.end).toLocaleDateString()}
          </p>
          <h4>Crops</h4>
          <p>{project.crop}</p>
        </div>
      </div>
    </div>
  );
};
