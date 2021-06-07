import React from "react";
import { InputElement, useFormState, FormState, StateErrors } from "react-use-form-state";
import { z, ZodObject } from "zod";

interface ValidationMessageProps {
  formState: FormState<any, StateErrors<any, string>>;
  name: string;
}

export const ValidationErrorMessage = (props: ValidationMessageProps) => {
  const { formState, name } = props;
  return formState.touched[name] && formState.validity[name] === false
    ? (<p className="text-danger">{formState.errors[name]}</p>)
    : (null);
}

export const getInputValidityStyle = (formState: FormState<any, StateErrors<any, string>>, name: string) => {
  return formState.touched[name] && (formState.validity[name] === false) && "is-invalid";
}

export function getFieldValidator<T extends ZodObject<any>>(schema: T) {
  return (field: keyof any) => {
    return (value: unknown): string | true => {
      const parsedResult = schema
        .pick({ [field]: true })
        .safeParse({ [field]: value });
      return !parsedResult.success
        ? parsedResult.error.errors[0].message
        : true;
    }
  }
}