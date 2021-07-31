import React, { useState, ChangeEventHandler, ChangeEvent } from "react";
import { useDebounceCallback } from '@react-hook/debounce'
import { AnyObjectSchema, ValidationError } from "yup";

export function useInputValidator<T>(schema: AnyObjectSchema) {
  type TKey = keyof T;

  const [errors, setErrors] = useState({} as Record<TKey, string>);

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
  };

  const onBlur = (name: TKey) => (e: HTMLInputElement) => {
    // TODO: Add some basic functions for managing touched and dirty state
  };

  return {
    valueChanged,
    onChange,
    onBlur,
    InputErrorMessage,
    getClassName
  }
}

