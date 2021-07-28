import React from "react";
import { Field } from "../types";

interface Props {
  field: Field;
  updateFieldId: (field: Field) => void;
  removeField: (field: Field) => void;
}

export const FieldPopup = (props: Props) => {
  return (
    <div>
      <div className="tooltip-title">
        <h3>{props.field.name}</h3>
      </div>
      <div className="tooltip-description">
        <p>{props.field.crop}</p>
        <p>{props.field.details}</p>
      </div>
      <div className="row justify-content-between">
        <button
          className="btn btn-link btn-sm mt-5"
          onClick={() => props.updateFieldId(props.field)}
        >
          Edit
        </button>
        <button
          className="btn btn-link btn-sm mt-5"
          onClick={() => props.removeField(props.field)}
        >
          Remove
        </button>
      </div>
    </div>
  );
};
