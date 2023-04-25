import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { LayersControl, GeoJSON, Popup, LayerGroup } from "react-leaflet";
import { Field, Project, CommonRouteParams } from "../types";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

interface Props {
  project: Project;
}

export const FieldLayers = (props: Props) => {
  const [fields, setFields] = useState<Field[]>([]);
  const { team } = useParams<CommonRouteParams>();
  const getIsMounted = useIsMounted();

  // load fields for active projects
  useEffect(() => {
    const cb = async () => {
      const startDate = new Date(props.project.start);
      const endDate = new Date(props.project.end);
      const projectId = props.project.id;

      const activeFieldResponse = await authenticatedFetch(
        `/api/${team}/Field/Active?start=${startDate.toLocaleDateString()}&end=${endDate.toLocaleDateString()}&projectId=${projectId}`
      );

      if (activeFieldResponse.ok) {
        const activeFields: Field[] = await activeFieldResponse.json();
        getIsMounted() && setFields(activeFields);
      }
    };

    cb();
  }, [
    props.project.end,
    props.project.start,
    getIsMounted,
    props.project.id,
    team,
  ]);

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
