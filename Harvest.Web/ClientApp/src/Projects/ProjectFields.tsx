import React, { useEffect, useMemo, useState } from "react";
import { GeoJSON, MapContainer, TileLayer, Popup  } from "react-leaflet";

import { getBoundingBox } from "../Util/Geography";

import { Field } from "../types";
import { LatLngBoundsExpression } from "leaflet";

export const ProjectFields = () => {
  const [fields, setFields] = useState<Field[]>([]);

  useEffect(() => {
    const getFields = async () => {
      const response = await fetch("/Project/GetFiles");

      if (response.ok) {
        const result = await response.json();
        setFields(result);
      }
    };

    getFields();
  }, []);

  const bounds: LatLngBoundsExpression = useMemo(() => {
    const bounds = getBoundingBox(fields.map((f) => f.geometry));
    return [
      [bounds.yMin, bounds.xMin],
      [bounds.yMax, bounds.xMax],
    ];
  }, [fields]);

  if (!fields || fields.length === 0) {
    return null;
  }

  return (
    <MapContainer style={{ height: 600 }} bounds={bounds}>
      <TileLayer
        attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
      {fields.map((field, i) => (
        <GeoJSON key={`field-${i}`} data={field.geometry} />
      ))}
    </MapContainer>
  );
};
