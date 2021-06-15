import React from "react";
import { FieldError } from "react-hook-form"

interface ValidationMessageProps<T> {
  error: FieldError | undefined;
}


export function ValidationErrorMessage<T>(props: ValidationMessageProps<T>) {
  const { error } = props;

  return error
    ? (<p className="text-danger">{error.message}</p>)
    : (null);
}

export function getInputValidityStyle<T>(error: FieldError | undefined) {
  return error && "is-invalid";
}

