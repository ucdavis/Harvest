import * as yup from "yup";
import { SchemaOf } from "yup";
import { RequestInput, User, WorkItem } from "./types";

export const investigatorSchema: SchemaOf<User> = yup
  .object()
  .shape({
    id: yup.number().required(),
    firstName: yup.string().required(),
    lastName: yup.string().required(),
    email: yup.string().required(),
    iam: yup.string().required(),
    kerberos: yup.string().required(),
    name: yup.string().required(),
    nameAndEmail: yup.string(),
  });

export const requestSchema: SchemaOf<RequestInput> = yup.object().shape({
  id: yup.number().required(),
  start: yup.string().required(),
  end: yup.string().required(),
  crop: yup.string().required(),
  cropType: yup.string().required(),
  requirements: yup.string(),
  principalInvestigator: investigatorSchema,
});

export const workItemSchema: SchemaOf<WorkItem> = yup.object().shape({
  id: yup.number().required().default(0),
  activityId: yup.number().required().integer(),
  type: yup.mixed().required().oneOf(["Acreage", "Equipment", "Other", "Labor"]),
  rateId: yup.number().required().default(0),
  rate: yup.number().required().positive(),
  description: yup.string().defined(),
  quantity: yup.number().required().positive(),
  unit: yup.string().required().nullable(),
  total: yup.number().required().positive()
});