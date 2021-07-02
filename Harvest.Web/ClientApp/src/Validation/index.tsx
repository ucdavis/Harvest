import React, { useMemo, useCallback } from "react";
import { FieldError, useFormState, useFormContext } from "react-hook-form"
import { ErrorMessage } from "@hookform/error-message";
import get from "lodash/get";

// EXAMPLE: path of root.values.activities[0].workItems[1] will be "activities.0.workitems.1"
export const getObjectPath = (from: object, to: object) => {
  return traverse(from).join(".");

  function traverse(traverseFrom: object, path: string[] = []): string[] {
    for (const [key, value] of Object.entries(traverseFrom).filter(e => !!e[1] && typeof e[1] === "object")) {
      if (value === to) {
        return [...path, key];
      }
      const newPath = traverse(value, [...path, key]);
      if (newPath.length > 0) {
        return newPath;
      }
    }
    return [];
  }
}

interface ValidationMessageProps {
  name: string;
}

export function ValidationErrorMessage(props: ValidationMessageProps) {
  const { name } = props;

  return <ErrorMessage
    name={name}
    render={({ message }) => <p className="text-danger">{message}</p>} />;
}

export function useFormHelpers(path: string) {
  const { control } = useFormContext();

  const { errors } = useFormState({ control });

  const getPath = useCallback((name: string) => (path || "") === "" ? name : `${path}.${name}`, [path]);

  const getInputValidityStyle = (name: string) => {
    return get(errors, getPath(name)) && "is-invalid";
  }

  return {
    getInputValidityStyle,
    getPath,
    getObjectPath
  }
}
