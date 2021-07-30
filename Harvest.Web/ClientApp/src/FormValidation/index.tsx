import React, { useState } from "react";
import { useDebounceCallback } from '@react-hook/debounce'
import { AnyObjectSchema, ValidationError } from "yup";

export function useInputValidator<TValues>(schema: AnyObjectSchema) {
  type TKey = keyof TValues;

  const [errors, setErrors] = useState({} as Record<TKey, string>);

  const validateField = useDebounceCallback(async (name: TKey, value: TValues[TKey]) => {
    const valuesWithSingleProp = { [name]: value } as unknown as TValues;
    try {
      await schema.validateAt(name as string, valuesWithSingleProp);
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

  const valueChanged = (name: TKey, value: TValues[TKey]) => {
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

