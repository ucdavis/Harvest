import React, { useEffect, useState } from "react";

import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from "reactstrap";

import { Field } from "../types";

interface Props {
  crops: string[];
  field: Field;
  updateFieldId: (field?: Field) => void;
  updateField: (field: Field) => void;
}

export const EditField = (props: Props) => {
  // Render modal if field is not undefined
  const update = () => {
    if (props.field) {
      //   props.saveFieldChanges({ ...props.field });
      props.updateFieldId(undefined);
    }
  };

  return (
    <div>
      <Modal isOpen={props.field != undefined}>
        <ModalHeader>Field #{props.field.id}</ModalHeader>
        <ModalBody>
          <form onSubmit={(e) => e.preventDefault()}>
            <div className="form-group">
              <label htmlFor="fieldName">Field Name</label>
              <input
                required
                type="text"
                className="form-control"
                id="fieldName"
                aria-describedby="fieldNameHelp"
                value={props.field.name}
                onChange={(e) =>
                  props.updateField({ ...props.field, name: e.target.value })
                }
              />
              <small id="fieldNameHelp" className="form-text text-muted">
                A short, useful name to refer to this field later by
              </small>
            </div>
            <div className="form-group">
              <label htmlFor="crop">Crop</label>
              <select
                className="form-control"
                id="crop"
                defaultValue={props.field.crop}
                onChange={(e) =>
                  props.updateField({
                    ...props.field,
                    crop: e.target.value,
                  })
                }
              >
                {props.crops.map((crop) => (
                  <option key={crop}>{crop}</option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label htmlFor="exampleFormControlTextarea1">Description</label>
              <textarea
                className="form-control"
                id="exampleFormControlTextarea1"
                placeholder="Optional description and notes specific to this field"
                rows={3}
                value={props.field.details}
                onChange={(e) =>
                  props.updateField({ ...props.field, details: e.target.value })
                }
              ></textarea>
            </div>
          </form>
        </ModalBody>
        <ModalFooter>
          <Button color="danger" onClick={update}>
            Cancel
          </Button>
          <Button color="primary" onClick={update}>
            Confirm
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};
