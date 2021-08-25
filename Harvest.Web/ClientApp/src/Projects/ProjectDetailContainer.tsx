import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { Progress } from "reactstrap";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";

import { FileUpload } from "../Shared/FileUpload";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { RecentInvoicesContainer } from "../Invoices/RecentInvoicesContainer";
import { RecentTicketsContainer } from "../Tickets/RecentTicketsContainer";
import { ProjectUnbilledButton } from "./ProjectUnbilledButton";
import { BlobFile, Project } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { ShowFor } from "../Shared/ShowFor";
import { usePromiseNotification } from "../Util/Notifications";
import { millisecondsPerDay } from "../Util/Calculations";
import { ProjectProgress } from "./ProjectProgress";

interface RouteParams {
  projectId?: string;
}

export const ProjectDetailContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [newFiles, setNewFiles] = useState<BlobFile[]>([]);

  const [notification, setNotification] = usePromiseNotification();
  const [beyondCloseoutDisplayDate, setBeyondCloseoutDisplayDate] = useState(false);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        const project = await response.json() as Project;
        setProject(project);

        //show Closeout link if we are in last week of or beyond project end 
        setBeyondCloseoutDisplayDate(new Date(project.end).getTime() - (new Date().getTime()) >= (7 * millisecondsPerDay));
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  const updateFiles = async (attachments: BlobFile[]) => {
    const request = fetch(`/Request/Files/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ Attachments: attachments }),
    });

    setNotification(request, "Saving File(s)", "File(s) Saved");

    const response = await request;

    if (response.ok) {
      const files = await response.json();
      setProject({
        ...project,
        attachments: [...project.attachments, ...files],
      });
    }
  };

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project?.id || "")}
      />
      <div className="card-green-bg">
        <div className="card-content">
          <div className="row justify-content-between">
            <div className="col-md-7">
              <ShowFor
                roles={["Supervisor", "FieldManager"]}
                condition={
                  project.status === "Active" ||
                  project.status === "AwaitingCloseout"
                }
              >
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/expense/entry/${project.id}`}
                >
                  Enter Expenses
                </Link>
              </ShowFor>
              <ShowFor
                roles={["FieldManager", "Supervisor"]}
                condition={
                  project.status === "Requested" ||
                  project.status === "ChangeRequested" ||
                  project.status === "QuoteRejected"
                }
              >
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/quote/create/${project.id}`}
                >
                  Edit Quote
                </Link>
              </ShowFor>
              <ShowFor
                roles={["Supervisor", "FieldManager"]}
                condition={project.status === "AwaitingCloseout" || (project.status === "Active" && beyondCloseoutDisplayDate)}
              >
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/project/closeout/${project.id}`}
                >
                  Close Out Project
                </Link>
              </ShowFor>

              <ShowFor
                roles={["PI"]}
                condition={project.status === "PendingApproval"}
              >
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/request/approve/${project.id}`}
                >
                  Approve Quote
                </Link>
              </ShowFor>

              <ShowFor roles={["PI"]} condition={project.status === "Active"}>
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/request/changeAccount/${project.id}`}
                >
                  Change Accounts
                </Link>
              </ShowFor>
              <ShowFor roles={["PI"]} condition={project.status === "Active"}>
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/request/create/${project.id}`}
                >
                  Change Requirements
                </Link>
              </ShowFor>
            </div>
            <div className="col-md-5 text-right">
              <ProjectUnbilledButton projectId={project.id} />
            </div>
          </div>
        </div>
      </div>
      <div className="card-green-bg green-bg-border pt-3 pb-3">
        <div className="card-content">
          <ProjectProgress project={project} />
        </div>
      </div>
      <div>
        {project.status !== "ChangeRequested" && (
          <div className="row project-detail-tables">
            <div className="col-md-6">
              <RecentTicketsContainer compact={true} projectId={projectId} />
            </div>
            <div className="col-md-6">
              <RecentInvoicesContainer compact={true} projectId={projectId} />
            </div>
          </div>
        )}
        <div className="row justify-content-center">
          <div className="col-md-5">
            <div className="card-wrapper no-green mt-4 mb-4">
              <div className="card-content">
                <h2>Project Attachements</h2>
                <FileUpload
                  disabled={notification.pending}
                  files={newFiles}
                  setFiles={(f) => {
                    let files = f.slice(newFiles.length);
                    if (newFiles.length === 0) {
                      files = f;
                    }

                    setNewFiles([...f]);
                    updateFiles(files);
                  }}
                  updateFile={(f) => {
                    setNewFiles((oldFiles) => {
                      if (oldFiles) {
                        oldFiles[
                          oldFiles.findIndex(
                            (file) => file.identifier === f.identifier
                          )
                        ] = { ...f };
                        return [...oldFiles];
                      }

                      return oldFiles;
                    });
                  }}
                />
                <ul className="no-list-style attached-files-list">
                  {project.attachments.map((attachment, i) => (
                    <li key={`attachment-${i}`}>
                      <a
                        href={attachment.sasLink}
                        target="_blank"
                        rel="noreferrer"
                      >
                        <FontAwesomeIcon icon={faDownload} />
                        {attachment.fileName}
                      </a>{" "}
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
