import React from "react";
import { useFormik, FormikConfig, FormikValues } from "formik";

//hack to get return type of useFormic()
class UseFormikWrapper<T> {
  // wrapped has no explicit return type so we can infer it
  wrapped(e: FormikConfig<T>) {
    // renaming prevents react from recognizing this as a hook to avoid runtime error
    const refUseFormik = useFormik;
    return refUseFormik<T>(e);
  }
}
export type FormikState<T> = ReturnType<UseFormikWrapper<T>['wrapped']>;


interface ValidationMessageProps<T> {
  formik: FormikState<T>;
  name: string;
}


export function ValidationErrorMessage<T>(props: ValidationMessageProps<T>) {
  const { formik, name } = props;
  const meta = formik.getFieldMeta(name);

  return meta.touched && meta.error !== undefined && meta.error !== ""
    ? (<p className="text-danger">{meta.error}</p>)
    : (null);
}

export function getInputValidityStyle<T>(formik: FormikState<T>, name: string) {
  const meta = formik.getFieldMeta(name);
  return meta.touched && meta.error !== undefined && meta.error !== "" && "is-invalid";
}

