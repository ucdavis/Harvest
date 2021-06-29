import React, { useMemo } from "react";
import { useFormik, FormikConfig, FormikValues } from "formik";

//hack to get return type of useFormic()
class FormikBagInferer<T> {
  // wrapped has no explicit return type so we can infer it
  wrapped(e: FormikConfig<T>) {
    // renaming prevents react from recognizing this as a hook to avoid runtime error
    const refUseFormik = useFormik;
    return refUseFormik<T>(e);
  }
}
export type RootFormikBag<T> = ReturnType<FormikBagInferer<T>['wrapped']>;

// EXAMPLE: path of root.values.activities[0].workItems[1] will be "activities.0.workitems.1"
const getPath = (from: object, to: object) => {
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

//wrapper hook so that, most of the time, we'll only have to deal with FormikBag
export function useWrappedFormik<Values extends FormikValues = FormikValues>(props: FormikConfig<Values>) {
  const rootFormikBag = useFormik(props);

  const rootGetValues = (root: RootFormikBag<Values>) => root.values;
  
  const getNestedBag = <TNested extends object>(getValues: (values: Values) => TNested): FormikBag<Values, TNested> => {
    const newGetValues = (nestedRoot: RootFormikBag<Values>) => getValues(rootGetValues(nestedRoot));
    const newGetNestedBag = <TNextNested extends object>(nextGetValues: (values: TNested) => TNextNested): FormikBag<Values, TNextNested> =>
      getNestedBag<TNextNested>(rootValues => nextGetValues(getValues(rootValues)))
    const path = getPath(rootGetValues(rootFormikBag), newGetValues(rootFormikBag));
    return new FormikBag<Values, TNested>(rootFormikBag, newGetValues, newGetNestedBag, path);
  }


  const formikBag = new FormikBag(rootFormikBag, rootGetValues, getNestedBag, "");
  return formikBag;
}

//wrapper to help reduce a bunch of boilerplate when dealing with nested objects
export class FormikBag<TRoot extends object, TNested extends object = TRoot> implements Pick<RootFormikBag<TRoot>, "getFieldMeta"> {
  constructor(root: RootFormikBag<TRoot>, getValues: (root: RootFormikBag<TRoot>) => TNested,
    getNestedBag: <TNextNested extends object> (getValues: (values: TNested) => TNextNested) => FormikBag<TRoot, TNextNested>,
    path: string) {
    this.root = root;
    this.getValues = getValues;
    this.path = path;
    this.getNestedBag = getNestedBag;
  }

  private getValues: (root: RootFormikBag<TRoot>) => TNested;
  root: RootFormikBag<TRoot>;
  path: string;
  fullPath(name: string) { return !this.path || this.path === "" ? name : `${this.path}.${name}`; }
  getNestedBag: <TNextNested extends object>(getValues: (values: TNested) => TNextNested) => FormikBag<TRoot, TNextNested>;

  get handleBlur() { return this.root.handleBlur }
  get handleChange() { return this.root.handleChange }
  get handleReset() { return this.root.handleReset }
  get handleSubmit() { return this.root.handleSubmit }
  get resetForm() { return this.root.resetForm }
  get setErrors() { return this.root.setErrors }
  get setFormikState() { return this.root.setFormikState }
  setFieldTouched(field: string, touched?: boolean | undefined, shouldValidate?: boolean | undefined) { return this.root.setFieldTouched(this.fullPath(field), touched, shouldValidate); }
  setFieldValue(field: string, value: any, shouldValidate?: boolean | undefined) { return this.root.setFieldValue(this.fullPath(field), value, shouldValidate); }
  setFieldError(field: string, value: string | undefined) { return this.root.setFieldError(this.fullPath(field), value); }
  get setStatus() { return this.root.setStatus }
  get setSubmitting() { return this.root.setSubmitting }
  get setTouched() { return this.root.setTouched }
  get setValues() { return this.root.setValues }
  get submitForm() { return this.root.submitForm }
  get validateForm() { return this.root.validateForm }
  validateField(name: string) { this.root.validateField(this.fullPath(name)) }
  get isValid() { return this.root.isValid }
  get dirty() { return this.root.dirty }
  unregisterField(name: string) { this.root.unregisterField(this.fullPath(name)) }
  registerField(name: string, { validate }: any) { this.registerField(this.fullPath(name), { validate }); }
  getFieldProps(name: string) { return this.root.getFieldProps(this.fullPath(name)) }
  getFieldMeta(name: string) { return this.root.getFieldMeta(this.fullPath((name))) }
  getFieldHelpers(name: string) { this.root.getFieldHelpers(this.fullPath(name)) }
  get validateOnBlur() { return this.root.validateOnBlur }
  get validateOnChange() { return this.root.validateOnChange }
  get validateOnMount() { return this.root.validateOnMount }
  get values(): TNested { return this.getValues(this.root); }
  get errors() { return this.root.errors }
  get touched() { return this.root.touched }
  get isSubmitting() { return this.root.isSubmitting }
  get isValidating() { return this.root.isValidating }
  get status() { return this.root.status }
  get submitCount() { return this.root.submitCount }
}



interface ValidationMessageProps<T> {
  formik: Pick<RootFormikBag<T>, "getFieldMeta">;
  name: string;
}


export function ValidationErrorMessage<T>(props: ValidationMessageProps<T>) {
  const { formik, name } = props;
  const meta = formik.getFieldMeta(name);

  return meta.touched && meta.error !== undefined && meta.error !== ""
    ? (<p className="text-danger">{meta.error}</p>)
    : (null);
}

export function getInputValidityStyle<T>(formik: Pick<RootFormikBag<T>, "getFieldMeta">, name: string) {
  const meta = formik.getFieldMeta(name);
  return meta.touched && meta.error !== undefined && meta.error !== "" && "is-invalid";
}

