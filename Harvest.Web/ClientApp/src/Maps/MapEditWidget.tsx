import React, { useEffect, useRef, useState } from "react";
import WebMap from "@arcgis/core/WebMap";
import MapView from "@arcgis/core/views/MapView";
import Editor from "@arcgis/core/widgets/Editor";
import FieldElement from "@arcgis/core/form/elements/FieldElement.js";
import FormTemplate from "@arcgis/core/form/FormTemplate.js";
import CodedValueDomain from "@arcgis/core/layers/support/CodedValueDomain.js";

import FeatureLayer from "@arcgis/core/layers/FeatureLayer";
import BasemapToggle from "@arcgis/core/widgets/BasemapToggle";

// from: https://github.com/Esri/jsapi-resources/blob/main/esm-samples/jsapi-react/src/App.jsx
export const MapEditWidget = () => {
  console.log("Map.tsx");
  const mapDiv = useRef(null);

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

      const featureLayer = new FeatureLayer({
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

      view.when(() => {
        // when our view is loaded
        const editor = new Editor({
          view: view,
          label: "LABEL",
          layerInfos: [
            {
              layer: featureLayer,
              formTemplate: new FormTemplate({
                description: "",
                // expressionInfos: [],
                // preserveFieldValuesWhenHidden: false,
                title: "Edit Field Details",
                elements: [
                  // was having trouble with the autocast so this is my workaround
                  new FieldElement({
                    fieldName: "name", // fieldName has to match the case of the field name in the feature layer
                    label: "Name", // you can check that at Content -> select feature layer -> Data (at top) -> fields
                  }),
                  new FieldElement({
                    fieldName: "Crop",
                    label: "Crop",
                    domain: new CodedValueDomain({
                      // dropdown list of available crop values
                      // https://developers.arcgis.com/javascript/latest/api-reference/esri-layers-support-CodedValueDomain.html
                      // doesnt look like this supports dynamic values, but it's currently a dropdown anyways
                      codedValues: [
                        {
                          name: "Potato",
                          code: "potato",
                        },
                        {
                          name: "Tomato",
                          code: "Tomato",
                        },
                        {
                          name: "Corn",
                          code: "Corn",
                        },
                        {
                          name: "Celery",
                          code: "Celery",
                        },
                        {
                          name: "Carrot",
                          code: "Carrot",
                        },
                      ],
                    }),
                  }),
                  new FieldElement({
                    fieldName: "Details",
                    label: "Details",
                  }),
                ],
              }),
            },
          ],
        });

        // Add the widget to the view
        view.ui.add(editor, "top-right");
      });

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

        // query the features once webmap is loaded
        featureLayer
          .queryFeatures()
          .then((featureSet) => {
            console.log("features", featureSet.features);
            // then we can do things like:
            console.log(
              "featureSet.features[0].attributes: ",
              featureSet.features[0].attributes
            );
            // and get:
            // {
            //   Crop: "Cabbage",
            //   Details: null,
            //   OBJECTID: 2,
            //   Shape__Area: 176921.81640625,
            //   Shape__Length: 1728.6668691192322,
            //   name: "cabbages"
            // }
          })
          .catch((error) => {
            console.error("Error querying features: ", error);
          });
      });
    }
  }, [mapDiv]);

  return <div className="mapDiv" ref={mapDiv}></div>;
};
