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
      geometry: polyGeoJson, // TODO: for now just a hardcoded polygon
    };

    props.updateFields([...props.fields, newField]);
  };
  return (
    <div>
      <button onClick={addDefaultField}>Add Sample Field</button>
      (Eventually this will be map where you can set fields)
    </div>
  );
};

const polyGeoJson: GeoJSON.FeatureCollection = {
  type: "FeatureCollection",
  features: [
    {
      type: "Feature",
      geometry: {
        type: "Polygon",
        coordinates: [
          [
            [-121.7362572473126, 38.53441977139934, 0],
            [-121.7355166365403, 38.53441178558781, 0],
            [-121.7354814725578, 38.53455895851263, 0],
            [-121.7362224219185, 38.53454446823565, 0],
            [-121.7362572473126, 38.53441977139934, 0],
          ],
        ],
      },
      properties: {
        name: "Corn",
      },
    },
  ],
};
