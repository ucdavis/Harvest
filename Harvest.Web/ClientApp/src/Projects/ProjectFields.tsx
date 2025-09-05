import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { GeoJSON, MapContainer, TileLayer, Popup } from "react-leaflet";
import { Project, CommonRouteParams } from "../types";
import { getBoundingBox } from "../Util/Geography";
import { LatLngBoundsExpression } from "leaflet";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

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
  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const getFields = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/GetFields`
      );

      if (response.ok) {
        const result = await response.json();
        getIsMounted() && setFields(result);
      }
    };

    getFields();
  }, [getIsMounted, team]);

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
                <p>
                  Start Date:{" "}
                  {new Intl.DateTimeFormat("en-US").format(
                    new Date(field.project.start)
                  )}
                </p>
                <p>
                  End Date:{" "}
                  {new Intl.DateTimeFormat("en-US").format(
                    new Date(field.project.end)
                  )}
                </p>
                <Link to={`/${team}/project/details/${field.projectId}`}>
                  Project Details
                </Link>
                <br />
                <Link to={`/${team}/expense/entry/${field.projectId}`}>
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
