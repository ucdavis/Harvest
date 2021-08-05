import React, { useState, ChangeEventHandler, ChangeEvent, FocusEvent, useContext, useEffect, useRef } from "react";
import { useDebounceCallback } from '@react-hook/debounce'
import { AnyObjectSchema, ValidationError } from "yup";

import { ValidationContext, ValidationContextState, useValidationContext } from "./ValidationProvider";
import { notEmptyOrFalsey } from "../Util/ValueChecks";

export function useInputValidator<T>(schema: AnyObjectSchema) {
  type TKey = keyof T;

  const defaultContext = useValidationContext();
  const providerContext = useContext(ValidationContext);
  const context = providerContext || defaultContext;

  const { formErrorCount, setFormErrorCount, formIsTouched, setFormIsTouched, formIsDirty,
    setFormIsDirty, resetContext, contextIsReset } = context;

  const [errors, setErrors] = useState({} as Record<TKey, string>);
  const previousErrors = usePrevious(errors) || {} as Record<TKey, string>;
  const [touchedFields, setTouchedFields] = useState([] as TKey[]);
  const [dirtyFields, setDirtyFields] = useState([] as TKey[]);

  useEffect(() => {
    const errorCount = Object.values(errors).filter(notEmptyOrFalsey).length;
    const previousErrorCount = Object.values(previousErrors).filter(notEmptyOrFalsey).length;
    if (errorCount !== previousErrorCount) {
      setFormErrorCount(formErrorCount + errorCount - previousErrorCount);
    }
  }, [errors]);

  useEffect(() => {
    if (contextIsReset) {
      setTouchedFields([]);
      setDirtyFields([]);
      setErrors({} as Record<TKey, string>);
    }
  }, [contextIsReset]);

  const validateField = useDebounceCallback(async (name: TKey, value: T[TKey]) => {
    const tempObject = { [name]: value } as unknown as T;
    try {
      await schema.validateAt(name as string, tempObject);
      if (notEmptyOrFalsey(errors[name])) {
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
    return notEmptyOrFalsey(errors[name])
      ? `${passThroughClassNames} is-invalid`
      : passThroughClassNames;
  }

  const InputErrorMessage = ({ name }: { name: TKey }) => {
    const message = errors[name];

    return notEmptyOrFalsey(errors[name])
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
    if (notEmptyOrFalsey(errors[name])) {
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
    resetContext,
    resetField,
    context
  }
}

// provides previous value of given state
function usePrevious<T>(value: T) {
  const ref = useRef<T>();
  useEffect(() => { ref.current = value; });
  return ref.current;
}

