import * as yup from "yup";
import { SchemaOf } from "yup";
import { BlobFile, RequestInput, User, WorkItem } from "./types";

export const investigatorSchema: SchemaOf<User> = yup
  .object()
  .shape({
    id: yup.number().required(),
    firstName: yup.string(),
    lastName: yup.string().required(),
    email: yup.string().required(),
    iam: yup.string().required(),
    kerberos: yup.string().required(),
    name: yup.string().required(),
    nameAndEmail: yup.string(),
  });

export const fileSchema: SchemaOf<BlobFile> = yup.object().shape({
  identifier: yup.string().required(),
  fileName: yup.string().required(),
  fileSize: yup.number().required(),
  contentType: yup.string().required(),
  uploaded: yup.boolean().required().isTrue(), // files are only valid if they are done uploading
  sasLink: yup.string()
});

export const requestSchema: SchemaOf<RequestInput> = yup.object().shape({
  id: yup.number().required(),
  start: yup.string().required(),
  end: yup.string().required(),
  crop: yup.string().required(),
  cropType: yup.string().required(),
  requirements: yup.string(),
  principalInvestigator: investigatorSchema,
  files: yup.array().of(fileSchema)
});

export const workItemSchema: SchemaOf<WorkItem> = yup.object().shape({
  id: yup.number().required().default(0),
  activityId: yup.number().required().integer(),
  type: yup.mixed().required().oneOf(["Acreage", "Equipment", "Other", "Labor"]),
  rateId: yup.number().required().default(0),
  rate: yup.number().required().positive(),
  description: yup.string().defined(),
  quantity: yup.number().required().positive(),
  unit: yup.string().defined(),
  total: yup.number().required().positive()
});