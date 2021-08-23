import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { GeoJSON, MapContainer, TileLayer, Popup } from "react-leaflet";
import { Project } from "../types";
import { getBoundingBox } from "../Util/Geography";
import { LatLngBoundsExpression } from "leaflet";

interface ProjectField {
  id: number;
  name: string;
  crop: string;
  details: string;
  location: GeoJSON.Polygon;
  project: Project;
  projectId: number;
}

export const ProjectFields = () => {
  const [fields, setFields] = useState<ProjectField[]>([]);

  useEffect(() => {
    const getFields = async () => {
      const response = await fetch("/Project/GetFields");

      if (response.ok) {
        const result = await response.json();
        console.log(result);
        setFields(result);
      }
    };

    getFields();
  }, []);

  const bounds: LatLngBoundsExpression = useMemo(() => {
    const bounds = getBoundingBox(fields.map((f) => f.location));
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
        <GeoJSON key={`field-${i}`} data={field.location}>
          <Popup>
            <div>
              <div className="tooltip-title">
                <h3>{field.project.name}</h3>
              </div>
              <div className="tooltip-description">
                <p>Crops: {field.crop}</p>
                <p>{field.details}</p>
                <p>Start Date: {new Intl.DateTimeFormat('en-US').format(new Date(field.project.start))}</p>
                <p>End Date: {new Intl.DateTimeFormat('en-US').format(new Date(field.project.end))}</p>
                <Link to={`/project/details/${field.projectId}`}>
                  Project Details
                </Link>
                <br />
                <Link to={`/expense/entry/${field.projectId}`}>
                  Project Expenses
                </Link>
              </div>
            </div>
          </Popup>
        </GeoJSON>
      ))}
    </MapContainer>
  );
};