import { useMemo } from "react";

import { CommonRouteParams, Project } from "../types";
import { convertCamelCase } from "../Util/StringFormatting";
import { Link, useParams } from "react-router-dom";

interface Props {
  project: Project;
  extraText?: string;
  linkId?: number; // Optional link ID for specific actions
  linkText?: string; // Optional text for the link
  skipStatusCheck?: boolean; // Optional flag to skip status check
}

export const ProjectAlerts = (props: Props) => {
  const { project } = props;
  const { extraText } = props;
  const { linkId, linkText } = props;
  const { team } = useParams<CommonRouteParams>();
  const { skipStatusCheck } = props;

  const statusDetail = useMemo(() => {
    return getStatusDetail(project, skipStatusCheck);
  }, [project, skipStatusCheck]);

  if (!statusDetail.showAlert && !extraText) {
    return null;
  }

  return (
    <div>
      {extraText && (
        <div className={`card-project-status merlot-bg`}>
          <div className="card-content">
            <p>{extraText} </p>
            {linkId && (
              <Link to={`/${team}/project/details/${linkId}`}>
                {linkText || `Go To Project ${linkId}`}
              </Link>
            )}
          </div>
        </div>
      )}
      {statusDetail.showAlert && (
        <div className={`card-project-status ${statusDetail.cardClass}`}>
          <div className="card-content">
            <h3>Current Status: {convertCamelCase(project.status)}</h3>
            <p>{statusDetail.statusText} </p>
            {/* <Link
          to={statusDetail.linkTo}
          className={`btn ${statusDetail.linkClass} btn-sm`}
        >
          {statusDetail.actionText}
        </Link> */}
          </div>
        </div>
      )}
    </div>
  );
};

interface StatusDetail {
  showAlert: boolean;
  cardClass: string;
  statusText: string;
  linkClass: string;
  linkTo: string;
  actionText: string;
}

const getStatusDetail = (
  project: Project,
  skipStatusCheck?: boolean
): StatusDetail => {
  const team = project.team.slug;

  if (skipStatusCheck) {
    return {
      showAlert: false,
    } as StatusDetail;
  }

  // switch return different classname based on status
  switch (project.status) {
    case "Requested":
    case "ChangeRequested":
      return {
        showAlert: true,
        cardClass: "sunflower-bg",
        statusText:
          "This project is waiting for a quote. Use the 'edit quote' button to create one.",
        actionText: "Create Quote",
        linkClass: "btn-secondary",
        linkTo: `/${team}/quote/create/${project.id}`,
      };
    case "PendingApproval":
      return {
        showAlert: true,
        cardClass: "sunflower-bg",
        statusText:
          "This project has a quote waiting for your approval. Use the 'view quote' button to view it.",
        actionText: "View Quote",
        linkClass: "btn-sunflower",
        linkTo: `/${team}/request/approve/${project.id}`,
      };
    case "QuoteRejected":
      return {
        showAlert: true,
        cardClass: "merlot-bg",
        statusText:
          "The PI has rejected the quote and is waiting for an updated quote. Use the 'edit quote' button to create one.",
        actionText: "Create Quote",
        linkClass: "btn-danger",
        linkTo: `/${team}/quote/create/${project.id}`,
      };
    default:
      return {
        showAlert: false,
      } as StatusDetail;
  }
};
