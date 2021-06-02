import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ProjectHeader } from "../Requests/ProjectHeader";
import { InvoiceListContainer } from "../Invoices/InvoiceListContainer";

import { Project } from "../types";

interface RouteParams {
  projectId?: string;
}

export const ProjectDetailContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        setProject(await response.json());
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project?.id || "")}
      ></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <Link
            className="btn btn-primary btn-small mr-4"
            to={`/quote/create/${project.id}`}
          >
            Create Quote
          </Link>

          <Link
            className="btn btn-primary btn-small mr-4"
            to={`/request/approve/${project.id}`}
          >
            Approve Quote
          </Link>

          {/* TODO: Update approve accounts page to react once we know what it should do */}
          <a
            className="btn btn-primary btn-small"
            href={`/project/accountApproval/${project.id}`}
          >
            Approve Accounts
          </a>
        </div>
      </div>
      <InvoiceListContainer projectId={projectId}></InvoiceListContainer>
    </div>
  );
};
