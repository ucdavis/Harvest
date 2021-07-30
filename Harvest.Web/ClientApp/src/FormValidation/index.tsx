import React, { useState } from "react";
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

  const InputErrorMessage = ({ name }: { name: TKey }) => {
    const message = errors[name];

    return ((message || "") !== "")
      ? <p className="text-danger">{message}</p>
      : null;
  }

  const valueChanged = (name: TKey, value: T[TKey]) => {
    validateField(name, value);
  };

  const onBlur = (name: TKey) => (e: HTMLInputElement) => {
    // TODO: Add some basic functions for managing touched and dirty state
  };

  return {
    valueChanged,
    onBlur,
    InputErrorMessage
  }
}

