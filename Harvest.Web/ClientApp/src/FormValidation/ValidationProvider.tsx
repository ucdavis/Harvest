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

export const useValidationContext = () => {

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

  const context: ValidationContextState = {
    formErrorCount,
    setFormErrorCount,
    formIsTouched,
    setFormIsTouched,
    formIsDirty,
    setFormIsDirty,
    resetContext,
    contextIsReset,
  }

  return context;
}

export interface ValidationProviderProps {
  context?: ValidationContextState
}

export const ValidationProvider: React.FC<ValidationProviderProps> = (props) => {
  const defaultContext = useValidationContext();

  return <ValidationContext.Provider value={props.context || defaultContext}>{props.children}</ValidationContext.Provider>;
}

