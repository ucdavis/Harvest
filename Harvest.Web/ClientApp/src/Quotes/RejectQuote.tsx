import React, { useState } from "react";
import { useHistory } from "react-router-dom";

import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from "reactstrap";
import { Project } from "../types";
import { usePromiseNotification } from "../Util/Notifications";

interface Props {
  project: Project;
}

export const RejectQuote = (props: Props) => {
  const history = useHistory();
  const [isOpen, setIsOpen] = useState(false);
  const [reason, setReason] = useState("");

  const [notification, setNotification] = usePromiseNotification();

  const reject = async () => {
    const request = fetch(`/Request/RejectQuote/${props.project.id}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({}),
    });

    setNotification(request, "Saving", "Quote Rejection Saved");

    const response = await request;

    if (response.ok) {
      history.replace(`/Project/Details/${props.project.id}`);
    }
  };
  return (
    <>
      <button onClick={() => setIsOpen(true)} className="btn btn-link mr-2">
        Reject
      </button>
      <Modal isOpen={isOpen}>
        <ModalHeader>Reject Quote</ModalHeader>
        <ModalBody>
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
        </ModalBody>
        <ModalFooter>
          <Button color="link" onClick={() => setIsOpen(false)}>
            Cancel
          </Button>
          <Button
            color="primary"
            onClick={reject}
            disabled={notification.pending || !reason}
          >
            Confirm
          </Button>
        </ModalFooter>
      </Modal>
    </>
  );
};
