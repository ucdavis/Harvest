import React, { useEffect, useState } from "react";

import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from "reactstrap";

import { Field } from "../types";

interface Props {
  field: Field;
  saveFieldChanges: (field: Field) => void;
}

export const EditField = (props: Props) => {
  const [isOpen, setIsOpen] = useState(true);

  // every time the field changes, re-open the modal
  useEffect(() => {
    setIsOpen(true);
  }, [props.field]);

  const update = () => {
    if (props.field) {
      //   props.saveFieldChanges({ ...props.field });
      setIsOpen(false);
    }
  };

  return (
    <div>
      <Modal isOpen={isOpen}>
        <ModalHeader>Modal title</ModalHeader>
        <ModalBody>
          Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
          eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad
          minim veniam, quis nostrud exercitation ullamco laboris nisi ut
          aliquip ex ea commodo consequat. Duis aute irure dolor in
          reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla
          pariatur. Excepteur sint occaecat cupidatat non proident, sunt in
          culpa qui officia deserunt mollit anim id est laborum.
        </ModalBody>
        <ModalFooter>
          <Button color="primary" onClick={update}>
            Do Something
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};
