import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faCheck,
  faDownload,
  faEdit,
  faExchangeAlt,
  faEye,
  faTimes,
} from "@fortawesome/free-solid-svg-icons";

import { FileUpload } from "../Shared/FileUpload";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { RecentInvoicesContainer } from "../Invoices/RecentInvoicesContainer";
import { RecentTicketsContainer } from "../Tickets/RecentTicketsContainer";
import { ProjectUnbilledButton } from "./ProjectUnbilledButton";
import { BlobFile, Project } from "../types";
import { ShowFor } from "../Shared/ShowFor";
import { ShowForPiOnly } from "../Shared/ShowForPiOnly";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";
import { useHistory } from "react-router-dom";
import { ProjectProgress } from "./ProjectProgress";
import { ProjectAlerts } from "./ProjectAlerts";

interface RouteParams {
  projectId?: string;
}

export const ProjectDetailContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [newFiles, setNewFiles] = useState<BlobFile[]>([]);
  const history = useHistory();

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(`/api/Project/Get/${projectId}`);

      if (response.ok) {
        const project = (await response.json()) as Project;
        getIsMounted() && setProject(project);
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  const updateFiles = async (attachments: BlobFile[]) => {
    const request = fetch(`/api/Request/Files/${projectId}`, {
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
      getIsMounted() &&
        setProject({
          ...project,
          attachments: [...project.attachments, ...files],
        });
    }
  };

  //cancel the project
  const cancelProject = async () => {
    const request = fetch(`/api/Request/Cancel/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
    });
    setNotification(request, "Canceling", "Project Request Canceled");
    const response = await request;
    if (response.ok) {
      //redirect to home
      history.push("/");
    }
  };

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project?.id || "")}
        hideBack={true}
      />
      <ShowFor
        roles={["FieldManager", "Supervisor"]}
        condition={
          project.status === "Requested" ||
          project.status === "ChangeRequested" ||
          project.status === "QuoteRejected"
        }
      >
        <ProjectAlerts project={project} />
      </ShowFor>
      <ShowForPiOnly
        project={project}
        condition={project.status === "PendingApproval"}
      >
        <ProjectAlerts project={project} />
      </ShowForPiOnly>
      <div className="card-green-bg">
        <div className="card-content">
          <div className="row justify-content-between">
            <div className="col-md-12 project-actions">
              <h4>Project actions</h4>
              <ShowFor
                roles={["Supervisor", "FieldManager"]}
                condition={
                  project.status === "Active" ||
                  project.status === "AwaitingCloseout" ||
                  project.status === "PendingCloseoutApproval"
                }
              >
                <Link
                  className="btn btn-primary btn-sm mr-4"
                  to={`/expense/entry/${project.id}`}
                >
                  Enter Expenses <FontAwesomeIcon icon={faEdit} />
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
                  className="btn btn-primary btn-sm mr-4"
                  to={`/quote/create/${project.id}`}
                >
                  Edit Quote <FontAwesomeIcon icon={faEdit} />
                </Link>
              </ShowFor>
              <ShowFor
                roles={["FieldManager"]}
                condition={
                  project.status === "Requested" ||
                  project.status === "ChangeRequested" ||
                  project.status === "QuoteRejected"
                }
              >
                <button
                  className="btn btn-danger btn-sm mr-4 float-right"
                  onClick={() => cancelProject()}
                >
                  Cancel Request <FontAwesomeIcon icon={faTimes} />
                </button>
              </ShowFor>
              <ShowFor
                roles={["FieldManager"]}
                condition={
                  project.status === "AwaitingCloseout" ||
                  project.status === "Active"
                }
              >
                <Link
                  className="btn btn-primary btn-sm mr-4"
                  to={`/project/closeout/${project.id}`}
                >
                  Close Out Project <FontAwesomeIcon icon={faCheck} />
                </Link>
              </ShowFor>
              <ShowForPiOnly
                project={project}
                condition={project.status === "PendingCloseoutApproval"}
              >
                <Link
                  className="btn btn-primary btn-sm mr-4"
                  to={`/project/closeoutconfirmation/${project.id}`}
                >
                  Confirm Close Out
                </Link>
              </ShowForPiOnly>

              <ShowForPiOnly
                project={project}
                condition={project.status === "PendingApproval"}
              >
                <Link
                  className="btn btn-primary btn-sm mr-4"
                  to={`/request/approve/${project.id}`}
                >
                  View Quote <FontAwesomeIcon icon={faEye} />
                </Link>
              </ShowForPiOnly>
              <ShowForPiOnly
                project={project}
                condition={project.status === "Active"}
              >
                <Link
                  className="btn btn-primary btn-sm mr-4"
                  to={`/request/changeAccount/${project.id}`}
                >
                  Change Accounts <FontAwesomeIcon icon={faExchangeAlt} />
                </Link>
              </ShowForPiOnly>
              <ShowFor
                roles={["PI", "FieldManager"]}
                condition={project.status === "Active"}
              >
                <Link
                  className="btn btn-primary btn-sm mr-4"
                  to={`/request/create/${project.id}`}
                >
                  Change Requirements <FontAwesomeIcon icon={faExchangeAlt} />
                </Link>
              </ShowFor>
              <ShowFor
                roles={["PI", "FieldManager"]}
                condition={
                  // all statuses with approved quotes
                  project.status === "Active" ||
                  project.status === "Completed" ||
                  project.status === "AwaitingCloseout" ||
                  project.status === "PendingCloseoutApproval" ||
                  project.status === "FinalInvoicePending"
                }
              >
                <Link
                  className="btn btn-primary btn-sm mr-4"
                  to={`/quote/details/${project.id}`}
                >
                  View Quote <FontAwesomeIcon icon={faEye} />
                </Link>
              </ShowFor>
            </div>
          </div>
        </div>
      </div>
      <div className="card-green-bg green-bg-border pt-3 pb-3">
        <div className="card-content">
          <div className="row">
            <div className="col-md-6">
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
            <div className="col-md-6 text-center">
              {" "}
              <ProjectUnbilledButton
                projectId={project.id}
                remaining={project.quoteTotal - project.chargedTotal}
              />
            </div>
          </div>
        </div>
      </div>
      <div>
        {project.status !== "ChangeRequested" && (
          <div className="card-content">
            <RecentTicketsContainer compact={true} projectId={projectId} />

            <RecentInvoicesContainer compact={true} projectId={projectId} />
          </div>
        )}
      </div>
    </div>
  );
};
