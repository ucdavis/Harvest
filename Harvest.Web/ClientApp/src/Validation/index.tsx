import React, { useMemo, useCallback } from "react";
import {
  useFormState, useFormContext, useWatch, FieldValues, UseFormReturn, UseFormWatch, UseFormGetValues, UseFormSetError,
  UseFormClearErrors, UseFormSetValue, UseFormTrigger, FormState, UseFormReset, UseFormHandleSubmit, UseFormUnregister,
  Control, UseFormRegister, UseFormSetFocus, FieldPath, WatchObserver, UnpackNestedValue, EventType, UseWatchProps,
  DeepPartial, FieldPathValue, FieldPathValues
} from "react-hook-form"
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

export function getFilteredItemsWithOriginalIndex<T>(items: T[], filter: (item: T) => boolean) {
  return items
    .map((item, i) => ({ item, i }))
    .filter((itemAndIndex) => filter(itemAndIndex.item));
}

export function usePropValuesFromArray<TObj = object, TElem = any>(arrayPropPath: string, nestedPropName: string, filter: (item: TObj) => boolean = (_) => true) {
  const { control, getValues } = useFormContext();
  const items = (getValues(arrayPropPath as "") || []) as TObj[];
  const paths = getFilteredItemsWithOriginalIndex(items, filter)
    .map((item) => `${arrayPropPath}.${item.i}.${nestedPropName}`);
  const values = useWatch({ control, name: paths as ""[], defaultValue: [] }) as TElem[];
  return [...values];
}

export const isString = (value: any): value is string => typeof value === 'string';
export const isStringArray = (value: any): value is string[] => Array.isArray(value) && (value.length === 0 || typeof value[0] === "string");
export const isFunction = (value: unknown): value is Function => typeof value === 'function';

const getNameTransformer = <TFieldValues extends FieldValues>(path: string | undefined) => {
  const namePrefix = (path || "") === "" ? "" : `${path}.`;
  return (fieldName: FieldPath<TFieldValues> | ReadonlyArray<FieldPath<TFieldValues>> | WatchObserver<TFieldValues> | undefined) => {
    if (isString(fieldName)) {
      return namePrefix + fieldName;
    }
    if (isStringArray(fieldName)) {
      return fieldName.map(n => namePrefix + n);
    }
    if (isFunction(fieldName)) {
      return ((value: UnpackNestedValue<TFieldValues>, info: { name?: string; type?: EventType; value?: unknown; }) => {
        return fieldName(value, { name: namePrefix + info.name, type: info.type, value: info.value });
      }) as WatchObserver<Record<string, any>>;
    }
    return undefined;
  };
}


export type UseNestedWatchProps<TFieldValues> = Omit<UseWatchProps<TFieldValues>, "control"> & { control?: Control<Record<string, any>>; path: string }
export function useNestedWatch<
  TFieldValues extends FieldValues = FieldValues,
  >(props: {
    defaultValue?: UnpackNestedValue<DeepPartial<TFieldValues>>;
    control?: Control<Record<string, any>>;
    path: string;
  }): UnpackNestedValue<DeepPartial<TFieldValues>>;
export function useNestedWatch<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
  >(props: {
    name: TName;
    defaultValue?: FieldPathValue<TFieldValues, TName>;
    control?: Control<Record<string, any>>;
    path: string;
  }): FieldPathValue<TFieldValues, TName>;
export function useNestedWatch<
  TFieldValues extends FieldValues = FieldValues,
  TName extends readonly FieldPath<TFieldValues>[] = FieldPath<TFieldValues>[],
  >(props: {
    name: TName;
    defaultValue?: UnpackNestedValue<DeepPartial<TFieldValues>>;
    control?: Control<Record<string, any>>;
    path: string;
  }): FieldPathValues<TFieldValues, TName>;
export function useNestedWatch<TFieldValues>(props?: UseNestedWatchProps<TFieldValues>) {
  const { control, name: origName, defaultValue, path } = props || {};
  const transformFieldName = useCallback(getNameTransformer<TFieldValues>(path), [path]);
  const name = transformFieldName(origName) as ""
  return useWatch({ control, name, defaultValue });
}

export type UseNestedFormReturn<TFieldValues extends FieldValues = FieldValues> =
  Omit<UseFormReturn<TFieldValues>, "formState" | "handleSubmit" | "reset" | "control">
  & Pick<UseFormReturn<Record<string, any>>, "control">

export const useNestedFormContext = <TFieldValues extends FieldValues>(path: string = ""): UseNestedFormReturn<TFieldValues> => {
  const rootContext = useFormContext();

  const transformFieldName = useCallback(getNameTransformer<TFieldValues>(path), [path]);

  const watch: UseFormWatch<TFieldValues> = useCallback((
    fieldName?: FieldPath<TFieldValues> | ReadonlyArray<FieldPath<TFieldValues>> | WatchObserver<TFieldValues>,
    defaultValue?: unknown,
  ) => {
    const fullPath = transformFieldName(fieldName) as "";
    // there are many watch overloads, but they all go to the same 2-arg function, so just cast to a valid "" argument
    return rootContext.watch(fullPath, defaultValue);
  }, [transformFieldName]);

  const unregister: UseFormUnregister<TFieldValues> = useCallback((name, options = {}) => {
    const fullPath = transformFieldName(name) as "";
    return rootContext.unregister(fullPath)
  }, [transformFieldName]);

  const register: UseFormRegister<TFieldValues> = useCallback(<TFieldName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>>(name: TFieldName, options = {}) => {
    const fullPath = transformFieldName(name) as "";
    return rootContext.register(fullPath, options)
  }, [transformFieldName]);

  const setFocus: UseFormSetFocus<TFieldValues> = useCallback((name) => {
    const fullPath = transformFieldName(name) as "";
    return rootContext.setFocus(fullPath)
  }, [transformFieldName]);

  const trigger: UseFormTrigger<TFieldValues> = useCallback(async (name, options = {}) => {
    const fullPath = transformFieldName(name) as "";
    return await rootContext.trigger(fullPath, options)
  }, [transformFieldName]);

  const setValue: UseFormSetValue<TFieldValues> = useCallback((name, value, options = {}) => {
    const fullPath = transformFieldName(name) as "";
    return rootContext.setValue(fullPath, value, options)
  }, [transformFieldName]);

  const getValues: UseFormGetValues<TFieldValues> = useCallback((fieldNames?: | FieldPath<TFieldValues> | ReadonlyArray<FieldPath<TFieldValues>>,) => {
    const fullPath = transformFieldName(fieldNames) as "";
    return rootContext.getValues(fullPath)
  }, [transformFieldName]);

  const clearErrors: UseFormClearErrors<TFieldValues> = useCallback((fieldNames) => {
    const fullPath = transformFieldName(fieldNames) as "";
    return rootContext.clearErrors(fullPath)
  }, [transformFieldName]);

  const setError: UseFormSetError<TFieldValues> = useCallback((name, value, options) => {
    const fullPath = transformFieldName(name) as "";
    return rootContext.setError(fullPath, value, options)
  }, [transformFieldName]);

  return {
    control: rootContext.control,
    //formstate
    trigger,
    register,
    //handleSubmit
    watch,
    setValue,
    getValues,
    //reset,
    clearErrors,
    unregister,
    setError,
    setFocus
  };// as UseFormReturn<TFieldValues>;

};


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
