import React from "react";

import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";

export const Map = () => {
  return (
    <div>
      <MapContainer
        style={{ height: window.innerHeight }}
        center={[38.5449, -121.7405]}
        zoom={13}
      >
        <TileLayer
          attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Marker position={[38.5449, -121.7405]}>
          <Popup>
            A pretty CSS3 popup. <br /> Easily customizable.
          </Popup>
        </Marker>
      </MapContainer>
    </div>
  );
};
