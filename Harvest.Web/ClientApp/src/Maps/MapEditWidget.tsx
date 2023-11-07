import React, { useEffect, useRef } from "react";
import WebMap from "@arcgis/core/WebMap";
import Bookmarks from "@arcgis/core/widgets/Bookmarks";
import Expand from "@arcgis/core/widgets/Expand";
import MapView from "@arcgis/core/views/MapView";
import Editor from "@arcgis/core/widgets/Editor";
import FieldElement from "@arcgis/core/form/elements/FieldElement.js";
import FormTemplate from "@arcgis/core/form/FormTemplate.js";

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
                  new FieldElement({
                    fieldName: "Name",
                    label: "Name",
                  }),
                  new FieldElement({
                    fieldName: "Crop",
                    label: "Crop",
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
      // bonus - how many bookmarks in the webmap?
      webmap.when(() => {
        if (webmap.bookmarks && webmap.bookmarks.length) {
          console.log("Bookmarks: ", webmap.bookmarks.length);
        } else {
          console.log("No bookmarks in this webmap.");
        }
      });
    }
  }, [mapDiv]);

  return <div className="mapDiv" ref={mapDiv}></div>;
};
