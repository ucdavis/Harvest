import React, { useState, useEffect } from "react";

export interface ValidationContextState {
  formErrorCount: number;
  setFormErrorCount: React.Dispatch<React.SetStateAction<number>>;
  formIsTouched: boolean;
  setFormIsTouched: React.Dispatch<React.SetStateAction<boolean>>;
  formIsDirty: boolean;
  setFormIsDirty: React.Dispatch<React.SetStateAction<boolean>>;
  resetContext: () => void;
  // useFormValidator will use an effect triggered on contextIsReset to reset its internal state 
  contextIsReset: boolean;
}

export const ValidationContext = React.createContext<ValidationContextState | null>(null);

export const ValidationProvider: React.FC = ({ children }) => {

  const [formErrorCount, setFormErrorCount] = useState(0);
  const [formIsTouched, setFormIsTouched] = useState(false);
  const [formIsDirty, setFormIsDirty] = useState(false);
  const [contextIsReset, setcontextIsReset] = useState(false);
  const resetContext = () => {
    setFormErrorCount(0);
    setFormIsTouched(false);
    setFormIsDirty(false);
    setcontextIsReset(true);
  }

  useEffect(() => {
    if ((formErrorCount || formIsTouched || formIsDirty) && contextIsReset) {
      setcontextIsReset(false);
    }
  }, [formErrorCount, formIsTouched, formIsDirty, contextIsReset, setcontextIsReset]);

  const contextState: ValidationContextState = {
    formErrorCount,
    setFormErrorCount,
    formIsTouched,
    setFormIsTouched,
    formIsDirty,
    setFormIsDirty,
    resetContext,
    contextIsReset,
  }

  return <ValidationContext.Provider value={contextState}>{children}</ValidationContext.Provider>;
}

