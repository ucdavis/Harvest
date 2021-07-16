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

interface RouteParams {
  projectId?: string;
}

export const ProjectDetailContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [newFiles, setNewFiles] = useState<BlobFile[]>([]);

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

  const updateFiles = async (attachments: BlobFile[]) => {
    await fetch(`/Request/Files/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ Attachments: attachments }),
    });
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
            <div className="col">
              {project.status === "Requested" && (
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/quote/create/${project.id}`}
                >
                  Edit Quote
                </Link>
              )}

              {project.status === "PendingApproval" && (
                <Link
                  className="btn btn-primary btn-small mr-4"
                  to={`/request/approve/${project.id}`}
                >
                  Approve Quote
                </Link>
              )}
              { project.status === "Active" && (
              <Link
                className="btn btn-primary btn-small mr-4"
                to={`/request/changeAccount/${project.id}`}
              >
                Change Accounts
              </Link>
              )}
            </div>
            <div className="col text-right">
              <ProjectUnbilledButton projectId={project.id} />
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
                <FileUpload
                  files={newFiles}
                  setFiles={(f) => {
                    let files = f.slice(newFiles.length);
                    if (newFiles.length === 0) {
                      files = f;
                    }

                    setProject({
                      ...project,
                      attachments: [...project.attachments, ...files],
                    });
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

                    setProject((proj) => {
                      if (proj) {
                        proj.attachments[
                          proj.attachments.findIndex(
                            (file) => file.identifier === f.identifier
                          )
                        ] = { ...f };

                        return { ...proj, attachments: [...proj.attachments] };
                      }
                    });
                  }}
                />
                <ul className="no-list-style attached-files-list">
                  {project.attachments.map((attachment, i) => (
                    <li key={`attachment-${i}`}>
                      {/* TODO: Add a way to download files from Azure */}
                      <a href="#">
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
      <div className="row">
        <div className="col">
          <RecentInvoicesContainer compact={true} projectId={projectId} />
        </div>
        <div className="col">
          <RecentTicketsContainer compact={true} projectId={projectId} />
        </div>
      </div>
    </div>
  );
};
