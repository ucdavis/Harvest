import React from "react";
import { Field } from "../types";

interface Props {
  field: Field;
  removeField: (field: Field) => void;
}

export const FieldPopup = (props: Props) => {
  return (
    <div>
      <h2>{props.field.name}</h2>
      <h2>{props.field.crop}</h2>
      <p>{props.field.details}</p>
      <button
        className="btn btn-danger"
        onClick={() => props.removeField(props.field)}
      >
        Remove
      </button>
    </div>
  );
};
