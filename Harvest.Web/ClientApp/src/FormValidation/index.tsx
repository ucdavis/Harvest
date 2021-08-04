import React, { useState, FocusEventHandler, ChangeEventHandler, ChangeEvent, FocusEvent, useContext, useEffect, useRef } from "react";
import { useDebounceCallback } from '@react-hook/debounce'
import { AnyObjectSchema, ValidationError } from "yup";

interface ValidationContextState {
  formErrorCount: number;
  setFormErrorCount: React.Dispatch<React.SetStateAction<number>>;
  formIsTouched: boolean;
  setFormIsTouched: React.Dispatch<React.SetStateAction<boolean>>;
  formIsDirty: boolean;
  setFormIsDirty: React.Dispatch<React.SetStateAction<boolean>>;
  resetForm: () => void;
  // only way I could think of to trigger reset of all hook states
  formIsReset: boolean;
  //setFormIsReset: React.Dispatch<React.SetStateAction<boolean>>;
}

const ValidationContext = React.createContext<ValidationContextState | null>(null);

export const ValidationProvider: React.FC = ({ children }) => {

  const [formErrorCount, setFormErrorCount] = useState(0);
  const [formIsTouched, setFormIsTouched] = useState(false);
  const [formIsDirty, setFormIsDirty] = useState(false);
  const [formIsReset, setFormIsReset] = useState(false);
  const resetForm = () => {
    setFormErrorCount(0);
    setFormIsTouched(false);
    setFormIsDirty(false);
    setFormIsReset(true);
  }

  useEffect(() => {
    if ((formErrorCount || formIsTouched || formIsDirty) && formIsReset) {
      setFormIsReset(false);
    }
  }, [formErrorCount, formIsTouched, formIsDirty, formIsReset, setFormIsReset]);

  const contextState: ValidationContextState = {
    formErrorCount,
    setFormErrorCount,
    formIsTouched,
    setFormIsTouched,
    formIsDirty,
    setFormIsDirty,
    resetForm,
    formIsReset,
  }

  return <ValidationContext.Provider value={contextState}>{children}</ValidationContext.Provider>;
}

export function useInputValidator<T>(schema: AnyObjectSchema) {
  type TKey = keyof T;

  const context = useContext(ValidationContext);
  if (context === null) {
    throw new Error("Form does not appear to be wrapped in a <ValidationProvider>");
  }

  const { formErrorCount, setFormErrorCount, formIsTouched, setFormIsTouched, formIsDirty,
    setFormIsDirty, resetForm, formIsReset } = context;

  const [errors, setErrors] = useState({} as Record<TKey, string>);
  const previousErrors = usePrevious(errors) || {} as Record<TKey, string>;
  const [touchedFields, setTouchedFields] = useState([] as TKey[]);
  const [dirtyFields, setDirtyFields] = useState([] as TKey[]);

  useEffect(() => {
    const errorCount = Object.values(errors).filter(v => (v || "") !== "").length;
    const previousErrorCount = Object.values(previousErrors).filter(v => (v || "") !== "").length;
    if (errorCount !== previousErrorCount) {
      setFormErrorCount(formErrorCount + errorCount - previousErrorCount);
    }
  }, [errors]);

  useEffect(() => {
    if (formIsReset) {
      setTouchedFields([]);
      setDirtyFields([]);
      setErrors({} as Record<TKey, string>);
    }
  }, [formIsReset]);

  const validateField = useDebounceCallback(async (name: TKey, value: T[TKey]) => {
    const tempObject = { [name]: value } as unknown as T;
    try {
      await schema.validateAt(name as string, tempObject);
      if ((errors[name] || "") !== "") {
        setErrors({ ...errors, [name]: "" });
      }
    } catch (e: unknown) {
      if (typeof e === "string") {
        setErrors({ ...errors, [name]: e });
      } else if (e instanceof ValidationError) {
        setErrors({ ...errors, [name]: e.errors.join(", ") });
      }
    }
  }, 250);

  const getClassName = (name: TKey, passThroughClassNames: string = "") => {
    return (errors[name] || "") !== ""
      ? `${passThroughClassNames} is-invalid`
      : passThroughClassNames;
  }

  const InputErrorMessage = ({ name }: { name: TKey }) => {
    const message = errors[name];

    return ((message || "") !== "")
      ? <p className="text-danger">{message}</p>
      : null;
  }

  const valueChanged = (name: TKey, value: T[TKey]) => {
    validateField(name, value);
  };

  const onChange = (name: TKey, handler: ChangeEventHandler<HTMLInputElement> | null = null) => (e: ChangeEvent<HTMLInputElement>) => {
    handler && handler(e);
    // If T[TKey] is a number, this doesn't actually convert the string to a number.
    // But yup doesn't seem to mind, and that's what counts.
    valueChanged(name, e.target.value as unknown as T[TKey]);
    setFormIsDirty(true);
    if (!dirtyFields.some(f => f === name)) {
      setDirtyFields([...dirtyFields, name]);
    }
  };

  const onBlur = (name: TKey) => (e: FocusEvent<HTMLInputElement>) => {
    setFormIsTouched(true);
    if (!touchedFields.some(f => f === name)) {
      setTouchedFields([...touchedFields, name]);
    }
    validateField(name, e.target.value as unknown as T[TKey]);
  };

  const fieldIsTouched = (name: TKey) => touchedFields.some(f => f === name);
  const fieldIsDirty = (name: TKey) => dirtyFields.some(f => f === name);
  const resetField = (name: TKey) => {
    setTouchedFields(touchedFields.filter(f => f === name));
    setDirtyFields(dirtyFields.filter(f => f === name));
    if ((errors[name] || "") !== "") {
      setErrors({ ...errors, [name]: "" });
    }
  };

  return {
    valueChanged,
    onChange,
    onBlur,
    InputErrorMessage,
    getClassName,
    formErrorCount,
    formIsTouched,
    fieldIsTouched,
    formIsDirty,
    fieldIsDirty,
    resetForm,
    resetField
  }
}

// provides previous value of given state
function usePrevious<T>(value: T) {
  const ref = useRef<T>();
  useEffect(() => { ref.current = value; });
  return ref.current;
}

