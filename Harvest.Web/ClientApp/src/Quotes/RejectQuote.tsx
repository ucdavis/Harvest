import React, { useState } from "react";

import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from "reactstrap";

interface Props {}

export const RejectQuote = (props: Props) => {
  const [isOpen, setIsOpen] = useState(false);

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
          <Button color="primary" onClick={() => {}}>
            Confirm
          </Button>
        </ModalFooter>
      </Modal>
    </>
  );
};
