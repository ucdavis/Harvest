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

export const ValidationContext = React.createContext<ValidationContextState | null>(
  null
);

export const useOrCreateValidationContext = (
  context?: ValidationContextState | null
) => {
  const [formErrorCount, setFormErrorCount] = useState(0);
  const [formIsTouched, setFormIsTouched] = useState(false);
  const [formIsDirty, setFormIsDirty] = useState(false);
  const [contextIsReset, setcontextIsReset] = useState(false);
  const resetContext = () => {
    setFormErrorCount(0);
    setFormIsTouched(false);
    setFormIsDirty(false);
    setcontextIsReset(true);
  };

  useEffect(() => {
    if ((formErrorCount || formIsTouched || formIsDirty) && contextIsReset) {
      setcontextIsReset(false);
    }
  }, [
    formErrorCount,
    formIsTouched,
    formIsDirty,
    contextIsReset,
    setcontextIsReset,
  ]);

  if (context) {
    // wishing this early return could be earlier, but RULES of HOOKS
    return context;
  }

  const newContext: ValidationContextState = {
    formErrorCount,
    setFormErrorCount,
    formIsTouched,
    setFormIsTouched,
    formIsDirty,
    setFormIsDirty,
    resetContext,
    contextIsReset,
  };

  return newContext;
};

export interface ValidationProviderProps {
  context?: ValidationContextState;
}

export const ValidationProvider: React.FC<ValidationProviderProps> = (
  props
) => {
  const context = useOrCreateValidationContext(props.context);

  return (
    <ValidationContext.Provider value={context}>
      {props.children}
    </ValidationContext.Provider>
  );
};
