import React, {
  useState,
  ChangeEventHandler,
  ChangeEvent,
  FocusEvent,
  useContext,
  useEffect,
  useRef,
  useCallback,
} from "react";
import { useDebounceCallback } from "@react-hook/debounce";
import { AnyObjectSchema, ValidationError } from "yup";

import {
  ValidationContext,
  useOrCreateValidationContext,
  ValidatorCallbacks,
} from "./ValidationProvider";

export function useInputValidator<T>(
  schema: AnyObjectSchema,
  obj: T | null = null
) {
  type TKey = keyof T;

  // maintain an internal copy of obj so that compex property validations have access to other properties
  const [values, setValues] = useState(obj ? { ...obj } : ({} as T));
  useEffect(() => {
    setValues(obj ? { ...obj } : ({} as T));
  }, [obj, setValues]);

  const existingContext = useContext(ValidationContext);
  const context = useOrCreateValidationContext(existingContext);

  const {
    formErrorCount,
    setFormErrorCount,
    formIsTouched,
    setFormIsTouched,
    formIsDirty,
    setFormIsDirty,

    callbacks,
  } = context;

  const [errors, setErrors] = useState([] as ValidationError[]);
  const [previousErrors] = usePrevious(errors);
  const [touchedFields, setTouchedFields] = useState([] as TKey[]);
  const [dirtyFields, setDirtyFields] = useState([] as TKey[]);

  const propertyHasErrors = useCallback(
    (name: TKey) => errors.some((e) => e.path === name),
    [errors]
  );

  useEffect(() => {
    const errorCount = errors.flatMap((e) => e.errors).length;
    const previousErrorCount = (previousErrors || []).flatMap(
      (e) => e.errors
    ).length;
    if (errorCount !== previousErrorCount) {
      setFormErrorCount(
        (formErrorCount) => formErrorCount + errorCount - previousErrorCount
      );
    }
  }, [errors, formErrorCount, setFormErrorCount, previousErrors]);

  const validateFieldImpl = useCallback(
    async (name: TKey, value: T[TKey], reevaluateErrors: boolean = false) => {
      const newValues = { ...values, [name]: value } as unknown as T;
      setValues(newValues);
      try {
        await schema.validateAt(name as string, newValues);
        if (propertyHasErrors(name)) {
          setErrors((e) => e.filter((e) => e.path !== name));
        }
      } catch (e: unknown) {
        if (e instanceof ValidationError) {
          setErrors((errors) => [
            ...errors.filter((e) => e.path !== name),
            e as ValidationError,
          ]);
          return e;
        }
      } finally {
        if (reevaluateErrors) {
          // make sure other field errors are reevaluated to account for complex validations
          for (const field of errors
            .filter((e) => e.path !== name)
            .map((e) => e.path as TKey)) {
            validateFieldImpl(field, newValues[field]);
          }
        }
      }
    },
    [schema, setValues, setErrors, propertyHasErrors, values]
  );

  const validateField = useDebounceCallback(validateFieldImpl, 250);

  const registeredNames = useRef([] as TKey[]);
  const validatorCallbacks = useRef({} as ValidatorCallbacks);

  useEffect(() => {
    const cb = validatorCallbacks;
    callbacks.push(cb);

    return () => {
      callbacks.splice(callbacks.indexOf(cb), 1);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const reset = useCallback(() => {
    setTouchedFields([]);
    setDirtyFields([]);
    setErrors([]);
  }, [setTouchedFields, setDirtyFields, setErrors]);

  const validate = useCallback(async () => {
    const errors = [] as ValidationError[];
    for (const name of registeredNames.current) {
      const error = await validateFieldImpl(name, values[name]);
      if (error) {
        errors.push(error);
      }
    }
    return errors;
  }, [validateFieldImpl, values]);

  useEffect(() => {
    validatorCallbacks.current.reset = reset;
    validatorCallbacks.current.validate = validate;
  }, [reset, validate]);

  const resetContext = () => {
    setFormErrorCount(0);
    setFormIsTouched(false);
    setFormIsDirty(false);
    callbacks.forEach((cb) => {
      const reset = cb.current?.reset;
      reset && reset();
    });
  };

  const validateAll = async () => {
    let errors = [] as ValidationError[];
    for (const cb of callbacks) {
      const validate = cb.current?.validate;
      validate && (errors = [...errors, ...(await validate())]);
    }
    return errors;
  };

  const getClassName = (name: TKey, passThroughClassNames: string = "") => {
    return propertyHasErrors(name)
      ? `${passThroughClassNames} is-invalid`
      : passThroughClassNames;
  };

  const InputErrorMessage = ({ name }: { name: TKey }) => {
    // keep track of what properties are being validated
    if (!registeredNames.current.includes(name)) {
      registeredNames.current.push(name);
    }

    const messages = errors
      .filter((e) => e.path === name)
      .flatMap((e) => e.errors);

    return propertyHasErrors(name) ? (
      <>
        {messages.map((m, i) => (
          <p className="text-danger" key={i}>
            {m}
          </p>
        ))}
      </>
    ) : null;
  };

  const valueChanged = (name: TKey, value: T[TKey]) => {
    validateField(name, value, true);
  };

  const onChange =
    (name: TKey, handler: ChangeEventHandler<HTMLInputElement> | null = null) =>
    (e: ChangeEvent<HTMLInputElement>) => {
      handler && handler(e);
      // If T[TKey] is a number, this doesn't actually convert the string to a number.
      // But yup doesn't seem to mind, and that's what counts.
      valueChanged(name, e.target.value as unknown as T[TKey]);
      setFormIsDirty(true);
      if (!dirtyFields.some((f) => f === name)) {
        setDirtyFields([...dirtyFields, name]);
      }
    };

  // Some components return the selected element in the onChange function so
  // we have to create an onChange function that handles that
  const onChangeValue =
    (name: TKey, handler: ((value: any) => void) | null = null) =>
    (value: any) => {
      handler && handler(value);
      // If T[TKey] is a number, this doesn't actually convert the string to a number.
      // But yup doesn'tx seem to mind, and that's what counts.
      valueChanged(name, value as T[TKey]);
      setFormIsDirty(true);
      if (!dirtyFields.some((f) => f === name)) {
        setDirtyFields([...dirtyFields, name]);
      }
    };

  const onBlur = (name: TKey) => (e: FocusEvent<HTMLInputElement>) => {
    onBlurValue(name, e.target.value);
  };

  const onBlurValue = (name: TKey, value?: T[TKey] | string) => {
    setFormIsTouched(true);
    if (!touchedFields.some((f) => f === name)) {
      setTouchedFields([...touchedFields, name]);
    }
    validateField(name, (value || values[name]) as T[TKey]);
  };

  const fieldIsTouched = (name: TKey) => touchedFields.some((f) => f === name);
  const fieldIsDirty = (name: TKey) => dirtyFields.some((f) => f === name);
  const resetField = (name: TKey) => {
    setTouchedFields(touchedFields.filter((f) => f === name));
    setDirtyFields(dirtyFields.filter((f) => f === name));
    if (propertyHasErrors(name)) {
      setErrors((errors) => errors.filter((e) => e.path !== name));
    }
  };
  const resetLocalFields = () => {
    setTouchedFields([]);
    setDirtyFields([]);
    const errorCount = errors.flatMap((e) => e.errors).length;
    // update formErrorCount immediately instead of waiting for effect in case the owning component is about to be removed
    setFormErrorCount((formErrorCount) => formErrorCount - errorCount);
    setErrors([]);
  };

  return {
    valueChanged,
    onChange,
    onChangeValue,
    onBlur,
    onBlurValue,
    InputErrorMessage,
    getClassName,
    formErrorCount,
    formIsTouched,
    fieldIsTouched,
    formIsDirty,
    fieldIsDirty,
    resetContext,
    resetField,
    resetLocalFields,
    context,
    validateAll,
    propertyHasErrors,
  };
}

// provides previous value of given state
function usePrevious<T>(value: T): [T | undefined] {
  const ref = useRef<T>();
  useEffect(() => {
    ref.current = value;
  });
  return [ref.current];
}
