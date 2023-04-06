import React, { useState } from "react";
import { useHistory, useParams } from "react-router-dom";

import { CommonRouteParams, Project } from "../types";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { notEmptyOrFalsey } from "../Util/ValueChecks";

interface Props {
  project: Project;
}

export const RejectQuote = (props: Props) => {
  const history = useHistory();
  const { team } = useParams<CommonRouteParams>();
  const [reason, setReason] = useState("");

  const [notification, setNotification] = usePromiseNotification();

  const [getConfirmation] = useConfirmationDialog<string>(
    {
      title: "RejectQuote",
      message: (setReturn) => (
        <div className="form-group">
          <label htmlFor="fieldName">Reason</label>
          <textarea
            className="form-control"
            id="fieldName"
            rows={3}
            required
            value={reason}
            onChange={(e) => {
              setReason(e.target.value);
              setReturn(e.target.value);
            }}
          />
          <small id="fieldNameHelp" className="form-text text-muted">
            Let us know what issues you have with this quote.
          </small>
        </div>
      ),
      canConfirm: notEmptyOrFalsey(reason) && !notification.pending,
    },
    [reason, setReason, notification.pending]
  );

  const reject = async () => {
    const [confirmed, reason] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    const request = authenticatedFetch(
      `/api/Request/RejectQuote/${props.project.id}`,
      {
        method: "POST",
        body: JSON.stringify({ reason }),
      }
    );

    setNotification(request, "Saving", "Quote Rejection Saved");

    const response = await request;

    if (response.ok) {
      history.replace(`/${team}/project/details/${props.project.id}`);
    }
  };
  return (
    <button onClick={reject} className="btn btn-link mr-2">
      Reject
    </button>
  );
};
