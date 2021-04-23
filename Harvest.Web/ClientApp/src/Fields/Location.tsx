import React, { useMemo } from "react";
import { MapContainer, TileLayer, GeoJSON } from "react-leaflet";
import { Field } from "../types";

interface Props {
  fields: Field[];
}

export const Location = (props: Props) => {
  // TODO: calculate center based on fields, or perhaps use bounds[] to set map just to include all fields

  // TODO: use field data to determine center and then pass props.fields into dependency array
  const center: any = useMemo(() => {
    return [38.53441977139934, -121.7362572473126];
  }, []);

  return (
    <MapContainer style={{ height: 200 }} center={center} zoom={16}>
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
