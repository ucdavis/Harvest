import React, {
  useState,
  Dispatch,
  MutableRefObject,
  SetStateAction,
  createContext,
  useRef,
} from "react";
import { ValidationError } from "yup";

export interface ValidationContextState {
  formErrorCount: number;
  setFormErrorCount: Dispatch<SetStateAction<number>>;
  formIsTouched: boolean;
  setFormIsTouched: Dispatch<SetStateAction<boolean>>;
  formIsDirty: boolean;
  setFormIsDirty: Dispatch<SetStateAction<boolean>>;
  callbacks: MutableRefObject<ValidatorCallbacks>[];
}

export interface ValidatorCallbacks {
  reset: () => void;
  validate: () => Promise<ValidationError[]>;
}

export const ValidationContext = createContext<ValidationContextState | null>(
  null
);

export const useOrCreateValidationContext = (
  context?: ValidationContextState | null
) => {
  const [formErrorCount, setFormErrorCount] = useState(0);
  const [formIsTouched, setFormIsTouched] = useState(false);
  const [formIsDirty, setFormIsDirty] = useState(false);

  // a ref of array of refs
  // this is the only way to ensure the array does not get replaced on rerenders
  const callbacksRef = useRef<MutableRefObject<ValidatorCallbacks>[]>([]);

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
    callbacks: callbacksRef.current,
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
