import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ProjectHeader } from "../Requests/ProjectHeader";
import { InvoiceListContainer } from "../Invoices/InvoiceListContainer";
import { Progress } from "reactstrap";

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
          <div className="row justify-content-between">
            <div className="col">
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
            </div>
            <div className="col text-right">
              <a href="#" className="btn btn-light">
                View Unbilled Expenses - $124,555.54
              </a>
            </div>
          </div>
        </div>
      </div>
      <div className="card-green-bg">
        <div className="card-content">
          <div className="row justify-content-between">
            <div className="col">
              <h1>Project Progress</h1>
              <div className="row justify-content-between">
                <div className="col">
                  <p className="mb-1">$4,550 Billed</p>
                </div>
                <div className="col text-right">
                  <p className="mb-1">$220,000 Remaining</p>
                </div>
              </div>
              <Progress
                style={{ width: "100%", height: "20px" }}
                value={Math.random() * 100}
              />
            </div>
          </div>
          <div className="card-wrapper no-green mt-5">
            <InvoiceListContainer projectId={projectId}></InvoiceListContainer>
          </div>
        </div>
      </div>
    </div>
  );
};
