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
import { BlobFile, CommonRouteParams, Project } from "../types";
import { ShowFor, useFor } from "../Shared/ShowFor";
import { ShowForPiOnly, useForPiOnly } from "../Shared/ShowForPiOnly";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";
import { useHistory } from "react-router-dom";
import { ProjectAlerts } from "./ProjectAlerts";
import { authenticatedFetch } from "../Util/Api";
import { addDays } from "../Util/Calculations";
import { getDaysDiff } from "../Util/Calculations";

export const ProjectDetailContainer = () => {
  const { projectId } = useParams<CommonRouteParams>();
  const [project, setProject] = useState<Project>({} as Project);
  const [isLoading, setIsLoading] = useState(true);
  const [newFiles, setNewFiles] = useState<BlobFile[]>([]);
  const history = useHistory();

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      setIsLoading(true);
      const response = await authenticatedFetch(
        `/api/Project/Get/${projectId}`
      );
      if (response.ok) {
        const project = (await response.json()) as Project;

        getIsMounted() && setProject(project);
        setIsLoading(false);
      } else {
        setNotification(response, "Loading", "Error Loading Project");
        if (response.status === 403) {
          window.location.replace("/Account/AccessDenied");
        }
        //history.push("/"); //If we redirect to the home page, we will have to fix the tests
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted]);

  const team = project.team?.slug;

  const updateFiles = async (attachments: BlobFile[]) => {
    const request = authenticatedFetch(`/api/Request/Files/${projectId}`, {
      method: "POST",
      body: JSON.stringify({ Attachments: attachments }),
    });

    setNotification(request, "Saving File(s)", "File(s) Saved");

    const response = await request;

    if (response.ok && project) {
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
    const request = authenticatedFetch(`/api/Request/Cancel/${projectId}`, {
      method: "POST",
    });
    setNotification(request, "Canceling", "Project Request Canceled");
    const response = await request;
    if (response.ok) {
      //redirect to home
      history.push("/");
    }
  };

  const projectActions = [
    useFor({
      roles: ["Supervisor", "FieldManager"],
      condition:
        project.status === "Active" ||
        project.status === "AwaitingCloseout" ||
        project.status === "PendingCloseoutApproval",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/expense/entry/${project.id}`}
        >
          Enter Expenses <FontAwesomeIcon icon={faEdit} />
        </Link>
      ),
    }),
    useFor({
      roles: ["FieldManager", "Supervisor"],
      condition:
        project.status === "Requested" ||
        project.status === "ChangeRequested" ||
        project.status === "QuoteRejected",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/quote/create/${project.id}`}
        >
          Edit Quote <FontAwesomeIcon icon={faEdit} />
        </Link>
      ),
    }),
    useFor({
      roles: ["FieldManager"],
      condition:
        project.status === "Requested" ||
        project.status === "ChangeRequested" ||
        project.status === "QuoteRejected",
      children: (
        <button
          className="btn btn-danger btn-sm float-right"
          onClick={() => cancelProject()}
        >
          Cancel Request <FontAwesomeIcon icon={faTimes} />
        </button>
      ),
    }),
    useFor({
      roles: ["FieldManager"],
      condition:
        project.status === "AwaitingCloseout" || project.status === "Active",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/project/closeout/${project.id}`}
        >
          Close Out Project <FontAwesomeIcon icon={faCheck} />
        </Link>
      ),
    }),
    useForPiOnly({
      project: project,
      condition: project.status === "PendingCloseoutApproval",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/project/closeoutconfirmation/${project.id}`}
        >
          Confirm Close Out
        </Link>
      ),
    }),
    useForPiOnly({
      project: project,
      condition: project.status === "PendingApproval",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/request/approve/${project.id}`}
        >
          View Quote <FontAwesomeIcon icon={faEye} />
        </Link>
      ),
    }),
    useForPiOnly({
      project: project,
      condition: project.status === "Active",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/request/changeAccount/${project.id}`}
        >
          Change Accounts <FontAwesomeIcon icon={faExchangeAlt} />
        </Link>
      ),
    }),
    useFor({
      roles: ["System"],
      condition:
        project.status === "PendingApproval" &&
        addDays(new Date(project.lastStatusUpdatedOn), 18) <= new Date(),
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/request/approve/${project.id}`}
        >
          System View Quote <FontAwesomeIcon icon={faEye} />
        </Link>
      ),
    }),
    useFor({
      roles: ["System"],
      condition: project.status === "Active",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/request/changeAccount/${project.id}`}
        >
          System Change Accounts <FontAwesomeIcon icon={faExchangeAlt} />
        </Link>
      ),
    }),
    useFor({
      roles: ["PI", "FieldManager"],
      condition: project.status === "Active",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/request/create/${project.id}`}
        >
          Change Requirements <FontAwesomeIcon icon={faExchangeAlt} />
        </Link>
      ),
    }),
    useFor({
      roles: ["PI", "FieldManager"],
      condition:
        // all statuses with approved quotes
        project.status === "Active" ||
        project.status === "Completed" ||
        project.status === "AwaitingCloseout" ||
        project.status === "PendingCloseoutApproval" ||
        project.status === "FinalInvoicePending",
      children: (
        <Link
          className="btn btn-primary btn-sm mr-4"
          to={`/${team}/quote/details/${project.id}`}
        >
          View Quote <FontAwesomeIcon icon={faEye} />
        </Link>
      ),
    }),
  ].filter((a) => a !== null);

  if (isLoading) {
    return <div>Loading...</div>;
  }

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
      <ShowFor
        roles={["System"]}
        condition={project.status === "PendingApproval"}
      >
        <ProjectAlerts
          project={project}
          extraText={`Project has not been acted on since ${new Date(
            project.lastStatusUpdatedOn
          ).toLocaleDateString()}. ${getDaysDiff(
            new Date(),
            new Date(project.lastStatusUpdatedOn)
          ).toFixed(0)} days.`}
        />
      </ShowFor>
      {projectActions.length > 0 && (
        <div className="card-green-bg">
          <div className="card-content">
            <div className="row justify-content-between">
              <div className="col-md-12 project-actions">
                <h4>Project actions</h4>
                {projectActions.map((action, i) => ({
                  ...action,
                  key: `action_${i}`,
                }))}
              </div>
            </div>
          </div>
        </div>
      )}
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
            <div className="col-md-6 text-right">
              {" "}
              <ProjectUnbilledButton
                projectId={project.id}
                team={team}
                remaining={project.quoteTotal - project.chargedTotal}
              />
            </div>
          </div>
        </div>
      </div>
      <div>
        {project.status !== "ChangeRequested" && (
          <div className="card-content">
            <ShowFor roles={["PI", "FieldManager", "Supervisor", "System"]}>
              <RecentTicketsContainer
                compact={true}
                projectId={projectId}
                team={team}
              />
            </ShowFor>
            <RecentInvoicesContainer
              compact={true}
              projectId={projectId}
              team={team}
            />
          </div>
        )}
      </div>
    </div>
  );
};
