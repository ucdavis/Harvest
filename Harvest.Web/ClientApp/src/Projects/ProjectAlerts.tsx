import { useMemo } from "react";

import { Project } from "../types";
import { convertCamelCase } from "../Util/StringFormatting";

interface Props {
  project: Project;
}

export const ProjectAlerts = (props: Props) => {
  const { project } = props;

  const statusDetail = useMemo(() => {
    return getStatusDetail(project);
  }, [project]);

  if (!statusDetail.showAlert) {
    return null;
  }

  return (
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
        linkTo: `/quote/create/${project.id}`,
      };
    case "PendingApproval":
      return {
        showAlert: true,
        cardClass: "sunflower-bg",
        statusText:
          "This project has a quote waiting for your approval. Use the 'view quote' button to view it.",
        actionText: "View Quote",
        linkClass: "btn-sunflower",
        linkTo: `/request/approve/${project.id}`,
      };
    case "QuoteRejected":
      return {
        showAlert: true,
        cardClass: "merlot-bg",
        statusText:
          "The PI has rejected the quote and is waiting for an updated quote. Use the 'edit quote' button to create one.",
        actionText: "Create Quote",
        linkClass: "btn-danger",
        linkTo: `/quote/create/${project.id}`,
      };
    default:
      return {
        showAlert: false,
      } as StatusDetail;
  }
};