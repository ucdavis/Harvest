import React, { ReactNode, useRef } from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter, Button } from "reactstrap";
import { useModal } from "react-modal-hook";

interface Props {
  title: ReactNode;
  message: ReactNode;
  canConfirm?: boolean;
}

export const useConfirmationDialog = (props: Props, dependencies: any[] = []) => {
  const promiseRef = useRef<Promise<boolean>>();
  const resolveRef = useRef<(value: boolean) => void>();

  const confirm = () => {
    resolveRef.current && resolveRef.current(true);
    promiseRef.current = undefined;
    resolveRef.current = undefined;
  };

  const dismiss = () => {
    resolveRef.current && resolveRef.current(false);
    promiseRef.current = undefined;
    resolveRef.current = undefined;
  }

  const [showModal, hideModal] = useModal(() => (
    <Modal isOpen={true}>
      <ModalHeader>{props.title}</ModalHeader>
      <ModalBody>
        {props.message}
      </ModalBody>
      <ModalFooter>
        <Button
          color="primary"
          onClick={() => { confirm(); hideModal(); }}
          disabled={props.canConfirm === undefined ? false : !props.canConfirm}
        >
          Confirm
        </Button>{" "}
        <Button color="link" onClick={() => { dismiss(); hideModal(); }}>
          Cancel
        </Button>
      </ModalFooter>
    </Modal>
  ), dependencies);


  const getConfirmation = () => {
    let promise = promiseRef.current || new Promise<boolean>((resolve) => {
      resolveRef.current = resolve;
      showModal();
    });
    if (promiseRef.current === undefined) {
      promiseRef.current = promise;
    }
    return promise;
  };

  return [getConfirmation];
};