import { ProjectStatus } from "../types";

export const StatusToActionRequired = (status: ProjectStatus) => {
  switch (status) {
    case "Requested":
    case "ChangeRequested":
    case "QuoteRejected":
      return "Needs Quote";
    case "PendingApproval":
      return "Needs Approval";
    default:
      return "";
  }
};
