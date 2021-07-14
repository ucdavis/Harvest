import * as yup from "yup";
import { SchemaOf } from "yup";
import { BlobFile, RequestInput, User } from "./types";

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
  uploaded: yup.boolean().required().isTrue() // files are only valid if they are done uploading
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
