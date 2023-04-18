import { useMemo } from "react";

import { Project } from "../types";
import { convertCamelCase } from "../Util/StringFormatting";

interface Props {
  project: Project;
  extraText?: string;
}

export const ProjectAlerts = (props: Props) => {
  const { project } = props;
  const { extraText } = props;

  const statusDetail = useMemo(() => {
    return getStatusDetail(project);
  }, [project]);

  if (!statusDetail.showAlert) {
    return null;
  }

  return (
    <div>
      <div className={`card-project-status ${statusDetail.cardClass}`}>
        <div className="card-content">
          <h4>Current Status: {convertCamelCase(project.status)}</h4>
          <p>{statusDetail.statusText} </p>
          {/* <Link
          to={statusDetail.linkTo}
          className={`btn ${statusDetail.linkClass} btn-sm`}
        >
          {statusDetail.actionText}
        </Link> */}
        </div>
      </div>
      {extraText && (
        <div className={`card-project-status merlot-bg`}>
          <div className="card-content">
            <p>{extraText} </p>
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

const getStatusDetail = (project: Project): StatusDetail => {
  const team = project.team.slug;

  // switch return different classname based on status
  switch (project.status) {
    case "Requested":
    case "ChangeRequested":
      return {
        showAlert: true,
        cardClass: "california-bg",
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
