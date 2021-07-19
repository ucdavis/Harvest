import React, { useEffect, useState } from "react";
import { LayersControl, GeoJSON, Popup, LayerGroup } from "react-leaflet";
import { Field, Project } from "../types";

interface Props {
  project: Project;
}

export const FieldLayers = (props: Props) => {
  const [fields, setFields] = useState<Field[]>([]);

  // load fields for active projects
  useEffect(() => {
    const cb = async () => {
      const startDate = new Date(props.project.start);
      const endDate = new Date(props.project.end);

      const activeFieldResponse = await fetch(
        `/Field/Active?start=${startDate.toLocaleDateString()}&end=${endDate.toLocaleDateString()}`
      );

      if (activeFieldResponse.ok) {
        const activeFields: Field[] = await activeFieldResponse.json();
        setFields(activeFields);
      }
    };

    cb();
  }, [props.project.end, props.project.start]);

  if (fields.length === 0) {
    return null;
  }

  return (
    <LayersControl position="topleft">
      <LayersControl.Overlay checked name="Active project fields">
        <LayerGroup>
          {fields.map((field) => (
            <GeoJSON
              key={`field-${field.id}`}
              data={field.geometry}
              style={{ color: "red" }}
            >
              <Popup>{field.crop}</Popup>
            </GeoJSON>
          ))}
        </LayerGroup>
      </LayersControl.Overlay>
    </LayersControl>
  );
};
