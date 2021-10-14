import { RateType } from "./types";
import { ValidatorOptions } from "use-input-validator";

export const ActivityRateTypes: RateType[] = ["Labor", "Equipment", "Other"];

export const validatorOptions: ValidatorOptions = {
  classNameErrorInput: "is-invalid",
  classNameErrorMessage: "text-danger",
};
