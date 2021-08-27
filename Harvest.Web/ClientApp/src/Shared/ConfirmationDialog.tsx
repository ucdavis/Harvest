import React from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter, Button } from "reactstrap";

interface DialogConfig {
  title?: React.ReactNode;
  message?: React.ReactNode;
  actionCallback: ((isConfirmed: boolean) => void);
  canConfirm: boolean;
}

interface DialogContextState {
  openDialog: (config: DialogConfig) => void;
}

const ConfirmationDialogContext = React.createContext({} as DialogContextState);

export const ConfirmationDialogProvider: React.FC = ({ children }) => {
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [dialogConfig, setDialogConfig] = React.useState({} as DialogConfig);

  const openDialog = (config: DialogConfig) => {
    setDialogOpen(true);
    setDialogConfig(config);
  };

  const resetDialog = () => {
    setDialogOpen(false);
    setDialogConfig({} as DialogConfig);
  };

  const onConfirm = () => {
    resetDialog();
    dialogConfig.actionCallback(true);
  };

  const onDismiss = () => {
    resetDialog();
    dialogConfig.actionCallback(false);
  };

  return (
    <ConfirmationDialogContext.Provider value={{ openDialog }}>
      <Modal isOpen={dialogOpen}>
        <ModalHeader>{dialogConfig?.title}</ModalHeader>
        <ModalBody>
          {dialogConfig?.message}
        </ModalBody>
        <ModalFooter>
          <Button
            color="primary"
            onClick={onConfirm}
            enabled={dialogConfig?.canConfirm}
          >
            Confirm
          </Button>{" "}
          <Button color="link" onClick={onDismiss}>
            Cancel
          </Button>
        </ModalFooter>
      </Modal>
      {children}
    </ConfirmationDialogContext.Provider>
  );
};

export const useConfirmationDialog = () => {
  const { openDialog } = React.useContext(ConfirmationDialogContext);

  const getConfirmation = (title: React.ReactNode, message: React.ReactNode, canConfirm: boolean = true) =>
    new Promise<boolean>((res) => {
      openDialog({ actionCallback: res, title, message, canConfirm });
    });

  return { getConfirmation };
};