import React, { useEffect, useRef, useState } from "react";
import { Button, Input } from "reactstrap";

import WebMap from "@arcgis/core/WebMap";
import MapView from "@arcgis/core/views/MapView";
import FeatureLayer from "@arcgis/core/layers/FeatureLayer";
import BasemapToggle from "@arcgis/core/widgets/BasemapToggle";
import Graphic from "@arcgis/core/Graphic";
import Polygon from "@arcgis/core/geometry/Polygon";
import Layer from "@arcgis/core/layers/Layer";

// the attributes of the feature layer ()
interface FieldAttributes {
  name: string; // field name, not display name (i messed up the casing but you can't rename the field name)
  Crop: string;
  Details: string;
}

// from: https://github.com/Esri/jsapi-resources/blob/main/esm-samples/jsapi-react/src/App.jsx
export const MapEditReactState = () => {
  console.log("Map.tsx");
  const mapDiv = useRef(null);
  const [features, setFeatures] = useState<Graphic[]>([]);
  const [viewState, setViewState] = useState<MapView>();

  const [newAttributes, setNewAttributes] = useState<FieldAttributes>({
    name: "New Attributes Test",
    Crop: "Potato",
    Details: "Created from React",
  });

  const addFeature = () => {
    if (!viewState) {
      console.error("View state not set");
      return;
    }
    const layer: Layer = viewState.map.findLayerById("fieldsLayer");
    if (!layer) {
      console.error("Feature layer not found");
      return;
    }
    // have to be sure that the layer is a feature layer to do this
    const featureLayer = layer as FeatureLayer;

    const newFeature = new Graphic({
      geometry: new Polygon({
        rings: [
          [
            [-121.80148, 38.568554],
            [-121.80148, 38.568554 - 0.01],
            [-121.80148 + 0.01, 38.568554 - 0.01],
            [-121.80148 + 0.01, 38.568554],
            [-121.80148, 38.568554],
          ],
        ],
        spatialReference: { wkid: 4326 },
      }),
      attributes: newAttributes,
    });

    featureLayer
      .applyEdits({ addFeatures: [newFeature] })
      .then((res) => {
        if (res.addFeatureResults.length > 0) {
          console.log("Feature added successfully");
          newFeature.attributes.objectId = res.addFeatureResults[0].objectId;
          setFeatures([...features, newFeature]);
        }
      })
      .catch((error) => {
        console.error("Error adding feature: ", error);
      });
  };

  const deleteFeature = () => {
    if (!viewState) {
      console.error("View state not set");
      return;
    }
    const layer: Layer = viewState.map.findLayerById("fieldsLayer");
    if (!layer) {
      console.error("Feature layer not found");
      return;
    }
    // have to be sure that the layer is a feature layer to do this
    const featureLayer = layer as FeatureLayer;

    // just delete the most recent feature for now
    const feature = features[features.length - 1];

    featureLayer
      .applyEdits({ deleteFeatures: [feature] })
      .then((res) => {
        if (res.deleteFeatureResults.length > 0) {
          console.log("Feature deleted successfully");
          const featureIndex = features.findIndex(
            (f) => f.getObjectId() === feature.getObjectId()
          );
          setFeatures(features.splice(featureIndex, 1));
        }
      })
      .catch((error) => {
        console.error("Error deleting feature: ", error);
      });
  };

  useEffect(() => {
    if (mapDiv.current) {
      /**
       * Initialize application
       */
      // https://developers.arcgis.com/javascript/latest/api-reference/esri-WebMap.html
      const webmap = new WebMap({
        portalItem: {
          // the unique id for your map
          // you can access this by going to arcgis -> content -> my content -> map and copy the id on the right (or from url)
          id: "8e276b0a2b6b49b88a1ea459d91f5fd3",
        },
        basemap: "hybrid", // https://developers.arcgis.com/rest/basemap-styles/#arcgis-styles
      });

      const view = new MapView({
        container: mapDiv.current,
        map: webmap,
        center: [-121.748, 38.54],
        zoom: 13,
      });
      setViewState(view);

      const featureLayer = new FeatureLayer({
        id: "fieldsLayer",
        portalItem: {
          // the feature layer is not added to the map in ArcGIS Online, but is overlayed here
          id: "66289d4facfb4932a5b1d91db8792c4f",
        },
      });
      webmap.add(featureLayer);

      // https://developers.arcgis.com/javascript/latest/api-reference/esri-widgets-BasemapToggle.html
      let basemapToggle = new BasemapToggle({
        view: view,
        nextBasemap: "osm", // Allows for toggling to the openstreetmaps (non-satellite )
      });
      view.ui.add(basemapToggle, "top-left");

      // once the whole webmap is loaded
      webmap.when(() => {
        if (webmap.allLayers && webmap.allLayers.length) {
          console.log("All layers: ", webmap.allLayers.length);
        } else {
          console.log("No layers in this webmap");
        }
        if (webmap.allTables && webmap.allTables.length) {
          console.log("All tables: ", webmap.allTables.length);
        } else {
          console.log("No tables in this webmap");
        }
      });

      featureLayer
        .queryFeatures()
        .then((featureSet) => {
          console.log("features", featureSet.features);
          // then we can do things like:
          // console.log(
          //   "featureSet.features[0].attributes: ",
          //   featureSet.features[0].attributes
          // );
          // and get:
          // {
          //   Crop: "Cabbage",
          //   Details: null,
          //   OBJECTID: 2,
          //   Shape__Area: 176921.81640625,
          //   Shape__Length: 1728.6668691192322,
          //   name: "cabbages"
          // }

          // and also:
          setFeatures(featureSet.features);
        })
        .catch((error) => {
          console.error("Error querying features: ", error);
        });
    }
  }, [mapDiv, newAttributes]);

  return (
    <>
      <Button onClick={addFeature}>Add Feature</Button>
      <Button onClick={deleteFeature}>Delete Feature</Button>
      <Input></Input>
      <div className="mapDiv" ref={mapDiv}></div>
    </>
  );
};
