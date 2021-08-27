import React, { useState, useEffect } from "react";
import { useHistory } from "react-router-dom";

import { Project } from "../types";
import { usePromiseNotification } from "../Util/Notifications";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import { notEmptyOrFalsey } from "../Util/ValueChecks";

interface Props {
  project: Project;
}

export const RejectQuote = (props: Props) => {
  const history = useHistory();
  const [reason, setReason] = useState("");

  const [notification, setNotification] = usePromiseNotification();

  const [getConfirmation] = useConfirmationDialog({
    title: "RejectQuote",
    message: (
      <div className="form-group">
        <label htmlFor="fieldName">Reason</label>
        <textarea
          className="form-control"
          id="fieldName"
          rows={3}
          required
          value={reason}
          onChange={(e) => setReason(e.target.value)}
        />
        <small id="fieldNameHelp" className="form-text text-muted">
          Let us know what issues you have with this quote.
        </small>
      </div>
    ),
    canConfirm: notEmptyOrFalsey(reason) && !notification.pending
  }, [reason, setReason, notification.pending]);

  const reject = async () => {
    if (!await getConfirmation()) {
      return;
    }

    const request = fetch(`/Request/RejectQuote/${props.project.id}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ reason }),
    });

    setNotification(request, "Saving", "Quote Rejection Saved");

    const response = await request;

    if (response.ok) {
      history.replace(`/project/details/${props.project.id}`);
    }
  };
  return (
    <button onClick={reject} className="btn btn-link mr-2">
      Reject
    </button>
  );
};
