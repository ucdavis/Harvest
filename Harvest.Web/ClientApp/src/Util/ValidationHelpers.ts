import { AnySchema, SchemaOf, ValidationError } from "yup";

export interface ValidateOptions {
  strict?: boolean;
  abortEarly?: boolean;
  stripUnknown?: boolean;
  recursive?: boolean;
  context?: object;
}

export async function checkValidity<T extends object>(value: T, schema: AnySchema, options?: ValidateOptions): Promise<string[]> {
  try {
    await schema.validate(value, options || { abortEarly: false });
  } catch (err) {
    if (err instanceof ValidationError) {
      return err.errors;
    }
  }
  return [];
};