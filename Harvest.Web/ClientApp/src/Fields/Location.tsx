import React, { useMemo } from "react";
import { MapContainer, TileLayer, GeoJSON } from "react-leaflet";

import { getBoundingBox } from "../Util/Geography";

import { Field } from "../types";
import { LatLngBoundsExpression } from "leaflet";

interface Props {
  fields: Field[];
}

export const Location = (props: Props) => {
  const bounds: LatLngBoundsExpression = useMemo(() => {
    const bounds = getBoundingBox(props.fields.map((f) => f.geometry));
    return [
      [bounds.yMin, bounds.xMin],
      [bounds.yMax, bounds.xMax],
    ];
  }, [props.fields]);

  if (!props.fields || props.fields.length === 0) {
    return null;
  }

  return (
    <MapContainer style={{ height: 200 }} bounds={bounds}>
      <TileLayer
        attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
      {props.fields.map((field) => (
        <GeoJSON key={`field-${field.id}`} data={field.geometry} />
      ))}
    </MapContainer>
  );
};
