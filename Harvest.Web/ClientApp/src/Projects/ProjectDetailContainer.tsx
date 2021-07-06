import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { Progress } from "reactstrap";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";

import { ProjectHeader } from "../Shared/ProjectHeader";
import { RecentInvoicesContainer } from "../Invoices/RecentInvoicesContainer";
import { ProjectUnbilledButton } from "./ProjectUnbilledButton";
import { Project } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

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

              <Link
                className="btn btn-primary btn-small mr-4"
                to={`/request/changeAccount/${project.id}`}
              >
                Change Accounts
              </Link>
              <Link
                className="btn btn-primary btn-small mr-4"
                to={`/ticket/create/${project.id}`}
              >
                Create Ticket
              </Link>
            </div>
            <div className="col text-right">
              <ProjectUnbilledButton
                projectId={project.id}
              ></ProjectUnbilledButton>
            </div>
          </div>
        </div>
      </div>
      <div className="card-green-bg green-bg-border pt-3 pb-3">
        <div className="card-content">
          <div className="row justify-content-between">
            <div className="col">
              <h1>Project Progress</h1>
              <div className="row justify-content-between">
                <div className="col">
                  <p className="mb-1">
                    ${formatCurrency(project.chargedTotal)} Billed
                  </p>
                </div>
                <div className="col text-right">
                  <p className="mb-1">
                    ${formatCurrency(project.quoteTotal - project.chargedTotal)}{" "}
                    Remaining
                  </p>
                </div>
              </div>
              <Progress
                style={{ width: "100%", height: "20px" }}
                value={(project.chargedTotal / project.quoteTotal) * 100}
              />
            </div>
          </div>
        </div>
      </div>
      <div>
        <div className="row justify-content-around">
          <div className="col-md-5">
            <div className="card-wrapper no-green mt-4 mb-4">
              <div className="card-content">
                <h2>Project Attachements</h2>
                !! Add upload file input here !!
                <ul className="no-list-style attached-files-list">
                  <li>
                    <a href="#">
                      <FontAwesomeIcon icon={faDownload} />
                      Filename 1.pdf
                    </a>{" "}
                    uploaded 9.23.2021
                  </li>
                  <li>
                    <a href="#">
                      <FontAwesomeIcon icon={faDownload} />
                      Filename 1.pdf
                    </a>{" "}
                    uploaded 9.23.2021
                  </li>
                </ul>
              </div>
            </div>
          </div>
          <div className="col-md-6">
            <RecentInvoicesContainer
              compact={true}
              projectId={projectId}
            ></RecentInvoicesContainer>
          </div>
        </div>
      </div>
    </div>
  );
};
