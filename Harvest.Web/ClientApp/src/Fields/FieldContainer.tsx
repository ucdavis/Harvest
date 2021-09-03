import React from "react";

import {
  MapContainer,
  TileLayer,
  Popup,
  GeoJSON,
  FeatureGroup,
} from "react-leaflet";

// TODO: if we need to customize leaflet directly
// import L from 'leaflet';

import { EditControl } from "react-leaflet-draw";

import { EditField } from "./EditField";
import { FieldLayers } from "./FieldLayers";
import { FieldPopup } from "./FieldPopup";
import { Field, Project } from "../types";

interface Props {
  crops: string[];
  fields: Field[];
  project: Project;
  updateFields: (fields: Field[]) => void;
}

interface State {
  editFieldId: number | undefined;
}

// NOTE: leaflet-draw plugin is not compatible with react hooks so we use a full component here
export class FieldContainer extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      editFieldId: undefined,
    };
  }

  _addDefaultField = (e: any) => {
    const { layer } = e;
    console.log(e);
    console.log("creating field", this.props.fields);

    const newId = Math.max(...this.props.fields.map((f) => f.id), 0) + 1;
    const newField: Field = {
      id: newId,
      name: `field-${newId}`,
      crop: this.props.crops[0],
      details: "",
      geometry: layer.toGeoJSON().geometry,
    };

    this.props.updateFields([...this.props.fields, newField]);

    // immediately set the new field for editing
    this.setState({ editFieldId: newId });

    // immediately remove added layer because we are going to let react handle rendering layers
    this._editableFG?.removeLayer(layer);
  };

  _updateField = (field: Field) => {
    // TODO: can we get away without needing to spread copy?  do we need to totally splice/replace?
    const allItems = this.props.fields;
    const itemIndex = allItems.findIndex((a) => a.id === field.id);
    allItems[itemIndex] = {
      ...field,
    };

    this.props.updateFields([...allItems]);
  };

  _removeField = (field: Field) => {
    const itemsToKeep = this.props.fields.filter((w) => w.id !== field.id);
    this.props.updateFields(itemsToKeep);
  };

  render() {
    return (
      <div className="map-wrapper">
        <MapContainer
          style={{ height: window.innerHeight }}
          center={[38.5449, -121.7405]}
          zoom={13}
        >
          <TileLayer
            attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <FieldLayers project={this.props.project}></FieldLayers>
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
                <FieldPopup
                  field={field}
                  updateFieldId={this._updateFieldId}
                  removeField={this._removeField}
                ></FieldPopup>
              </Popup>
            </GeoJSON>
          ))}
        </MapContainer>
        {this._renderModal()}
      </div>
    );
  }

  _renderModal = () => {
    if (this.state.editFieldId) {
      const field = this.props.fields.find(
        (f) => f.id === this.state.editFieldId
      );

      if (field) {
        return (
          <EditField
            crops={this.props.crops}
            field={field}
            updateFieldId={this._updateFieldId}
            updateField={this._updateField}
          />
        );
      }
    }

    return null;
  };

  // This is used to trigger the modal to open
  _updateFieldId = (field?: Field) => {
    console.log(this.state.editFieldId);
    if (field) {
      this.setState({ editFieldId: field.id });
    } else {
      this.setState({ editFieldId: undefined });
    }
  };

  // our render feature group, only used for dynamic drawing
  // once drawing is finished, a field is added and react-leaflet takes over with rendering and control
  _editableFG: any = null;
  _onFeatureGroupReady = (reactFGref: any) => {
    this._editableFG = reactFGref;
  };
}
