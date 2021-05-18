import React from "react";
import { Field } from "../types";

interface Props {
  field: Field;
  removeField: (field: Field) => void;
}

export const FieldPopup = (props: Props) => {
  return (
    <div>
      <p>
        <b>{props.field.name}</b>
      </p>
      <p>{props.field.crop}</p>
      <p>{props.field.details}</p>
      <button
        className="btn btn-primary btn-sm mt-4"
        onClick={() => props.removeField(props.field)}
      >
        Remove
      </button>
    </div>
  );
};
