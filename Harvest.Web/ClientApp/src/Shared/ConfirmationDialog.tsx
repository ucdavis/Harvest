import React from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter, Button } from "reactstrap";

interface DialogProps {
  title?: React.ReactNode;
  message?: React.ReactNode;
  open: boolean;
  onConfirm: () => void;
  onDismiss: () => void;
}

interface DialogConfig {
  title?: React.ReactNode;
  message?: React.ReactNode;
  actionCallback: ((isConfirmed: boolean) => void);
}

interface DialogContextState {
  openDialog: (config: DialogConfig) => void;
}

const ConfirmationDialog = ({ open, title, message, onConfirm, onDismiss }: DialogProps) => {
  return (
    <Modal isOpen={open}>
      <ModalHeader>{title}</ModalHeader>
      <ModalBody>
        {message}
      </ModalBody>
      <ModalFooter>
        <Button
          color="primary"
          onClick={onConfirm}
        >
          Confirm
        </Button>{" "}
        <Button color="link" onClick={onDismiss}>
          Cancel
        </Button>
      </ModalFooter>
    </Modal>
  );
};

const ConfirmationDialogContext = React.createContext({} as DialogContextState);

export const ConfirmationDialogProvider: React.FC = ({ children }) => {
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [dialogConfig, setDialogConfig] = React.useState({} as DialogConfig);

  const openDialog = ({ title, message, actionCallback }: DialogConfig) => {
    setDialogOpen(true);
    setDialogConfig({ title, message, actionCallback });
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
      <ConfirmationDialog
        open={dialogOpen}
        title={dialogConfig?.title}
        message={dialogConfig?.message}
        onConfirm={onConfirm}
        onDismiss={onDismiss}
      />
      {children}
    </ConfirmationDialogContext.Provider>
  );
};

export const useConfirmationDialog = () => {
  const { openDialog } = React.useContext(ConfirmationDialogContext);

  const getConfirmation = (title: React.ReactNode, message: React.ReactNode) =>
    new Promise<boolean>((res) => {
      openDialog({ actionCallback: res, title, message });
    });

  return { getConfirmation };
};