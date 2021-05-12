import React from "react";

import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  GeoJSON,
  FeatureGroup,
} from "react-leaflet";

// TODO: if we need to customize leaflet directly
// import L from 'leaflet';

import { EditControl } from "react-leaflet-draw";

import { Field } from "../types";

interface Props {
  fields: Field[];
  updateFields: (fields: Field[]) => void;
}

// NOTE: leaflet-draw plugin is not compatible with react hooks so we use a full component here
export class FieldContainer extends React.Component<Props> {
  _addDefaultField = (e: any) => {
    const { layer } = e;
    console.log(e);
    console.log("creating field", this.props.fields);

    const newId = Math.max(...this.props.fields.map((f) => f.id), 0) + 1;
    const newField: Field = {
      id: newId,
      name: "Default",
      crop: "Corn",
      details: "",
      geometry: layer.toGeoJSON(), // TODO: for now just a hardcoded polygon
    };

    this.props.updateFields([...this.props.fields, newField]);

    // immediately remove added layer because we are going to let react handle rendering layers
    this._editableFG?.removeLayer(layer);
  };

  render() {
    return (
      <div>
        <button onClick={this._addDefaultField}>Add Field</button>
        <MapContainer
          style={{ height: window.innerHeight }}
          center={[38.5449, -121.7405]}
          zoom={13}
        >
          <TileLayer
            attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <FeatureGroup
            ref={(reactFGref) => {
              this._onFeatureGroupReady(reactFGref);
            }}
          >
            <EditControl
              position="topright"
              onCreated={this._addDefaultField}
              draw={{
                polygon: {
                  allowIntersection: false,
                  showArea: true,
                },
                rectangle: true,
                polyline: false,
                circle: false,
                circlemarker: false,
                marker: false,
              }}
            />
          </FeatureGroup>
          {this.props.fields.map((field) => (
            <GeoJSON key={`field-${field.id}`} data={field.geometry}>
              <Popup>
                Some content here
              </Popup>
            </GeoJSON>
          ))}

          <Marker position={[38.5449, -121.7405]}>
            <Popup>
              A pretty CSS3 popup. <br /> Easily customizable.
            </Popup>
          </Marker>
        </MapContainer>
      </div>
    );
  }

  // our render feature group, only used for dynamic drawing
  // once drawing is finished, a field is added and react-leaflet takes over with rendering and control
  _editableFG: any = null;
  _onFeatureGroupReady = (reactFGref: any) => {
    this._editableFG = reactFGref;
  };
}
