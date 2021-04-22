import { Field } from "../types";

interface Props {
  fields: Field[];
  updateFields: (fields: Field[]) => void;
}

export const FieldContainer = (props: Props) => {
  const addDefaultField = () => {
    const newId = Math.max(...props.fields.map((f) => f.id), 0) + 1;
    const newField: Field = {
      id: newId,
      name: "Default",
      crop: "Corn",
      details: "",
      geometry: {},
    };

    props.updateFields([...props.fields, newField]);
  };
  return (
    <div>
      <button onClick={addDefaultField}>
        Add Sample Field
      </button>
      (Eventually this will be map where you can set fields)
    </div>
  );
};
